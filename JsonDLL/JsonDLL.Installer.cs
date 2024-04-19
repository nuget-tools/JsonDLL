using System;
using System.IO.Compression;
using System.IO;

namespace JsonDLL;

public class Installer
{
    public static string InstallZipFromURL(string url, string targetDir, string baseName)
    {
        var guid = Guid.NewGuid().ToString("D");
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
}
