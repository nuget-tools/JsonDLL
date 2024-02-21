#if true
using Jint;
using System.Reflection;

namespace JsonDLL;

public class JintScript
{
    public static Jint.Engine CreateEngine(params Assembly[] list)
    {
        var engine = new Jint.Engine(cfg =>
        {
            //cfg.AllowClr(typeof(Global.Util).Assembly);
            cfg.AllowClr();
            for (int i = 0; i < list.Length; i++)
            {
                cfg.AllowClr(list[i]);
            }
        });
        engine.SetValue("_console", new JintScriptConsole());
        engine.Execute(@"
var print = _console.print;
var log = _console.log;
");
        return engine;
    }
}

internal class JintScriptConsole
{
    public void print(dynamic x, string? title = null)
    {
        Util.Print(x, title);
    }
    public void log(dynamic x, string? title = null)
    {
        Util.Log(x, title);
    }
}
#endif
