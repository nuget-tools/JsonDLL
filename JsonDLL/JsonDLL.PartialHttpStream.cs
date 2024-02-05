// https://codereview.stackexchange.com/questions/70679/seekable-http-range-stream
using System.IO;
using System.Net;
using System;
using static JsonDLL.Util;
using CurlThin;
using CurlThin.Enums;
using CurlThin.Helpers;
using CurlThin.SafeHandles;
using System.Text;
using System.Web.Routing;

namespace JsonDLL;

#if true
class PartialHTTPStream : Stream, IDisposable
{
    Stream stream;
    WebResponse resp;
    //int cacheRemaining = 0;
    //const int cachelen = 1024;

    public string Url { get; private set; }
    public override bool CanRead { get { return true; } }
    public override bool CanWrite { get { return false; } }
    public override bool CanSeek { get { return true; } }

    long position = 0;
    public override long Position
    {
        get { return position; }
        set
        {
            position = value;
            //Log($"Seek {value}");
        }
    }

    long? length;
    public override long Length
    {
        get
        {
            if (length == null)
            {
                HttpWebRequest req = null;
                try
                {
                    req = HttpWebRequest.CreateHttp(Url);
                    req.Method = "HEAD";
                    req.AllowAutoRedirect = true;
                    length = req.GetResponse().ContentLength;
                }
                finally
                {
                    if (req != null)
                    {
                        // 連続呼び出しでエラーになる場合があるのでその対策
                        req.Abort();
                    }
                }

            }
            return length.Value;
        }
    }

    public PartialHTTPStream(string Url) { this.Url = Url; }

    public override void SetLength(long value)
    { throw new NotImplementedException(); }

    public override int Read(byte[] buffer, int offset, int count)
    {
        //Log(new { offset = offset, count = count, Position = Position, Length = Length });
        HttpWebRequest req = null;
        try
        {
            req = HttpWebRequest.CreateHttp(Url);
            //Log("(1)");
            //req.AddRange(Position, Position + count - 1);
            req.AddRange(Position);
            req.AllowAutoRedirect = true;
            //Log("(2)");
            try
            {
                //Log("(3)");
                resp = req.GetResponse();
                //Log("(4)");
            }
            catch (WebException ex)
            {
                throw ex;
            }
            //Log("(5)");
            int rest = count;
            int nread = 0;
            using (Stream stream = resp.GetResponseStream())
            {
                while (true)
                {
                    int len = stream.Read(buffer, offset, rest);
                    if (len == 0) break;
                    nread += len;
                    offset += len;
                    rest -= len;
                }
            }
            //Log(nread, "nread");
            //Log("(6)");
            return nread;
        }
        finally
        {
            if (req != null)
            {
                // 連続呼び出しでエラーになる場合があるのでその対策
                req.Abort();
            }
        }

    }

    public override void Write(byte[] buffer, int offset, int count)
    { throw new NotImplementedException(); }

    public override long Seek(long pos, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.End:
                Position = Length + pos;
                break;
            case SeekOrigin.Begin:
                Position = pos;
                break;
            case SeekOrigin.Current:
                Position += pos;
                break;
        }
        return Position;
    }

    public override void Flush() { }

    new void Dispose()
    {
        base.Dispose();
        if (stream != null)
        {
            stream.Dispose();
            stream = null;
        }
        if (resp != null)
        {
            resp.Dispose();
            resp = null;
        }
    }
}
#else
public class PartialHTTPStream : Stream, IDisposable
{
    private const int CacheLen = 1024*1024;
    CURLcode global = CurlNative.Init();

    // Cache for short requests.
    private readonly byte[] cache;
    private readonly int cacheLen;
    private Stream? stream;
    private WebResponse? response;
    //private long position = 0;
    private long? length;
    private long cachePosition;
    private int cacheCount;

     static PartialHTTPStream()
    {
        LibCurlLoader.Initialize();
    }
    public PartialHTTPStream(string url, int cacheLen = CacheLen)
    {
        if (string.IsNullOrEmpty(url))
            throw new ArgumentException("url empty");
        if (cacheLen <= 0)
            throw new ArgumentException("cacheLen must be greater than 0");

        Url = url;
        this.cacheLen = cacheLen;
        cache = new byte[cacheLen];
    }

    public string Url { get; private set; }

    public override bool CanRead { get { return true; } }
    public override bool CanWrite { get { return false; } }
    public override bool CanSeek { get { return true; } }

    public override long Position { get; set; }

    /// <summary>
    /// Lazy initialized length of the resource.
    /// </summary>
    public override long Length
    {
        get
        {
            if (length == null)
                length = HttpGetLength();
            return length.Value;
        }
    }

    /// <summary>
    /// Count of HTTP requests. Just for statistics reasons.
    /// </summary>
    public int HttpRequestsCount { get; private set; }

    public override void SetLength(long value)
    { throw new NotImplementedException(); }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (buffer == null)
            throw new ArgumentNullException(nameof(buffer));
        if (offset < 0 || offset >= buffer.Length)
            throw new ArgumentException(nameof(offset));
        if (count < 0 || offset + count > buffer.Length)
            throw new ArgumentException(nameof(count));

        long curPosition = Position;
        Position += ReadFromCache(buffer, ref offset, ref count);
        //if (Position > curPosition)
        //    Console.WriteLine($"Cache hit {Position - curPosition}");
        if (count > cacheLen)
        {
            // large request, do not cache
            Position += HttpRead(buffer, offset, count);
        }
        else if (count > 0)
        {
            // read to cache
            cachePosition = Position;
            cacheCount = HttpRead(cache, 0, cacheLen);
            Position += ReadFromCache(buffer, ref offset, ref count);
        }

        return (int)(Position - curPosition);
    }

    public override void Write(byte[] buffer, int offset, int count)
    { throw new NotImplementedException(); }

    public override long Seek(long pos, SeekOrigin origin)
    {
        switch (origin)
        {
            case SeekOrigin.End:
                Position = Length + pos;
                break;

            case SeekOrigin.Begin:
                Position = pos;
                break;

            case SeekOrigin.Current:
                Position += pos;
                break;
        }
        return Position;
    }

    public override void Flush()
    {
    }

    private int ReadFromCache(byte[] buffer, ref int offset, ref int count)
    {
        if (cachePosition > Position || (cachePosition + cacheCount) <= Position)
            return 0; // cache miss
        int ccOffset = (int)(Position - cachePosition);
        int ccCount = Math.Min(cacheCount - ccOffset, count);
        Array.Copy(cache, ccOffset, buffer, offset, ccCount);
        offset += ccCount;
        count -= ccCount;
        return ccCount;
    }

    private int HttpRead(byte[] buffer, int offset, int count)
    {
        HttpRequestsCount++;
        var easy = CurlNative.Easy.Init();
        var dataCopier = new DataCallbackCopier();

        CurlNative.Easy.SetOpt(easy, CURLoption.URL, Url);
        CurlNative.Easy.SetOpt(easy, CURLoption.WRITEFUNCTION, dataCopier.DataHandler);
        CurlNative.Easy.SetOpt(easy, CURLoption.FOLLOWLOCATION, 1);
        CurlNative.Easy.SetOpt(easy, CURLoption.SSL_VERIFYPEER, 0);
        CurlNative.Easy.SetOpt(easy, CURLoption.SSL_VERIFYHOST, 0);

        var headers = CurlNative.Slist.Append(SafeSlistHandle.Null, $"Range: bytes={Position}-{Position + count - 1}");
        // Add one more value to existing HTTP header list.
        //CurlNative.Slist.Append(headers, "X-Qwerty: Asdfgh");
        CurlNative.Easy.SetOpt(easy, CURLoption.HTTPHEADER, headers.DangerousGetHandle());

        var result = CurlNative.Easy.Perform(easy);

        CurlNative.Slist.FreeAll(headers);

        //Log($"Result code: {result}.");
        var bytes = dataCopier.Stream.ToArray();
        easy.Dispose();
        //Log(bytes.Length, "Response body length");
        bytes.CopyTo(buffer, 0);
        return bytes.Length;
    }

    private long HttpGetLength()
    {
        HttpRequestsCount++;
#if false
        HttpWebRequest request = HttpWebRequest.CreateHttp(Url);
        request.Method = "HEAD";
        return request.GetResponse().ContentLength;
#else
        var easy = CurlNative.Easy.Init();
        var headCopier = new DataCallbackCopier();
        CurlNative.Easy.SetOpt(easy, CURLoption.URL, Url);
        CurlNative.Easy.SetOpt(easy, CURLoption.NOBODY, 1); // HEAD?
        CurlNative.Easy.SetOpt(easy, CURLoption.HEADERFUNCTION, headCopier.DataHandler);
        CurlNative.Easy.SetOpt(easy, CURLoption.FOLLOWLOCATION, 1);
        CurlNative.Easy.SetOpt(easy, CURLoption.SSL_VERIFYPEER, 0);
        CurlNative.Easy.SetOpt(easy, CURLoption.SSL_VERIFYHOST, 0);
        var result = CurlNative.Easy.Perform(easy);
        //Log(result.ToString());
        CurlNative.Easy.GetInfo(easy, CURLINFO.RESPONSE_CODE, out int httpCode);
        //Log(httpCode, "httpCode");
        string headers = headCopier.ReadAsString();
        string line;
        StringReader streamReader = new StringReader(headers);
        long n = 0;
        while ((line = streamReader.ReadLine()) != null)
        {
            //Log(line, "line");
            if (line.ToUpper().StartsWith("CONTENT-LENGTH:"))
            {
                line = line.Substring(15).Trim();
                //Log($"line='{line}'");
                n = long.Parse(line);
                //Log($"n={n}");
            }

        }
        easy.Dispose();
        return n;
#endif
    }

    private new void Dispose()
    {
        base.Dispose();
        if (stream != null)
        {
            stream.Dispose();
            stream = null;
        }
        if (response != null)
        {
            response.Dispose();
            response = null;
        }
        /*
        if (global == CURLcode.OK)
        {
            CurlNative.Cleanup();
        }
        */
    }
}
#endif