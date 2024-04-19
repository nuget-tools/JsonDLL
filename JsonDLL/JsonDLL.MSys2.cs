using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
namespace JsonDLL;
public class MSys2
{
#if true
    public static string MSys2Dir;
    public static string MSys2Bin;
#endif
    //static string resDir = Internal.InstallResourceZip("res.zip");
    static MSys2()
    {
#if true
        string baseName = "msys2-base-x86_64-20240113";
        string zipPath = Path.Combine(Dirs.ProfilePath(".javacommons", "JsonDLL"), @$".msys2\{baseName}.zip");
        if (!File.Exists(zipPath))
        {
            //Dirs.PrepareForFile(zipPath);
            Util.Log($"Donloading to {zipPath}...");
            Util.DownloadBinaryFromUrl($"https://github.com/nuget-tools/JsonDLL.Assets/releases/download/64bit/{baseName}.zip", zipPath);
            Util.Log($"Donloading to {zipPath}...Done");
        }
        MSys2Dir = Path.Combine(Dirs.ProfilePath(".JsonDLL", ".msys2"), $"{baseName}");
        if (!Directory.Exists(MSys2Dir))
        {
            Util.Log($"Extracting to {MSys2Dir}...");
            ZipFile.ExtractToDirectory(zipPath, MSys2Dir);
            Util.Log($"Extracting to {MSys2Dir}...Done");
        }
        MSys2Bin = Path.Combine(MSys2Dir, "usr\\bin");
#endif
    }
    public static void Initialize()
    {
        ;
    }
#if true
    public static int RunBashScript(string script)
    {
        Util.ProcessMutex.WaitOne();
        string ORIG_PATH = Environment.GetEnvironmentVariable("PATH");
        var PATH = $"{MSys2Dir}\\usr\\bin;{ORIG_PATH}";
        Environment.SetEnvironmentVariable("PATH", PATH);
        string scriptPath = Path.Combine(Dirs.GetTempPath(), "tmp.sh");
        Util.Log(scriptPath, "scriptPath");
        Dirs.PrepareForFile(scriptPath);
        File.WriteAllText(scriptPath, script);
        var p_info = new ProcessStartInfo
        {
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = true,
            FileName = "bash.exe",
            Arguments = scriptPath,
        };
        Process child = Process.Start(p_info);
        Environment.SetEnvironmentVariable("PARH", ORIG_PATH);
        Util.ProcessMutex.ReleaseMutex();
        child.WaitForExit();
        File.Delete(scriptPath);
        return child.ExitCode;
    }
#endif
#if true
    public static int RunBashScript(bool windowed, string script, string cwd = "")
    {
        string bashExe = Path.Combine(MSys2.MSys2Bin, "bash.exe");
        string tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, script);
        string PATH = Environment.GetEnvironmentVariable("PATH");
        PATH = MSys2.MSys2Bin + ";" + PATH;
        int result = ProcessRunner.RunProcess(windowed, bashExe, new string[] { tempFile }, cwd, new Dictionary<string, string> {
            { "PATH", PATH }
        });
        File.Delete(tempFile);
        return result;
    }
    public static bool LaunchBashScript(bool windowed, string script, string cwd = "")
    {
        string bashExe = Path.Combine(MSys2.MSys2Bin, "bash.exe");
        string tempFile = Path.GetTempFileName();
        //string tempFile = @"D:\temp.sh";
        File.WriteAllText(tempFile, script);
        string PATH = Environment.GetEnvironmentVariable("PATH");
        PATH = MSys2.MSys2Bin + ";" + PATH;
        bool result = ProcessRunner.LaunchProcess(windowed, bashExe, new string[] { tempFile }, cwd, new Dictionary<string, string> {
            { "PATH", PATH }
        }, tempFile);
        return result;
    }
#endif
}
