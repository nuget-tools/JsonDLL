﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;

namespace JsonDLL;

public class JavaScriptServer
{
    protected Jint.Engine engine = JintScript.CreateEngine();
    protected List<Assembly> asmList = new List<Assembly>();
    public JavaScriptServer()
    {
    }
    public void Init(Assembly[] memAsmList = null, string[] asmSpecList = null/*, string cwd = null*/)
    {
        Console.Error.WriteLine($"[myjs] Initializing {typeof(JavaScriptServer).Assembly.Location}...");
        asmList.Clear();
        if (memAsmList != null )
        {
            foreach (var asm in memAsmList)
            {
                asmList.Add(asm);
            }
        }
        if (asmSpecList != null )
        {
            foreach (var asmSpec in asmSpecList)
            {
                var asm = LoadAssemblyForSpec(asmSpec/*, cwd*/);
                asmList.Add(asm);
            }
        }
        engine = JintScript.CreateEngine(asmList.ToArray());
    }
    protected Assembly LoadAssemblyForSpec(string asmSpec, string cwd = null)
    {
        Console.Error.WriteLine($"[myjs] //+ {asmSpec}");
        string realPath = null;
        if (cwd != null)
        {
            realPath = Util.FindExePath(asmSpec, cwd);
        }
        else
        {
            realPath = Util.FindExePath(asmSpec);
        }
        if (realPath is null)
        {
            string error = $"{asmSpec} not found";
            Console.Error.WriteLine(error);
            throw new Exception(error);
        }
        Console.Error.WriteLine($"[myjs] Loading from {realPath}...");
        var asm = Assembly.LoadFrom(realPath);
        return asm;
    }
    public void SetValue(string name, dynamic? value)
    {
        engine.Execute($"globalThis.{name}=({Util.ToJson(value)})");
    }
    public dynamic? GetValue(string name)
    {
        return engine.GetValue(name).ToNewton();
    }
    public void Execute(string script, object[]? vars = null)
    {
        if (vars is null) vars = new object[] { };
        for (int i = 0; i < vars.Length; i++)
        {
            SetValue($"${i + 1}", vars[i]);
        }
        engine.Execute(script);
        for (int i = 0; i < vars.Length; i++)
        {
            //SetValue($"${i + 1}", null);
            engine.Execute($"delete globalThis.${i + 1};");
        }
    }
    public dynamic? Evaluate(string script, object[]? vars = null)
    {
        if (vars is null) vars = new object[] { };
        for (int i = 0; i < vars.Length; i++)
        {
            SetValue($"${i + 1}", vars[i]);
        }
        var result = engine.Evaluate(script).ToNewton();
        for (int i = 0; i < vars.Length; i++)
        {
            //SetValue($"${i + 1}", null);
            engine.Execute($"delete globalThis.${i + 1};");
        }
        return result;
    }
    public dynamic? Call(string name, object[]? vars = null)
    {
        if (vars is null) vars = new object[] { };
        string script = name + "(";
        for (int i = 0; i < vars.Length; i++)
        {
            if (i > 0) script += ", ";
            script += $"${i + 1}";
        }
        script += ")";
        var result = Evaluate(script, vars);
        for (int i = 0; i < vars.Length; i++)
        {
            //SetValue($"${i + 1}", null);
            engine.Execute($"delete globalThis.${i + 1};");
        }
        return result;
    }
}
