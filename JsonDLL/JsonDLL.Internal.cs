using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static JsonDLL.Util;

namespace JsonDLL;

public class Internal
{
    public static string InstallResourceDll(string name)
    {
        int bit = IntPtr.Size * 8;
        var dir = Dirs.ProfilePath("JavaCommons Technologies", "JsonDLL");
        dir = Path.Combine(dir, $"x{bit}");
        var dllBytes = Util.ResourceAsBytes(typeof(ProcessRunner).Assembly, $"JsonDLL:{name}-x{bit}.dll");
        SHA256 crypto = new SHA256CryptoServiceProvider();
        byte[] hashValue = crypto.ComputeHash(dllBytes);
        string sha256 = String.Join("", hashValue.Select(x => x.ToString("x2")).ToArray());
        string dllName = $"{name}-{sha256}.dll";
        var dllPath = Path.Combine(dir, dllName);
        if (File.Exists(dllPath))
        {
            Util.Log($"{dllPath} is installed");
        }
        else
        {
            Dirs.PrepareForFile(dllPath);
            File.WriteAllBytes(dllPath, dllBytes);
            Util.Log($"{dllPath} has been written");
        }
        return dllPath;
    }
    public static string InstallResourceZip(string name)
    {
        int bit = IntPtr.Size * 8;
        var dir = Dirs.ProfilePath("JavaCommons Technologies", "JsonDLL");
        //dir = Path.Combine(dir, $"x{bit}");
        var zipBytes = Util.ResourceAsBytes(typeof(Internal).Assembly, $"JsonDLL:{name}");
        SHA256 crypto = new SHA256CryptoServiceProvider();
        byte[] hashValue = crypto.ComputeHash(zipBytes);
        string sha256 = String.Join("", hashValue.Select(x => x.ToString("x2")).ToArray());
        string zipName = $"{Dirs.GetFileNameWithoutExtension(name)}-{sha256}";
        var extractPath = Path.Combine(dir, zipName);
        Log(extractPath, "extractPath");
        if (Directory.Exists(extractPath))
        {
            Util.Log($"{extractPath} is installed");
        }
        else
        {
            string zipPath = Path.Combine(dir, $"{zipName}.zip");
            Log(zipPath, "zipPath");
            Dirs.PrepareForFile(zipPath);
            File.WriteAllBytes(zipPath, zipBytes);
            Dirs.Prepare(extractPath);
            ZipFile.ExtractToDirectory(zipPath, extractPath);
            Util.Log($"{extractPath} has been written");
        }
        return Path.Combine(extractPath, $"x{bit}");
    }
}
