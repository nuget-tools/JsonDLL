using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
namespace JsonDLL;
public class Busybox
{
    static string resDir = Internal.InstallResourceZip("res.zip");
    static Busybox()
    {
    }
    public static void Initialize()
    {
        ;
    }
    public static int RunBashScript(bool windowed, string script, string cwd = "")
    {
        string busyboxExe = Path.Combine(resDir, "busybox.exe");
        string tempFile = Path.GetTempFileName();
        //File.WriteAllText(tempFile, script);
        DLL1.API.CallOne("write_all_text_local8bit", new string[] { tempFile, script });
        string PATH = Environment.GetEnvironmentVariable("PATH");
        PATH = resDir + ";" + PATH;
        int result = ProcessRunner.RunProcess(windowed, busyboxExe, new string[] { "bash", tempFile }, cwd, new Dictionary<string, string> {
            { "PATH", PATH }
        });
        File.Delete(tempFile);
        return result;
    }
    public static bool LaunchBashScript(bool windowed, string script, string cwd = "")
    {
        string busyboxExe = Path.Combine(resDir, "busybox.exe");
        string tempFile = Path.GetTempFileName();
        //File.WriteAllText(tempFile, script);
        DLL1.API.CallOne("write_all_text_local8bit", new string[] { tempFile, script });
        string PATH = Environment.GetEnvironmentVariable("PATH");
        PATH = resDir + ";" + PATH;
        bool result = ProcessRunner.LaunchProcess(windowed, busyboxExe, new string[] { "bash", tempFile }, cwd, new Dictionary<string, string> {
            { "PATH", PATH }
        }, tempFile);
        return result;
    }
}
