using NetUV.Core.Logging;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonDLL;

public class MSys2
{
    private static string MSys2Dir;
    static MSys2()
    {
        string zipPath = Path.Combine(Dirs.ProfilePath(".JsonDLL", ".msys2"), "msys2-base-x86_64-20231026.zip");
        if (!File.Exists(zipPath))
        {
            //Dirs.PrepareForFile(zipPath);
            Util.Log($"Donloading to {zipPath}...");
            Util.DownloadBinaryFromUrl("https://github.com/nuget-tools/JsonDLL.Assets/releases/download/64bit/msys2-base-x86_64-20231026.zip", zipPath);
            Util.Log($"Donloading to {zipPath}...Done");
        }
        MSys2Dir = Path.Combine(Dirs.ProfilePath(".JsonDLL", ".msys2"), "msys2-base-x86_64-20231026");
        if (!Directory.Exists(MSys2Dir))
        {
            Util.Log($"Extracting to {MSys2Dir}...");
            ZipFile.ExtractToDirectory(zipPath, MSys2Dir);
            Util.Log($"Extracting to {MSys2Dir}...Done");
        }
    }
    public static void Initialize()
    {
        ;
    }
    public static void Test01()
    {
        var PATH=$"{MSys2Dir}\\usr\\bin;{Environment.GetEnvironmentVariable("PATH")}";
        //Util.Log(PATH, "PATH");
        var env = new Dictionary<string, string>();
        env["PATH"] = PATH;
        //Util.RunToConsole("cmd.exe", new string[] { "/c", "start", "bash.exe", "-c", "set -uvx;set -e;pwd;sleep 3600" }, env);
        Util.LaunchProcess("cmd.exe", new string[] { "/c", "start", "bash.exe", "-c", "set -uvx;set -e;pwd;sleep 3600" }, env);
    }
}
