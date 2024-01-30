using System;
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
        // for client
        this.handle = LoadLibraryW(dllName);
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
    [DllImport("kernel32", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = false)]
    internal static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
}
