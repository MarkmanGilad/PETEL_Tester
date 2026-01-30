using System;
using System.Diagnostics;
using Unit4;

const int timeoutMs = 30000;

if (args.Contains("--child"))
{
    RunChild();
    return;
}

RunParent();

void RunParent()
{
    var exe = Environment.ProcessPath;
    if (string.IsNullOrWhiteSpace(exe))
    {
        Console.WriteLine("Unable to resolve current executable path.");
        return;
    }

    var psi = new ProcessStartInfo
    {
        FileName = exe,
        Arguments = "--child",
        RedirectStandardOutput = false,
        RedirectStandardError = false,
        UseShellExecute = false,
        CreateNoWindow = false
    };

    using var p = Process.Start(psi);
    if (p == null)
    {
        Console.WriteLine("Failed to start child process.");
        return;
    }

    var sw = Stopwatch.StartNew();

    if (!p.WaitForExit(timeoutMs))
    {
        try { p.Kill(entireProcessTree: true); } catch { }
        Console.WriteLine($"Child timed out after {sw.Elapsed.TotalSeconds:F3}s.");
        return;
    }

    sw.Stop();

    int exitCode = p.ExitCode;
    bool isStackOverflow = exitCode == unchecked((int)0xC00000FD) || exitCode == 139;

    Console.WriteLine($"Child exit code: 0x{exitCode:X8}");
    Console.WriteLine($"Elapsed: {sw.Elapsed.TotalSeconds:F3}s");
    Console.WriteLine(isStackOverflow ? "Detected stack overflow." : "No stack overflow detected.");
}

void RunChild()
{
    Console.WriteLine($"Child Is64BitProcess={Environment.Is64BitProcess}");

    int[] arr = { 1, 5, 2, 5, 3, 5, 4, 5, 6, 7, 8, 9, 10 };
    Node<int> list = Unit4Helper.BuildNodeList(arr);

    Console.WriteLine(StudentAnswer.CountValues(list, 5));
}