using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.InteropServices;
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
    public JsonAPI(string dllName)
    {
        //Tool.Print(dllName, "dllName");
        // for client
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
        this.funcPtr = GetProcAddress(handle, "Call");
    }
    public dynamic Call(dynamic name, dynamic args)
    {
        IntPtr pName = Tool.StringToUTF8Addr(name);
        proto_Call pCall = (proto_Call)Marshal.GetDelegateForFunctionPointer(this.funcPtr, typeof(proto_Call));
        var argsJson = Tool.ToJson(args);
        IntPtr pArgsJson = Tool.StringToUTF8Addr(argsJson);
        IntPtr pResult = pCall(pName, pArgsJson);
        string result = Tool.UTF8AddrToString(pResult);
        Marshal.FreeHGlobal(pName);
        Marshal.FreeHGlobal(pArgsJson);
        return Tool.FromJson(result);
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
            Tool.FreeHGlobal(HandleCallPtr.Value);
            HandleCallPtr.Value = IntPtr.Zero;
        }
        var name = Tool.UTF8AddrToString(nameAddr);
        var input = Tool.UTF8AddrToString(inputAddr);
        var args = Tool.FromJson(input);
        MethodInfo mi = apiType.GetMethod(name);
        dynamic result = null;
        if (mi != null)
        {
            result = mi.Invoke(null, new object[] { args });
        }
        var output = Tool.ToJson(result);
        HandleCallPtr.Value = Tool.StringToUTF8Addr(output);
        return HandleCallPtr.Value;
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
