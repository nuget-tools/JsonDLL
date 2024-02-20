using System.Reflection;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using JsonDLL.Json;
using JsonDLL.Json.Bson;
using JsonDLL.Json.Linq;
using System.Text;
using System.Diagnostics;
using System.Media;
using System.Net.NetworkInformation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace JsonDLL;

/** @brief MyClass does something
 * @details I have something more long winded to say about it.  See example 
 * in test.cs: @include test.cs */
public class Util
{
    public static bool DebugFlag = false;
    public static System.Threading.Mutex ProcessMutex = new System.Threading.Mutex(false, "ProcessMutex");
    static Util()
    {
    }
    public static string[] TextToLines(string text)
    {
        List<string> lines = new List<string>();
        using (StringReader sr = new StringReader(text))
        {
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                lines.Add(line);
            }
        }
        return lines.ToArray();
    }
    public static string RandomString(Random r, string[] chars, int length)
    {
        if (chars.Length == 0 || length < 0)
        {
            throw new ArgumentException();
        }
        var sb = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            int idx = r.Next(0, chars.Length);
            sb.Append(chars[idx]);
        }
        return sb.ToString();
    }
    public static string ToIdentifier(string name)
    {
        var sb = new StringBuilder();
        char last = '\0';
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || ('0' <= c && c <= '9'))
            {
                ;
            }
            else
            {
                c = '_';
            }
            if (last == '_' && c == '_')
            {
                ;
            }
            else
            {
                sb.Append(c);
            }
            last = c;
        }
        return sb.ToString();
    }
    public static string[] ExpandWildcard(string path)
    {
        string? dir = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(dir)) dir = ".";
        //Util.Print(dir, "dir");
        string fname = Path.GetFileName(path);
        //Util.Print(fname, "fname");
        string[] files = Directory.GetFiles(dir, fname);
        List<string> result = new List<string>();
        for (int i = 0; i < files.Length; i++)
        {
            result.Add(Path.GetFullPath(files[i]));
        }

        return result.ToArray();
    }
    public static string[] ExpandWildcardList(params string[] pathList)
    {
        List<string> result = new List<string>();
        for (int i = 0; i < pathList.Length; i++)
        {
            //Util.Print(pathList[i], "pathList[i]");
            string[] files = ExpandWildcard(pathList[i]);
            result.AddRange(files.ToList());
        }

        return result.ToArray();
    }
    public static void AllocConsole()
    {
        WinConsole.Initialize();
    }

    public static void FreeConsole()
    {
        WinConsole.Deinitialize();
    }
    public static void ReallocConsole()
    {
        FreeConsole();
        AllocConsole();
    }
    public static void DownloadBinaryFromUrl(string url, string destinationPath)
    {
        Dirs.PrepareForFile(destinationPath);
        WebRequest objRequest = System.Net.HttpWebRequest.Create(url);
        var objResponse = objRequest.GetResponse();
        byte[] buffer = new byte[32768];
        using (Stream input = objResponse.GetResponseStream())
        {
            using (FileStream output = new FileStream(destinationPath, FileMode.CreateNew))
            {
                int bytesRead;
                while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    output.Write(buffer, 0, bytesRead);
                }
            }
        }
    }
    public static List<string> GetMacAddressList()
    {
        var list = NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Select(nic => String.Join("-", SplitStringByLengthList(nic.GetPhysicalAddress().ToString().ToLower(), 2)))
            .ToList();
        return list;
    }
    public static IEnumerable<string> SplitStringByLengthLazy(string str, int maxLength)
    {
        for (int index = 0; index < str.Length; index += maxLength)
        {
            yield return str.Substring(index, Math.Min(maxLength, str.Length - index));
        }
    }
    public static List<string> SplitStringByLengthList(string str, int maxLength)
    {
        return SplitStringByLengthLazy(str, maxLength).ToList();
    }
    public static byte[] ReadFileHeadBytes(string path, int maxSize)
    {
        System.IO.FileStream fs = new System.IO.FileStream(
            path,
            System.IO.FileMode.Open,
            System.IO.FileAccess.Read);
        byte[] array = new byte[maxSize];
        int size = fs.Read(array, 0, array.Length);
        fs.Close();
        byte[] result = new byte[size];
        Array.Copy(array, 0, result, 0, result.Length);
        return result;
    }
    public static bool IsBinaryFile(string path)
    {
        byte[] head = ReadFileHeadBytes(path, 8000);
        for (int i = 0; i < head.Length; i++)
        {
            if (head[i] == 0) return true;
        }
        return false;
    }
    public static bool LaunchProcess(string exePath, string[] args, Dictionary<string, string>? vars = null)
    {
        ProcessMutex.WaitOne();
        string argList = "";
        for (int i = 0; i < args.Length; i++)
        {
            if (i > 0) argList += " ";
            if (args[i].Contains(" "))
                argList += $"\"{args[i]}\"";
            else
                argList += args[i];
        }
        Process process = new Process();
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.FileName = exePath;
        process.StartInfo.Arguments = argList;
        if (vars != null)
        {
            foreach (string key in vars.Keys)
            {
                process.StartInfo.EnvironmentVariables[key] = vars[key];
            }
        }
        bool result = process.Start();
        ProcessMutex.ReleaseMutex();
        return result;
    }
    public static void Beep()
    {
        SystemSounds.Beep.Play();
    }
    public static string FindExePath(string exe)
    {
        string cwd = "";
        return FindExePath(exe, cwd);
    }
    public static string FindExePath(string exe, string cwd)
    {
        exe = Environment.ExpandEnvironmentVariables(exe);
        if (Path.IsPathRooted(exe))
        {
            if (!File.Exists(exe)) return null;
            return Path.GetFullPath(exe);
        }
        //var cwd = Directory.GetCurrentDirectory();
        var PATH = Environment.GetEnvironmentVariable("PATH") ?? "";
        PATH = $"{cwd};{PATH}";
        foreach (string test in PATH.Split(';'))
        {
            string path = test.Trim();
            if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                return Path.GetFullPath(path);
        }
        return null;
    }
    public static string FindExePath(string exe, Assembly assembly)
    {
        int bit = IntPtr.Size * 8;
        string cwd = AssemblyDirectory(assembly);
        string result = FindExePath(exe, $"{cwd}\\{bit}bit");
        if (result == null)
        {
            cwd = Path.Combine(cwd, "assets");
            result = FindExePath(exe, $"{cwd}\\{bit}bit");
        }
        return result;
    }
    public static string AssemblyDirectory(Assembly assembly)
    {
        string codeBase = assembly.CodeBase;
        UriBuilder uri = new UriBuilder(codeBase);
        string path = Uri.UnescapeDataString(uri.Path);
        return Path.GetDirectoryName(path);
    }
    public static void FreeHGlobal(IntPtr x)
    {
        Marshal.FreeHGlobal(x);
    }
    public static IntPtr StringToWideAddr(string s)
    {
        return Marshal.StringToHGlobalUni(s);
    }
    public static string WideAddrToString(IntPtr s)
    {
        return Marshal.PtrToStringUni(s);
    }
    public static IntPtr StringToUTF8Addr(string s)
    {
        int len = Encoding.UTF8.GetByteCount(s);
        byte[] buffer = new byte[len + 1];
        Encoding.UTF8.GetBytes(s, 0, s.Length, buffer, 0);
        IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
        Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
        return nativeUtf8;
    }
    public static string UTF8AddrToString(IntPtr s)
    {
        int len = 0;
        while (Marshal.ReadByte(s, len) != 0) ++len;
        byte[] buffer = new byte[len];
        Marshal.Copy(s, buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer);
    }
    public static string DateTimeString(DateTime x)
    {
        return x.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
    }
    public static int RunToConsole(string exePath, string[] args, Dictionary<string, string>? vars = null)
    {
        ProcessMutex.WaitOne();
        string argList = "";
        for (int i = 0; i < args.Length; i++)
        {
            if (i > 0) argList += " ";
            if (args[i].Contains(" "))
                argList += $"\"{args[i]}\"";
            else
                argList += args[i];
        }
        Process process = new Process();
        ProcessMutex.ReleaseMutex();
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.FileName = exePath;
        process.StartInfo.Arguments = argList;
        if (vars != null)
        {
            var keys = vars.Keys;
            foreach (var key in keys)
            {
                process.StartInfo.EnvironmentVariables[key] = vars[key];
            }
        }
        process.OutputDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
        process.ErrorDataReceived += (sender, e) => { Console.Error.WriteLine(e.Data); };
        process.Start();
        Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) { process.Kill(); };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        process.CancelOutputRead();
        process.CancelErrorRead();
        return process.ExitCode;
    }
    /**
     * <summary> Sleeps for specified milliseconds (指定されたミリ秒間スリープする)
     * </summary>
     * <description>
     * @code
     * using System;
     * using Global;
     * Util.Print(DateTime.Now, "begin");
     * Util.Sleep(3000); // sleeps for 3 seconds
     * Util.Print(DateTime.Now, "end");
     * @endcode
     * @code
     * begin: 2023-11-05T21:30:41.8610034+09:00
     * end: 2023-11-05T21:30:45.1998930+09:00
     * @endcode
     * </description>
     * @param[in] milliseconds milliseconds (ミリ秒)
     */
    public static void Sleep(int milliseconds)
    {
        Thread.Sleep(milliseconds);
    }
    public static string AssemblyName(Assembly assembly)
    {
        return System.Reflection.AssemblyName.GetAssemblyName(assembly.Location).Name;
    }

    public static int FreeTcpPort()
    {
        // https://stackoverflow.com/questions/138043/find-the-next-tcp-port-in-net
        TcpListener l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }

    public static DateTime? AsDateTime(dynamic x)
    {
        if (x is null) return null;
        string fullName = Util.FullName(x);
        if (fullName == "Newtonsoft.Json.Linq.JValue")
        {
            return ((DateTime)x);
        }
        else if (fullName == "System.DateTime")
        {
            return (System.DateTime)x;
        }
        else if (fullName == "System.String")
        {
            if (((string)x) == "") return null;
            return DateTime.Parse((string)x);
        }
        else
        {
            throw new ArgumentException("x");
        }
    }

    public static string FullName(dynamic x)
    {
        if (x is null) return "null";
        string fullName = ((object)x).GetType().FullName;
        return fullName.Split('`')[0];
    }

    public static string ToJson(dynamic x, bool indent = false)
    {
        return JsonConvert.SerializeObject(x, indent ? Formatting.Indented : Formatting.None);
    }

#if true
    public static dynamic? FromJson(string json)
    {
        if (String.IsNullOrEmpty(json)) return null;
        return JsonConvert.DeserializeObject(json, new JsonSerializerSettings
        {
            DateParseHandling = DateParseHandling.None
        });
        /*
        return JObject.Parse(json, new JsonLoadSettings
        {
            CommentHandling = CommentHandling.Load
        });
        */
    }
#endif


    public static dynamic? FromJson(byte[] json)
    {
        if (json is null) return null;
        return FromJson(Encoding.UTF8.GetString(json));
    }

    public static T? FromJson<T>(string json, T? fallback = default(T))
    {
        //if (String.IsNullOrEmpty(json)) return default(T);
        if (String.IsNullOrEmpty(json)) return fallback;
        return JsonConvert.DeserializeObject<T>(json);
    }

    public static byte[] ToBson(dynamic x)
    {
        MemoryStream ms = new MemoryStream();
        using (BsonWriter writer = new BsonWriter(ms))
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(writer, x);
        }

        return ms.ToArray();
    }

    public static dynamic? FromBson(byte[] bson)
    {
        if (bson is null) return null;
        MemoryStream ms = new MemoryStream(bson);
        using (BsonReader reader = new BsonReader(ms))
        {
            JsonSerializer serializer = new JsonSerializer();
            return serializer.Deserialize(reader);
        }
    }

    public static T? FromBson<T>(byte[] bson)
    {
        if (bson is null) return default(T);
        MemoryStream ms = new MemoryStream(bson);
        using (BsonReader reader = new BsonReader(ms))
        {
            JsonSerializer serializer = new JsonSerializer();
            return serializer.Deserialize<T>(reader);
        }
    }

    public static dynamic? FromObject(dynamic x)
    {
        if (x is null) return null;
        var o = (dynamic)JObject.FromObject(new { x = x },
            new JsonSerializer
            {
                DateParseHandling = DateParseHandling.None
            });
        return o.x;
    }

    public static T? FromObject<T>(dynamic x)
    {
        dynamic? o = FromObject(x);
        if (o is null) return default(T);
        return (T)(o.ToObject<T>());
    }

    public static dynamic? ToNewton(dynamic x)
    {
        if (x is null) return null;
        var o = (dynamic)JObject.FromObject(new { x = x },
            new JsonSerializer
            {
                DateParseHandling = DateParseHandling.None
            });
        return o.x;
    }

    public static T? ToNewton<T>(dynamic x)
    {
        dynamic? o = FromObject(x);
        if (o is null) return default(T);
        return (T)(o.ToObject<T>());
    }

    public static string? ToXml(dynamic x)
    {
        if (x is null) return null;
        if (FullName(x) == "System.Xml.Linq.XElement")
        {
            return ((XElement)x).ToString();
        }

        XDocument? doc;
        if (FullName(x) == "System.Xml.Linq.XDocument")
        {
            doc = (XDocument)x;
        }
        else
        {
            string json = ToJson(x);
            doc = JsonConvert.DeserializeXmlNode(json)?.ToXDocument();
            //return "<?>";
        }

        return doc is null ? "null" : doc.ToStringWithDeclaration();
    }

    public static XDocument? FromXml(string xml)
    {
        if (xml is null) return null;
        XDocument doc = XDocument.Parse(xml);
        return doc;
    }

    public static string ToString(dynamic x)
    {
        if (x is null) return "null";
        if (x is string) return (string)x;
        if (x is JsonDLL.Json.Linq.JValue)
        {
            var value = (JValue)x;
            try
            {
                x = (DateTime)value;
                //return Util.DateTimeString(x); //x.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
            }
            catch (Exception)
            {
            }
        }

        if (x is System.Xml.Linq.XDocument || x is System.Xml.Linq.XElement)
        {
            string xml = ToXml(x);
            return xml;
        }
        else if (x is IEnumerable<XElement>)
        {
            XElement result = new XElement("IEnumerable");
            foreach (var e in x)
            {
                //string xml = ToXml(e);
                result.Add(e);
            }
            return ToString(result);
        }
        else if (x is System.DateTime)
        {
            //return x.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
            return Util.DateTimeString(x); //x.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffffffzzz");
        }
        else
        {
            try
            {
                string json = ToJson(x, true);
                return json;
            }
            catch (Exception)
            {
                return x.ToString();
            }
        }
    }

    public static void Print(dynamic x, string? title = null)
    {
        String s = "";
        if (title != null) s = title + ": ";
        s += Util.ToString(x);
        Console.WriteLine(s);
        System.Diagnostics.Debug.WriteLine(s);
    }

    public static void Log(dynamic x, string? title = null)
    {
        String s = "";
        if (title != null) s = title + ": ";
        s += Util.ToString(x);
        Console.Error.WriteLine("[Log] " + s);
        System.Diagnostics.Debug.WriteLine("[Log] " + s);
    }

    public static void Debug(dynamic x, string? title = null)
    {
        if (!DebugFlag) return;
        String s = "";
        if (title != null) s = title + ": ";
        s += Util.ToString(x);
        Console.Error.WriteLine("[Debug] " + s);
        System.Diagnostics.Debug.WriteLine("[Debug] " + s);
    }

    public static XDocument ParseXml(string xml)
    {
        XDocument doc = XDocument.Parse(xml);
        return doc;
    }

    public static string[] ResourceNames(Assembly assembly)
    {
        return assembly.GetManifestResourceNames();
    }

    public static Stream? ResourceAsStream(Assembly assembly, string name)
    {
        string resourceName = name.Contains(":") ? name.Replace(":", ".") : $"{AssemblyName(assembly)}.{name}";
        Stream? stream = assembly.GetManifestResourceStream(resourceName);
        return stream;
    }

    public static string StreamAsText(Stream stream)
    {
        if (stream is null) return null; // "";
        long pos = stream.Position;
        var streamReader = new StreamReader(stream);
        var text = streamReader.ReadToEnd();
        stream.Position = pos;
        return text;
    }

    public static string ResourceAsText(Assembly assembly, string name)
    {
        string resourceName = name.Contains(":") ? name.Replace(":", ".") : $"{AssemblyName(assembly)}.{name}";
        Stream stream = assembly.GetManifestResourceStream(resourceName);
        return StreamAsText(stream);
    }

    public static byte[] StreamAsBytes(Stream stream)
    {
        if (stream is null) return null; // new byte[] { };
        long pos = stream.Position;
        byte[] bytes = new byte[(int)stream.Length];
        stream.Read(bytes, 0, (int)stream.Length);
        stream.Position = pos;
        return bytes;
    }

    public static byte[] ResourceAsBytes(Assembly assembly, string name)
    {
        string resourceName = name.Contains(":") ? name.Replace(":", ".") : $"{AssemblyName(assembly)}.{name}";
        Stream stream = assembly.GetManifestResourceStream(resourceName);
        return StreamAsBytes(stream);
    }

    public static dynamic? StreamAsJson(Stream stream)
    {
        string json = StreamAsText(stream);
        return FromJson(json);
    }

    public static dynamic? ResourceAsJson(Assembly assembly, string name)
    {
        string json = ResourceAsText(assembly, name);
        return FromJson(json);
    }

    public static byte[]? ToUtf8Bytes(string? s)
    {
        if (s is null) return null;
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(s);
        return bytes;
    }


    public static void Message(dynamic x, string? title = null)
    {
        if (title is null) title = "Message";
        if ((x as string) != null)
        {
            var s = (string)x;
            System.Diagnostics.Debug.WriteLine(s);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                NativeMethods.MessageBoxW(IntPtr.Zero, s, title, 0);
            }
            else
            {
                Util.Log(s, title);
            }
            return;
        }

        {
            var s = Util.ToString(x);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                NativeMethods.MessageBoxW(IntPtr.Zero, s, title, 0);
            }
            else
            {
                Util.Log(s, title);
            }
        }
        /*
        if (FullName(x) == "System.Xml.Linq.XDocument" ||
            FullName(x) == "System.Xml.Linq.XElement")
        {
            string xml = ToXml(x);
            System.Diagnostics.Debug.WriteLine(xml);
            var s = (string)x;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                NativeMethods.MessageBoxW(IntPtr.Zero, xml, title, 0);
            }
            else
            {
                Tool.Log(xml, title);
            }
        }
        else
        {
            string json = ToJson(x, true);
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                NativeMethods.MessageBoxW(IntPtr.Zero, json, title, 0);
            }
            else
            {
                Tool.Log(json, title);
            }
        }
        */
    }

    public static dynamic? FromNewton(dynamic x)
    {
        dynamic? dyn = FromObject(x);
        return FromNewtonHelper(dyn);
    }

    public static dynamic? ToObject(dynamic x)
    {
        dynamic? dyn = FromObject(x);
        return FromNewtonHelper(dyn);
    }
    public static dynamic? ToObject<T>(dynamic x)
    {
        dynamic? o = FromObject(x);
        if (o is null) return default(T);
        return (T)(o.ToObject<T>());
    }

    private static dynamic? FromNewtonHelper(dynamic? x)
    {
        if (x is null) return null;
        if (x is JArray)
        {
            var result = new List<object>();
            var ary = (JArray)x;
            foreach (var elem in ary)
            {
                result.Add(FromNewtonHelper(elem));
            }
            return result;
        }
        else if (x is JObject)
        {
            var result = new Dictionary<string, object>();
            var obj = (JObject)x;
            foreach (var pair in obj)
            {
                result[pair.Key] = FromNewtonHelper(pair.Value);
            }
            return result;
        }
        else
        {
            var result = (JValue)x;
            return result.Value;
        }
    }


    internal static class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int MessageBoxW(
            IntPtr hWnd, string lpText, string lpCaption, uint uType);
    }
}