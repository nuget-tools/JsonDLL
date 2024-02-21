using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace JsonDLL;

public class JsonAPI
{
    static Dictionary<int, JsonAPI> apiMap = new Dictionary<int, JsonAPI>();
    IntPtr handle = IntPtr.Zero;
    IntPtr funcPtr = IntPtr.Zero;
    delegate IntPtr proto_Call(IntPtr name, IntPtr args);
    public JsonAPI()
    {
        // for server
    }
    public JsonAPI(string dllSpec)
    {
        Util.Log(dllSpec, "dllSpec");
        string dllPath = Util.FindExePath(dllSpec);
        Util.Log(dllPath, "dllPath");
        if (dllPath is null) Environment.Exit(1);
        this.LoadDll(dllPath);
    }
    public JsonAPI(string dllSpec, string cwd)
    {
        Util.Log(dllSpec, "dllSpec");
        string dllPath = Util.FindExePath(dllSpec, cwd);
        Util.Log(dllPath, "dllPath");
        if (dllPath is null) Environment.Exit(1);
        this.LoadDll(dllPath);
    }
    public JsonAPI(string dllSpec, Assembly assembly)
    {
        Util.Log(dllSpec, "dllSpec");
        string dllPath = Util.FindExePath(dllSpec, assembly);
        Util.Log(dllPath, "dllPath");
        if (dllPath is null) Environment.Exit(1);
        this.LoadDll(dllPath);
    }
    private void LoadDll(string dllPath)
    {
        this.handle = LoadLibraryExW(
            dllPath,
            IntPtr.Zero,
            LoadLibraryFlags.LOAD_WITH_ALTERED_SEARCH_PATH
            );
        if (this.handle == IntPtr.Zero)
        {
            Util.Log($"DLL not loaded: {dllPath}");
            Environment.Exit(1);
        }
        this.funcPtr = GetProcAddress(handle, "Call");
        if (this.funcPtr == IntPtr.Zero)
        {
            Util.Log("Call() not found");
            Environment.Exit(1);
        }
    }
#if false
    public IntPtr CallThru(IntPtr nameAddr, IntPtr argsAddr)
    {
        proto_Call pCall = (proto_Call)Marshal.GetDelegateForFunctionPointer(this.funcPtr, typeof(proto_Call));
        return pCall(nameAddr, argsAddr);
    }
#endif
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
    }
    public int LoadAPI(IntPtr dllSpecAddr)
    {
        JsonAPI jsonAPI = new JsonAPI(Util.UTF8AddrToString(dllSpecAddr));
        int idx = apiMap.Keys.Count;
        apiMap[idx] = jsonAPI;
        return idx;
    }
    public IntPtr CallAPI(int idx, IntPtr nameAddr, IntPtr inputAddr)
    {
        JsonAPI jsonAPI = apiMap[idx];
        proto_Call pCall = (proto_Call)Marshal.GetDelegateForFunctionPointer(jsonAPI.funcPtr, typeof(proto_Call));
        return pCall(nameAddr, inputAddr);
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
