using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace JsonDLL;

public class JsonAPI
{
    IntPtr handle = IntPtr.Zero;
    IntPtr funcPtr = IntPtr.Zero;
    delegate IntPtr proto_Call(IntPtr name, IntPtr args);
    public JsonAPI()
    {
        // for server
    }
    public JsonAPI(string dllSpec)
    {
#if false
        if (System.IO.Path.IsPathRooted(dllName))
        {
            this.handle = LoadLibraryExW(
                dllName,
                IntPtr.Zero,
                LoadLibraryFlags.LOAD_WITH_ALTERED_SEARCH_PATH
                );
        }
        else
        {
            this.handle = LoadLibraryW(dllName);
        }
#else
        Util.Log(dllSpec, "dllSpec");
        string dllPath = FindExePath(dllSpec);
        Util.Log(dllPath, "dllPath");
        this.handle = LoadLibraryExW(
            dllPath,
            IntPtr.Zero,
            LoadLibraryFlags.LOAD_WITH_ALTERED_SEARCH_PATH
            );
#endif
        if (dllPath is null) Environment.Exit(1);
        this.funcPtr = GetProcAddress(handle, "Call");
        if (this.funcPtr == IntPtr.Zero)
        {
            Util.Log("Call() not found");
            Environment.Exit(1);
        }
    }
    private static string FindExePath(string exe)
    {
        if (Path.IsPathRooted(exe)) return exe;
        var cwd = Directory.GetCurrentDirectory();
        exe = Environment.ExpandEnvironmentVariables(exe);
        var PATH = Environment.GetEnvironmentVariable("PATH") ?? "";
        PATH = $"{cwd};{PATH}";
        foreach (string test in PATH.Split(';'))
        {
            string path = test.Trim();
            if (!String.IsNullOrEmpty(path) && File.Exists(path = Path.Combine(path, exe)))
                return Path.GetFullPath(path);
        }
        return null;
    }
    public dynamic Call(dynamic name, dynamic args)
    {
        IntPtr pName = Util.StringToUTF8Addr(name);
        proto_Call pCall = (proto_Call)Marshal.GetDelegateForFunctionPointer(this.funcPtr, typeof(proto_Call));
        var argsJson = Util.ToJson(args);
        IntPtr pArgsJson = Util.StringToUTF8Addr(argsJson);
        IntPtr pResult = pCall(pName, pArgsJson);
        string result = Util.UTF8AddrToString(pResult);
        Marshal.FreeHGlobal(pName);
        Marshal.FreeHGlobal(pArgsJson);
        return Util.FromJson(result);
    }
    public dynamic CallOne(dynamic name, dynamic args)
    {
        var result = Call(name, args);
        if (result is null) return null;
        return result[0];
    }
    static ThreadLocal<IntPtr> HandleCallPtr = new ThreadLocal<IntPtr>();
    public IntPtr HandleCall(Type apiType, IntPtr nameAddr, IntPtr inputAddr)
    {
#if true
        //Tool.Print("HandleCall(1)");
        if (HandleCallPtr.Value != IntPtr.Zero)
        {
            Util.FreeHGlobal(HandleCallPtr.Value);
            HandleCallPtr.Value = IntPtr.Zero;
        }
        var name = Util.UTF8AddrToString(nameAddr);
        var input = Util.UTF8AddrToString(inputAddr);
        var args = Util.FromJson(input);
        MethodInfo mi = apiType.GetMethod(name);
        dynamic result = null;
        if (mi != null)
        {
            result = mi.Invoke(null, new object[] { args });
        }
        var output = Util.ToJson(result);
        HandleCallPtr.Value = Util.StringToUTF8Addr(output);
        return HandleCallPtr.Value;
#else
        return IntPtr.Zero;
#endif
    }
    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    internal static extern IntPtr LoadLibraryW(string lpFileName);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    public static extern IntPtr LoadLibraryExW(string dllToLoad, IntPtr hFile, LoadLibraryFlags flags);
    [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = false)]
    internal static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
    [System.Flags]
    public enum LoadLibraryFlags : uint
    {
        DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
        LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
        LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
        LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
        LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
        LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008,
        LOAD_LIBRARY_SEARCH_DLL_LOAD_DIR = 0x00000100,
        LOAD_LIBRARY_SEARCH_SYSTEM32 = 0x00000800,
        LOAD_LIBRARY_SEARCH_DEFAULT_DIRS = 0x00001000
    }
}
