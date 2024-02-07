using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static JsonDLL.JsonAPI;

namespace JsonDLL;

public class ProcessRunner
{
    public static JsonAPI API = null;
    //static SynchronizationContext syncContext = SynchronizationContext.Current;
    //static System.Threading.Timer Timer = null;
    //static Form Form = new Form();
    static ProcessRunner()
    {
        //SynchronizationContext syncContext = SynchronizationContext.Current;
        int bit = IntPtr.Size * 8;
        var dir = Dirs.ProfilePath("JavaCommons Technologies", "JsonDLL");
        dir = Path.Combine(dir, $"x{bit}");
        var dllBytes = Util.ResourceAsBytes(typeof(ProcessRunner).Assembly, $"JsonDLL:dll1-x{bit}.dll");
        SHA256 crypto = new SHA256CryptoServiceProvider();
        byte[] hashValue = crypto.ComputeHash(dllBytes);
        string sha256 = String.Join("", hashValue.Select(x => x.ToString("x2")).ToArray());
        string dllName = $"dll1-{sha256}.dll";
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
        Util.Log($"Loading {dllPath}...");
        API = new JsonAPI(dllPath);
    }
    public static void Initialize()
    {
        ;
    }
    public static void HandleEvents()
    {
        API.CallOne("process_events", null);
    }
    public static int RunProcess(bool windowed, string exePath, string[] args, string cwd = "", Dictionary<string, string> env = null)
    {
        int result = (int)API.CallOne("run_process", new object[] { windowed, exePath, args, cwd, env });
        return result;
    }
    public static bool LaunchProcess(bool windowed, string exePath, string[] args, string cwd = "", Dictionary<string, string> env = null, string fileToDelete = "")
    {
        bool result = (bool)API.CallOne("launch_process", new object[] { windowed, exePath, args, cwd, env, fileToDelete });
        return result;
    }
}
