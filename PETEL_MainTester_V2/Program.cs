using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace PETEL_MainTester_V2
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            // Locate PETEL_Runner_V2.exe relative to this assembly
            string runnerPath = FindRunnerExecutable();
            
            if (string.IsNullOrEmpty(runnerPath) || !File.Exists(runnerPath))
            {
                Console.WriteLine("Comment :=>>Framework error: PETEL_Runner_V2.exe not found: failure. 0 points\n");
                Console.WriteLine("Grade :=>> 0");
                return 1;
            }

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = runnerPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(typeof(Program).Assembly.Location)
                };

                using var process = Process.Start(psi);
                if (process == null)
                {
                    Console.WriteLine("Comment :=>>Framework error: failed to start runner: failure. 0 points\n");
                    Console.WriteLine("Grade :=>> 0");
                    return 2;
                }

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                // Emit runner output directly
                if (!string.IsNullOrEmpty(output))
                    Console.Write(output);

                if (!string.IsNullOrEmpty(error))
                    Console.Error.Write(error);

                return process.ExitCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Comment :=>>Framework error: " + ex.Message + ": failure. 0 points\n");
                Console.WriteLine("Grade :=>> 0");
                return 3;
            }
        }

        private static string FindRunnerExecutable()
        {
            var baseDir = AppContext.BaseDirectory;
            
            // Try sibling bin folder (common in VS multi-project solutions)
            string[] candidates = 
            {
                // Same directory (published together)
                Path.Combine(baseDir, "PETEL_Runner_V2.exe"),
                // Sibling Debug output
                Path.Combine(baseDir, "..", "..", "..", "PETEL_Runner_V2", "bin", "Debug", "net8.0", "PETEL_Runner_V2.exe"),
                // Sibling Release output
                Path.Combine(baseDir, "..", "..", "..", "PETEL_Runner_V2", "bin", "Release", "net8.0", "PETEL_Runner_V2.exe"),
                // Absolute path attempt (Development mode)
                Path.Combine(Directory.GetParent(baseDir).Parent.Parent.Parent.Parent.FullName, "PETEL_Runner_V2", "bin", "Debug", "net8.0", "PETEL_Runner_V2.exe")
            };

            foreach (var candidate in candidates)
            {
                try
                {
                    var fullPath = Path.GetFullPath(candidate);
                    if (File.Exists(fullPath))
                        return fullPath;
                }
                catch
                {
                    // ignore and continue
                }
            }

            return null;
        }
    }
}
