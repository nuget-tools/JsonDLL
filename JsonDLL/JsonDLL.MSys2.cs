using System.Diagnostics;
using System.IO.Compression;

namespace JsonDLL;

public class MSys2
{
    private static string MSys2Dir;
    static MSys2()
    {
        string baseName = "msys2-base-x86_64-20240113";
        string zipPath = Path.Combine(Dirs.ProfilePath(".JsonDLL", ".msys2"), $"{baseName}.zip");
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
    }
    public static void Initialize()
    {
        ;
    }
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
        Util.ProcessMutex.ReleaseMutex();
        Environment.SetEnvironmentVariable("PARH", ORIG_PATH);
        child.WaitForExit();
        File.Delete(scriptPath);
        return child.ExitCode;
    }
}
