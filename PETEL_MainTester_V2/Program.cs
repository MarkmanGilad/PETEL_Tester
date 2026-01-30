using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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
                // Single source of truth: teachers configure the assignment inside TestCases.cs
                var tester = TestCases.CreateTester(showDetails: false);

                // allow analyzer usage for Code_ tests
                tester.InitializeCodeAnalyzer();

                method.Invoke(null, [tester]);
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
                var runnerExe = LocateRunnerExecutable();
                if (runnerExe == null)
                    return (0, $"Comment :=>>{methodName}: failure. 0 points\n<|--\nRunner.exe not found\n--|>\n\n");

                var psi = new ProcessStartInfo
                {
                    FileName = runnerExe,
                    Arguments = $"--method {EscapeArg(methodName)}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = AppContext.BaseDirectory,
                };

                using var p = Process.Start(psi);
                if (p == null)
                    return (0, $"Comment :=>>{methodName}: failure. 0 points\n<|--\nFailed to start Runner.exe\n--|>\n\n");

                string stdout = p.StandardOutput.ReadToEnd();
                string stderr = p.StandardError.ReadToEnd();
                p.WaitForExit(5000);

                var line = stdout
                    .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                    .FirstOrDefault(l => l.StartsWith(ProtocolPrefix, StringComparison.Ordinal));

                if (string.IsNullOrWhiteSpace(line))
                {
                    var msg = string.IsNullOrWhiteSpace(stderr) ? "Runner produced no protocol output." : "Runner stderr: " + stderr.Trim();
                    return (0, $"Comment :=>>{methodName}: failure. 0 points\n<|--\n{msg}\n--|>\n\n");
                }

                // PETEL_V2|<points>|<base64>
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

        private static string? LocateRunnerExecutable()
        {
            // Expected to be copied alongside MainTester in VPL.
            // Local dev: look in bin directories.
            var baseDir = AppContext.BaseDirectory;

            string[] candidates =
            [
                Path.Combine(baseDir, "Runner.exe"),
                Path.Combine(baseDir, "PETEL_Runner_V2.exe"),
                // dotnet build outputs
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "PETEL_Runner_V2", "bin", "Debug", "net8.0", "PETEL_Runner_V2.exe")),
                Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "PETEL_Runner_V2", "bin", "Debug", "net8.0", "PETEL_Runner_V2.exe")),
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
            // Basic quoting for method names (no spaces expected, but safe).
            if (value.Contains(' ') || value.Contains('"'))
                return '"' + value.Replace("\"", "\\\"") + '"';
            return value;
        }
    }
}
