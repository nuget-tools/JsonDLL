using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JsonDLL;

public class API
{
    public static dynamic add2(dynamic args)
    {
        try
        {
            var ary = Util.ToObject<double[]>(args);
            if (ary is null) return null;
            if (ary.Length != 2) return null;
            return new[] { ary[0] + ary[1] };
        }
        catch
        {
            return new object[] { };
        }
    }
}

static class APIHandler
{
    static Dictionary<int, JsonAPI> apiMap = new Dictionary<int, JsonAPI>();
    static APIHandler()
    {
        Initializer.Initialize();
    }
    [DllExport]
    [STAThread]
    public static int LoadAPI(IntPtr dllSpecAddr)
    {
        JsonDLL.JsonAPI jsonAPI = new JsonDLL.JsonAPI(Util.UTF8AddrToString(dllSpecAddr));
        int idx = apiMap.Keys.Count;
        apiMap[idx] = jsonAPI;
        return idx;
    }
    [DllExport]
    [STAThread]
    public static IntPtr CallAPI(int idx, IntPtr nameAddr, IntPtr inputAddr)
    {
        JsonDLL.JsonAPI jsonAPI = apiMap[idx];
        return jsonAPI.CallThru(nameAddr, inputAddr);
    }
    [DllExport]
    [STAThread]
    public static IntPtr Call(IntPtr nameAddr, IntPtr inputAddr)
    {
        JsonDLL.JsonAPI jsonAPI = new JsonDLL.JsonAPI();
        return jsonAPI.HandleCall(typeof(API), nameAddr, inputAddr);
    }
}
