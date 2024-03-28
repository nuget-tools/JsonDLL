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
    static ProcessRunner()
    {
    }
    public static void Initialize()
    {
        ;
    }
    public static void HandleEvents()
    {
        DLL1.API.CallOne("process_events", null);
    }
    public static int RunProcess(bool windowed, string exePath, string[] args, string cwd = "", Dictionary<string, string> env = null)
    {
        int result = (int)DLL1.API.CallOne("run_process", new object[] { windowed, exePath, args, cwd, env });
        return result;
    }
    public static bool LaunchProcess(bool windowed, string exePath, string[] args, string cwd = "", Dictionary<string, string> env = null, string fileToDelete = "")
    {
        bool result = (bool)DLL1.API.CallOne("launch_process", new object[] { windowed, exePath, args, cwd, env, fileToDelete });
        return result;
    }
}
