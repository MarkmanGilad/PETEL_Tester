using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PETEL_VPL;

namespace PETEL_MainTester_V2
{
    internal static class Program
    {
        private const string ProtocolPrefix = "PETEL_V2|";

        private static int Main(string[] args)
        {
            var testCasesType = Type.GetType("PETEL_VPL.TestCases")
                ?? AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType("PETEL_VPL.TestCases", throwOnError: false, ignoreCase: false))
                    .FirstOrDefault(t => t != null);

            if (testCasesType == null)
            {
                Console.WriteLine("Comment :=>>Framework error: type PETEL_VPL.TestCases not found: failure. 0 points\n");
                Console.WriteLine("Grade :=>> 0");
                return 2;
            }

            var testMethods = testCasesType
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.ReturnType == typeof(void))
                .Where(m =>
                {
                    var ps = m.GetParameters();
                    return ps.Length == 1 && ps[0].ParameterType == typeof(VPLTester);
                })
                .OrderBy(m => m.Name, StringComparer.Ordinal)
                .ToList();

            int total = 0;
            var outputBlocks = new List<string>();

            foreach (var method in testMethods)
            {
                if (method.Name.StartsWith("Code_", StringComparison.Ordinal))
                {
                    var (pts, txt) = RunInProcessCodeTest(method);
                    total += pts;
                    outputBlocks.Add(txt);
                }
                else
                {
                    var (pts, txt) = RunInRunner(method.Name);
                    total += pts;
                    outputBlocks.Add(txt);
                }
            }

            foreach (var block in outputBlocks)
                Console.Write(block);

            Console.WriteLine($"Grade :=>> {total}");
            return 0;
        }

        private static (int points, string text) RunInProcessCodeTest(MethodInfo method)
        {
            try
            {
                var tester = TestCases.CreateTester();
                tester.ShowDetails = false;
                tester.InitializeCodeAnalyzer();

                method.Invoke(null, new object[] { tester });
                return (tester.GetGrade(), tester.FormatResponse());
            }
            catch (TargetInvocationException tie)
            {
                return (0, "Comment :=>>" + method.Name + ": failure. 0 points\n<|--\n" + (tie.InnerException?.Message ?? tie.Message) + "\n--|>\n\n");
            }
            catch (Exception ex)
            {
                return (0, "Comment :=>>" + method.Name + ": failure. 0 points\n<|--\n" + ex.Message + "\n--|>\n\n");
            }
        }

        private static (int points, string text) RunInRunner(string methodName)
        {
            try
            {
                var runnerCmd = ResolveRunnerCommand();
                if (runnerCmd == null)
                    return (0, $"Comment :=>>{methodName}: failure. 0 points\n<|--\nRunner.exe not found\n--|>\n\n");

                var psi = new ProcessStartInfo
                {
                    FileName = runnerCmd.Value.FileName,
                    Arguments = $"{runnerCmd.Value.Arguments} --method {EscapeArg(methodName)} --probeDir {EscapeArg(AppContext.BaseDirectory)}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = AppContext.BaseDirectory,
                };

                using var p = Process.Start(psi);
                if (p == null)
                    return (0, $"Comment :=>>{methodName}: failure. 0 points\n<|--\nFailed to start Runner.exe\n--|>\n\n");

                var stdoutTask = p.StandardOutput.ReadToEndAsync();
                var stderrTask = p.StandardError.ReadToEndAsync();

                const int timeoutMs = 5000;
                if (!p.WaitForExit(timeoutMs))
                {
                    try { p.Kill(entireProcessTree: true); } catch { }
                    return (0, $"Comment :=>>{methodName}: failure. 0 points\n<|--\nRunner timed out (possible infinite recursion/loop).\n--|>\n\n");
                }

                Task.WaitAll(new Task[] { stdoutTask, stderrTask }, timeoutMs);

                string stdout = stdoutTask.IsCompleted ? stdoutTask.Result : string.Empty;
                string stderr = stderrTask.IsCompleted ? stderrTask.Result : string.Empty;

                var line = stdout
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault(l => l.StartsWith(ProtocolPrefix, StringComparison.Ordinal));

                if (string.IsNullOrWhiteSpace(line))
                {
                    var exitCode = p.ExitCode;
                    var isStackOverflow = exitCode == unchecked((int)0xC00000FD) || exitCode == 139;
                    var msg = isStackOverflow
                        ? "Runner crashed (stack overflow / infinite recursion)."
                        : (string.IsNullOrWhiteSpace(stderr) ? "Runner produced no protocol output." : "Runner stderr: " + stderr.Trim());

                    return (0, $"Comment :=>>{methodName}: failure. 0 points\n<|--\n{msg}\n--|>\n\n");
                }

                var parts = line.Split('|');
                if (parts.Length < 3)
                    return (0, $"Comment :=>>{methodName}: failure. 0 points\n<|--\nMalformed runner packet\n--|>\n\n");

                if (!int.TryParse(parts[1], out int pts))
                    pts = 0;

                string text;
                try
                {
                    text = Encoding.UTF8.GetString(Convert.FromBase64String(parts[2]));
                }
                catch
                {
                    text = "Comment :=>>" + methodName + ": failure. 0 points\n<|--\nInvalid base64 payload\n--|>\n\n";
                    pts = 0;
                }

                return (pts, text);
            }
            catch (Exception ex)
            {
                return (0, $"Comment :=>>{methodName}: failure. 0 points\n<|--\n{ex.Message}\n--|>\n\n");
            }
        }

        private readonly record struct RunnerCommand(string FileName, string Arguments);

        private static RunnerCommand? ResolveRunnerCommand()
        {
            var baseDir = AppContext.BaseDirectory;

            string[] exeCandidates =
            [
                Path.Combine(baseDir, "Runner.exe"),
                Path.Combine(baseDir, "PETEL_Runner_V2.exe"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "PETEL_Runner_V2", "bin", "Debug", "net8.0", "PETEL_Runner_V2.exe")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "PETEL_Runner_V2", "bin", "Release", "net8.0", "PETEL_Runner_V2.exe")),
            ];

            foreach (var c in exeCandidates)
            {
                try
                {
                    if (File.Exists(c))
                        return new RunnerCommand(c, string.Empty);
                }
                catch { }
            }

            string[] dllCandidates =
            [
                Path.Combine(baseDir, "PETEL_Runner_V2.dll"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "PETEL_Runner_V2", "bin", "Debug", "net8.0", "PETEL_Runner_V2.dll")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "PETEL_Runner_V2", "bin", "Release", "net8.0", "PETEL_Runner_V2.dll")),
            ];

            foreach (var c in dllCandidates)
            {
                try
                {
                    if (File.Exists(c))
                        return new RunnerCommand("dotnet", '"' + c + '"');
                }
                catch { }
            }

            return null;
        }

        private static string? LocateRunnerExecutable()
        {
            var baseDir = AppContext.BaseDirectory;

            string[] candidates =
            [
                Path.Combine(baseDir, "Runner.exe"),
                Path.Combine(baseDir, "PETEL_Runner_V2.exe"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "PETEL_Runner_V2", "bin", "Debug", "net8.0", "PETEL_Runner_V2.exe")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "PETEL_Runner_V2", "bin", "Release", "net8.0", "PETEL_Runner_V2.exe")),
            ];

            foreach (var c in candidates)
            {
                try
                {
                    if (File.Exists(c))
                        return c;
                }
                catch { }
            }

            return null;
        }

        private static string EscapeArg(string value)
        {
            if (value.Contains(' ') || value.Contains('"'))
                return '"' + value.Replace("\"", "\\\"") + '"';
            return value;
        }
    }
}
