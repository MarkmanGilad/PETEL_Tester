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

            var timeoutMs = GetRunnerTimeoutMs();

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
                    var (pts, txt) = RunInRunner(method.Name, timeoutMs);
                    total += pts;
                    outputBlocks.Add(txt);
                }
            }

            foreach (var block in outputBlocks)
                Console.Write(block);

            Console.WriteLine($"Grade :=>> {total}");
            return 0;
        }

        private static int GetRunnerTimeoutMs()
        {
            var tester = TestCases.CreateTester();
            var timeoutMs = tester.TimeoutMilliseconds;
            return timeoutMs > 0 ? timeoutMs : 500;
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
                return Fail(method.Name, tie.InnerException?.Message ?? tie.Message);
            }
            catch (Exception ex)
            {
                return Fail(method.Name, ex.Message);
            }
        }

        private static (int points, string text) RunInRunner(string methodName, int timeoutMs)
        {
            try
            {
                var runnerCmd = ResolveRunnerCommand();
                if (runnerCmd == null)
                    return Fail(methodName, "Runner.exe not found");

                using var p = StartRunner(runnerCmd.Value, methodName);
                if (p == null)
                    return Fail(methodName, "Failed to start Runner.exe");

                var stdoutTask = p.StandardOutput.ReadToEndAsync();
                var stderrTask = p.StandardError.ReadToEndAsync();

                if (!p.WaitForExit(timeoutMs))
                {
                    try { p.Kill(entireProcessTree: true); } catch { }
                    return Fail(methodName, "Runner timed out (possible infinite recursion/loop).");
                }

                Task.WaitAll(new Task[] { stdoutTask, stderrTask }, timeoutMs);

                string stdout = stdoutTask.IsCompleted ? stdoutTask.Result : string.Empty;
                string stderr = stderrTask.IsCompleted ? stderrTask.Result : string.Empty;

                var packet = TryReadPacket(stdout);
                if (packet == null)
                {
                    var exitCode = p.ExitCode;
                    var isStackOverflow = exitCode == unchecked((int)0xC00000FD) || exitCode == 139;
                    var msg = isStackOverflow
                        ? "Runner crashed (stack overflow / infinite recursion)."
                        : (string.IsNullOrWhiteSpace(stderr) ? "Runner produced no protocol output." : "Runner stderr: " + stderr.Trim());

                    return Fail(methodName, msg);
                }

                if (!int.TryParse(packet.Value.points, out int pts))
                    pts = 0;

                string text;
                try
                {
                    text = Encoding.UTF8.GetString(Convert.FromBase64String(packet.Value.payload));
                }
                catch
                {
                    text = $"Comment :=>>{methodName}: failure. 0 points\n<|--\nInvalid base64 payload\n--|>\n\n";
                    pts = 0;
                }

                return (pts, text);
            }
            catch (Exception ex)
            {
                return Fail(methodName, ex.Message);
            }
        }

        private static Process? StartRunner(RunnerCommand cmd, string methodName)
        {
            var psi = new ProcessStartInfo
            {
                FileName = cmd.FileName,
                Arguments = $"{cmd.Arguments} --method {EscapeArg(methodName)} --probeDir {EscapeArg(AppContext.BaseDirectory)}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = AppContext.BaseDirectory,
            };

            return Process.Start(psi);
        }

        private static (string points, string payload)? TryReadPacket(string stdout)
        {
            if (string.IsNullOrWhiteSpace(stdout))
                return null;

            using var reader = new StringReader(stdout);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!line.StartsWith(ProtocolPrefix, StringComparison.Ordinal))
                    continue;

                var parts = line.Split('|');
                if (parts.Length < 3)
                    return null;

                return (parts[1], parts[2]);
            }

            return null;
        }

        private static (int points, string text) Fail(string methodName, string message)
        {
            return (0, $"Comment :=>>{methodName}: failure. 0 points\n<|--\n{message}\n--|>\n\n");
        }

        private readonly record struct RunnerCommand(string FileName, string Arguments);

        private static RunnerCommand? ResolveRunnerCommand()
        {
            foreach (var c in EnumerateRunnerCommands())
            {
                try
                {
                    if (File.Exists(c.FileName))
                        return c;
                }
                catch { }
            }

            return null;
        }

        private static IEnumerable<RunnerCommand> EnumerateRunnerCommands()
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
                yield return new RunnerCommand(c, string.Empty);

            string[] dllCandidates =
            [
                Path.Combine(baseDir, "PETEL_Runner_V2.dll"),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "PETEL_Runner_V2", "bin", "Debug", "net8.0", "PETEL_Runner_V2.dll")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "PETEL_Runner_V2", "bin", "Release", "net8.0", "PETEL_Runner_V2.dll")),
            ];

            foreach (var c in dllCandidates)
                yield return new RunnerCommand("dotnet", '"' + c + '"');
        }

        private static string EscapeArg(string value)
        {
            if (value.Contains(' ') || value.Contains('"'))
                return '"' + value.Replace("\"", "\\\"") + '"';
            return value;
        }
    }
}
