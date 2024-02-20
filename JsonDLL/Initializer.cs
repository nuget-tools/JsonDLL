using System;
using System.IO;
using System.Reflection;

namespace JsonDLL;
static class Initializer
{
    public static string AssemblyDirectory
    {
        get
        {
            string codeBase = typeof(Initializer).Assembly.CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
    public static string AssemblyBaseDirectory = AssemblyDirectory;
    static Initializer()
    {
        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
        {
            string fileName = new AssemblyName(args.Name).Name + ".dll";
            string assemblyPath = Path.Combine(AssemblyBaseDirectory, fileName);
            var assembly = Assembly.LoadFile(assemblyPath);
            return assembly;
        };
    }
    public static void Initialize()
    {
        ;
    }
}