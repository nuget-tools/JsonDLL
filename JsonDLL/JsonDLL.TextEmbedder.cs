using System;
using System.IO;
using System.Text.RegularExpressions;
using static JsonDLL.Util;

namespace JsonDLL;

public static class TextEmbedder
{
    private static string TextEmbedFromUrl(string url, long searchLimit)
    {
        Log($"TextEmbedFromUrl(): searchLimit={searchLimit}");
        long endPos = -1;
        var phs = new PartialHttpStream(url, 1000000);
        var phsr = new System.IO.StreamReader(phs, System.Text.Encoding.UTF8);
        phs.Seek(0, System.IO.SeekOrigin.Begin);
        int shortLen = 1024*1024;
        byte[] byteArray = new byte[shortLen];
        long readLen = phs.Read(byteArray, 0, shortLen);
        Log(readLen, "hsortResult");
        if (readLen >= shortLen)
        {
            if (searchLimit < 0 || searchLimit > phs.Length) searchLimit = phs.Length;
            phs.Seek(phs.Length - searchLimit, System.IO.SeekOrigin.Begin);
            byteArray = new byte[searchLimit];
            phs.Read(byteArray, 0, (int)searchLimit);
        }
        else
        {
            byte[] newArray = new byte[readLen];
            Array.Copy(byteArray, 0, newArray, 0, newArray.Length);
            byteArray = newArray;
        }
        MemoryStream ms = new MemoryStream(byteArray);
        StreamReader sr = new StreamReader(ms);
        for (long i = 0; i < searchLimit; i++)
        {
            long pos = ms.Length - i;
            ms.Seek(pos, System.IO.SeekOrigin.Begin);
            string part = sr.ReadToEnd();
            if (endPos >= 0)
            {
                string pattern = @"^\[embed\]";
                Match m = Regex.Match(part, pattern);
                if (m.Success)
                {
                    long startPos = pos + 7;
                    long resultLength = endPos - startPos;
                    var result = new byte[resultLength];
                    ms.Seek(startPos, System.IO.SeekOrigin.Begin);
                    ms.Read(result, 0, result.Length);
                    string text = System.Text.Encoding.UTF8.GetString(result).Trim();
                    phs.Close();
                    return text;
                }
            }
            else
            {
                string pattern = @"^\[/embed\]\s*";
                Match m = Regex.Match(part, pattern);
                if (m.Success)
                {
                    endPos = pos;
                }
            }
        }
        //Log("Z");
        return null;
    }
    public static string TextEmbed(string path, long searchLimit = 8192)
    {
        //Log(path, "path");
        try
        {
            if (path.StartsWith("http:") || path.StartsWith("https:"))
            {
                //Log("is url");
                return TextEmbedFromUrl(path, searchLimit);
            }
            long endPos = -1;
            var fs = System.IO.File.OpenRead(path);
            var sr = new System.IO.StreamReader(fs, System.Text.Encoding.UTF8);
            if (searchLimit < 0 || searchLimit > fs.Length) searchLimit = fs.Length;
            for (long i = 0; i < searchLimit; i++)
            {
                long pos = fs.Length - i;
                fs.Seek(pos, System.IO.SeekOrigin.Begin);
                string part = sr.ReadToEnd();
                if (endPos >= 0)
                {
                    string pattern = @"^\[embed\]";
                    Match m = Regex.Match(part, pattern);
                    if (m.Success)
                    {
                        long startPos = pos + 7;
                        long resultLength = endPos - startPos;
                        var result = new byte[resultLength];
                        fs.Seek(startPos, System.IO.SeekOrigin.Begin);
                        fs.Read(result, 0, result.Length);
                        string text = System.Text.Encoding.UTF8.GetString(result).Trim();
                        fs.Close();
                        return text;
                    }
                }
                else
                {
                    string pattern = @"^\[/embed\]\s*";
                    Match m = Regex.Match(part, pattern);
                    if (m.Success)
                    {
                        endPos = pos;
                    }
                }
            }
            fs.Close();
            return null;
        }
        catch (Exception e)
        {
            Log(e.ToString());
            return null;
        }
    }
    private static string TextActualFromUrl(string url)
    {
        long searchLimit = -1;
        long endPos = -1;
        var phs = new PartialHttpStream(url, 1000000);
        var phsr = new System.IO.StreamReader(phs, System.Text.Encoding.UTF8);
        if (searchLimit < 0 || searchLimit > phs.Length) searchLimit = phs.Length;
        phs.Seek(phs.Length - searchLimit, System.IO.SeekOrigin.Begin);
        byte[] byteArray = new byte[searchLimit];
        phs.Read(byteArray, 0, (int)searchLimit);
        MemoryStream ms = new MemoryStream(byteArray);
        StreamReader sr = new StreamReader(ms);
        for (long i = 0; i < searchLimit; i++)
        {
            long pos = ms.Length - i;
            ms.Seek(pos, System.IO.SeekOrigin.Begin);
            string part = sr.ReadToEnd();
            if (endPos >= 0)
            {
                string pattern = @"^\[embed\]";
                Match m = Regex.Match(part, pattern);
                if (m.Success)
                {
                    long startPos = pos;
                    long resultLength = pos;
                    var result = new byte[resultLength];
                    ms.Seek(0, System.IO.SeekOrigin.Begin);
                    ms.Read(result, 0, result.Length);
                    string text = System.Text.Encoding.UTF8.GetString(result);
                    phs.Close();
                    return text;
                }
            }
            else
            {
                string pattern = @"^\[/embed\]\s*";
                Match m = Regex.Match(part, pattern);
                if (m.Success)
                {
                    endPos = pos;
                }
            }
        }
        {
            var result = new byte[phs.Length];
            phs.Seek(0, System.IO.SeekOrigin.Begin);
            phs.Read(result, 0, result.Length);
            string text = System.Text.Encoding.UTF8.GetString(result);
            phs.Close();
            return text;

            //return null;
        }
    }
    public static string TextActual(string path)
    {
        //Log(path, "path");
        long searchLimit = -1;
        try
        {
            if (path.StartsWith("http:") || path.StartsWith("https:"))
            {
                Log("is url");
                return TextActualFromUrl(path);
            }
            long endPos = -1;
            var fs = System.IO.File.OpenRead(path);
            var sr = new System.IO.StreamReader(fs, System.Text.Encoding.UTF8);
            if (searchLimit < 0 || searchLimit > fs.Length) searchLimit = fs.Length;
            for (long i = 0; i < searchLimit; i++)
            {
                long pos = fs.Length - i;
                fs.Seek(pos, System.IO.SeekOrigin.Begin);
                string part = sr.ReadToEnd();
                if (endPos >= 0)
                {
                    string pattern = @"^\[embed\]";
                    Match m = Regex.Match(part, pattern);
                    if (m.Success)
                    {
                        long startPos = pos;
                        long resultLength = pos;
                        var result = new byte[resultLength];
                        fs.Seek(0, System.IO.SeekOrigin.Begin);
                        fs.Read(result, 0, result.Length);
                        string text = System.Text.Encoding.UTF8.GetString(result);
                        fs.Close();
                        return text;
                    }
                }
                else
                {
                    string pattern = @"^\[/embed\]\s*";
                    Match m = Regex.Match(part, pattern);
                    if (m.Success)
                    {
                        endPos = pos;
                    }
                }
            }
            {
                var result = new byte[fs.Length];
                fs.Seek(0, System.IO.SeekOrigin.Begin);
                fs.Read(result, 0, result.Length);
                string text = System.Text.Encoding.UTF8.GetString(result);
                fs.Close();
                return text;
                //return null;
            }
        }
        catch (Exception e)
        {
            Log(e.ToString());
            return null;
        }
    }
}
