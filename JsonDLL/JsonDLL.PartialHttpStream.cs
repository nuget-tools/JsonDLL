using System.IO;
using System.Net;
using System;
using static JsonDLL.Util;
using CurlThin;
using CurlThin.Enums;
using CurlThin.Helpers;
using CurlThin.SafeHandles;
using System.Text;

namespace JsonDLL;

public class PartialHttpStream : Stream, IDisposable
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

     static PartialHttpStream()
    {
        LibCurlLoader.Initialize();
    }
    public PartialHttpStream(string url, int cacheLen = CacheLen)
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
        //Log(bytes.Length, "Response body length");
        bytes.CopyTo(buffer, 0);
        return bytes.Length;
    }

    private long HttpGetLength()
    {
        HttpRequestsCount++;
        HttpWebRequest request = HttpWebRequest.CreateHttp(Url);
        request.Method = "HEAD";
        return request.GetResponse().ContentLength;
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
        if (global == CURLcode.OK)
        {
            CurlNative.Cleanup();
        }
    }
}
