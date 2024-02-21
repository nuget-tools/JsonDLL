// https://stackoverflow.com/questions/15604014/no-console-output-when-using-allocconsole-and-target-architecture-x86
using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.InteropServices;
using System;
using System.Text;
using System.Security.Cryptography;
using static JsonDLL.JsonAPI;

namespace JsonDLL;

internal static class WinConsole
{
#if false
    delegate void proto_AfterAllocConsole();
    static proto_AfterAllocConsole AfterAllocConsole;
    static WinConsole()
    {
        SynchronizationContext syncContext = SynchronizationContext.Current;
        int bit = IntPtr.Size * 8;
        var dir = Dirs.ProfilePath("JavaCommons Technologies", "JsonDLL");
        dir = Path.Combine(dir, $"x{bit}");
        var dllBytes = Util.ResourceAsBytes(typeof(ProcessRunner).Assembly, $"JsonDLL:AfterAllocConsole-x{bit}.dll");
        SHA256 crypto = new SHA256CryptoServiceProvider();
        byte[] hashValue = crypto.ComputeHash(dllBytes);
        string sha256 = String.Join("", hashValue.Select(x => x.ToString("x2")).ToArray());
        string dllName = $"AfterAllocConsole-{sha256}.dll";
        var dllPath = Path.Combine(dir, dllName);
        if (File.Exists(dllPath))
        {
            Util.Log($"{dllPath} is installed");
        }
        else
        {
            Dirs.PrepareForFile(dllPath);
            File.WriteAllBytes(dllPath, dllBytes);
            Util.Log($"{dllPath} has been written");
        }
        Util.Log($"Loading {dllPath}...");
        IntPtr handle = LoadLibraryExW(
            dllPath,
            IntPtr.Zero,
            LoadLibraryFlags.LOAD_WITH_ALTERED_SEARCH_PATH
            );
        IntPtr funcPtr = GetProcAddress(handle, "AfterAllocConsole");
        AfterAllocConsole = (proto_AfterAllocConsole)Marshal.GetDelegateForFunctionPointer(funcPtr, typeof(proto_AfterAllocConsole));
    }
#endif
    static public void Initialize(bool alwaysCreateNewConsole = true)
    {
        bool consoleAttached = true;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            if (alwaysCreateNewConsole
                || (AttachConsole(ATTACH_PARRENT) == 0
                && Marshal.GetLastWin32Error() != ERROR_ACCESS_DENIED))
            {
                consoleAttached = AllocConsole() != 0;
            }
        }
        else
        {
            consoleAttached = true;
        }
        var stdout = new StreamWriter(Console.OpenStandardOutput(), Encoding.Default);
        stdout.AutoFlush = true;
        Console.SetOut(stdout);
        var stderr = new StreamWriter(Console.OpenStandardError(), Encoding.Default);
        stderr.AutoFlush = true;
        Console.SetError(stderr);
#if false
        AfterAllocConsole();
#endif
    }

    static public void Deinitialize()
    {
        Console.SetOut(TextWriter.Null);
        Console.SetError(TextWriter.Null);
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            FreeConsole();
        }
    }

#if false
    static FileStream CreateFileStream(string name, uint desAccess, uint shareMode, FileAccess fileAccess)
    {
        var fileHandle = CreateFileW(name, desAccess, shareMode, 0, (uint)FileMode.Open, (uint)FileAttributes.Normal, 0);
        var file = new SafeFileHandle(fileHandle, true);
        return !file.IsInvalid ? new FileStream(file, fileAccess) : null;
    }
#endif

    #region Win API Functions and Constants
    [DllImport("kernel32.dll",
        EntryPoint = "AllocConsole",
        SetLastError = true,
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    private static extern int AllocConsole();

    [DllImport("kernel32.dll",
        EntryPoint = "AttachConsole",
        SetLastError = true,
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    private static extern UInt32 AttachConsole(UInt32 dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern bool FreeConsole();

    private const UInt32 ERROR_ACCESS_DENIED = 5;
    private const UInt32 ATTACH_PARRENT = 0xFFFFFFFF;
#if false
    const uint
        GENERIC_WRITE = 0x40000000,
        GENERIC_READ = 0x80000000,
        FILE_SHARE_READ = 1,
        FILE_SHARE_WRITE = 2;
    [DllImport("kernel32.dll",
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.StdCall)]
    public static extern
    nint CreateFileW(string fileName, uint desAccess, uint shareMode, nint securityAttb, uint creationDispose, uint flagsAndAttb, nint templateFile);
#endif
#endregion
}
