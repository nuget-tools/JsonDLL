using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace JsonDLL;
public class DLL1
{
    public static JsonAPI API = null;
    static DLL1()
    {
        string dllPath = Internal.InstallResourceDll("dll1");
        //Util.Log($"Loading {dllPath}...");
        API = new JsonAPI(dllPath);
    }
}
