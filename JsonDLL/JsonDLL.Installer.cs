using System;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Reflection;

namespace JsonDLL;

public class Installer
{
    public static string InstallZipFromURL(string url, string targetDir, string baseName)
    {
        string guid = Util.GuidString();
        string zipPath = Path.Combine(targetDir, @$"{baseName}.zip");
        if (!File.Exists(zipPath))
        {
            Util.Log($"Donloading to {zipPath}...");
            Util.DownloadBinaryFromUrl(url, $"{zipPath}.{guid}");
            try
            {
                File.Move($"{zipPath}.{guid}", zipPath);
            }
            catch (Exception)
            {
                ;
            }
            Util.Log($"Donloading to {zipPath}...Done");
        }
        string installDir = Path.Combine(targetDir, baseName);
        if (!Directory.Exists(installDir))
        {
            Util.Log($"Extracting to {installDir}...");
            ZipFile.ExtractToDirectory(zipPath, $"{installDir}.{guid}");
            try
            {
                Directory.Move($"{installDir}.{guid}", installDir);
            }
            catch (Exception)
            {
                ;
            }
            Util.Log($"Extracting to {installDir}...Done");
        }
        return installDir;
    }
    public static string InstallResourceDll(Assembly assembly, string targetDir, string name)
    {
        string guid = Util.GuidString();
        var dllBytes = Util.ResourceAsBytes(typeof(ProcessRunner).Assembly, name);
        SHA256 crypto = new SHA256CryptoServiceProvider();
        byte[] hashValue = crypto.ComputeHash(dllBytes);
        string sha256 = String.Join("", hashValue.Select(x => x.ToString("x2")).ToArray());
        string dllName = $"{Dirs.GetFileNameWithoutExtension(name.Replace(":", "-"))}-{sha256}.dll";
        var dllPath = Path.Combine(targetDir, dllName);
        if (File.Exists(dllPath))
        {
            //Util.Log($"{dllPath} is installed");
        }
        else
        {
            Dirs.PrepareForFile(dllPath);
            File.WriteAllBytes($"{dllPath}.{guid}", dllBytes);
            try
            {
                File.Move($"{dllPath}.{guid}", dllPath);
            }
            catch (Exception)
            {
                ;
            }
            Util.Log($"{dllPath} has been written");
        }
        return dllPath;
    }
    public static string InstallResourceZip(Assembly assembly, string targetDir, string name)
    {
        string guid = Util.GuidString();
        var zipBytes = Util.ResourceAsBytes(typeof(Internal).Assembly, name);
        SHA256 crypto = new SHA256CryptoServiceProvider();
        byte[] hashValue = crypto.ComputeHash(zipBytes);
        string sha256 = String.Join("", hashValue.Select(x => x.ToString("x2")).ToArray());
        string zipName = $"{Dirs.GetFileNameWithoutExtension(name.Replace(":", "-"))}-{sha256}";
        var extractPath = Path.Combine(targetDir, zipName);
        if (Directory.Exists(extractPath))
        {
            //Util.Log($"{extractPath} already exists");
        }
        else
        {
            string zipPath = Path.Combine(targetDir, $"{zipName}.zip");
            Util.Log(zipPath, "zipPath");
            Dirs.PrepareForFile(zipPath);
            File.WriteAllBytes(zipPath, zipBytes);
            ZipFile.ExtractToDirectory(zipPath, $"{extractPath}.{guid}");
            try
            {
                Directory.Move($"{extractPath}.{guid}", extractPath);
            }
            catch (Exception)
            {
                ;
            }
            Util.Log($"{extractPath} has been created");
        }
        return extractPath;
    }
}
