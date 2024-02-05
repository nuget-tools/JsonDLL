using System.Diagnostics;
using System.IO.Compression;

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
        var PATH = $"{MSys2Dir}\\usr\\bin;{Environment.GetEnvironmentVariable("PATH")}";
        Environment.SetEnvironmentVariable("PATH", PATH);
    }
    public static void Initialize()
    {
        ;
    }
    public static int RunBashScript(string script)
    {
        var PATH = $"{MSys2Dir}\\usr\\bin;{Environment.GetEnvironmentVariable("PATH")}";
        var p_info = new ProcessStartInfo
        {
            CreateNoWindow = true,
            //WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            FileName = "bash.exe",
            Arguments = "",
        };
        p_info.EnvironmentVariables["PATH"] = PATH;
        Process child = Process.Start(p_info);
        child.OutputDataReceived += (sender, e) => { Console.WriteLine(e.Data); };
        child.ErrorDataReceived += (sender, e) => { Console.Error.WriteLine(e.Data); };
        using (var stdin = child.StandardInput)
        {
            stdin.Write(script);
        }
        Console.CancelKeyPress += delegate (object sender, ConsoleCancelEventArgs e) { child.Kill(); };
        child.BeginOutputReadLine();
        child.BeginErrorReadLine();
        child.WaitForExit();
        child.CancelOutputRead();
        child.CancelErrorRead();
        return child.ExitCode;
    }
    public static int RunBashScript2(string script)
    {
        string scriptPath = Path.Combine(Dirs.GetTempPath(), "tmp.sh");
        Util.Log(scriptPath, "scriptPath");
        Dirs.PrepareForFile(scriptPath);
        File.WriteAllText(scriptPath, script);
        //var PATH = $"{MSys2Dir}\\usr\\bin;{Environment.GetEnvironmentVariable("PATH")}";
        //Environment.SetEnvironmentVariable("PATH", PATH);
        var p_info = new ProcessStartInfo
        {
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = true,
            FileName = "bash.exe",
            Arguments = scriptPath,
        };
        //p_info.EnvironmentVariables["PATH"] = PATH;
        Process child = Process.Start(p_info);
        child.WaitForExit();
        File.Delete(scriptPath);
        return child.ExitCode;
    }
}
