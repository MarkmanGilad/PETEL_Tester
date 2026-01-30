using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using PETEL_VPL;

namespace PETEL_Runner_V2
{
    internal static class Program
    {
        private const string ProtocolPrefix = "PETEL_V2|";

        private static int Main(string[] args)
        {
            string? methodName = GetArgValue(args, "--method");
            if (string.IsNullOrWhiteSpace(methodName))
            {
                EmitPacket(0, "Runner error: missing required argument --method <name>.");
                return 2;
            }

            try
            {
                string? probeDir = GetArgValue(args, "--probeDir");
                LoadUserAssemblies(probeDir);

                var testCasesType = Type.GetType("PETEL_VPL.TestCases")
                    ?? AppDomain.CurrentDomain.GetAssemblies()
                        .Select(a => a.GetType("PETEL_VPL.TestCases", throwOnError: false, ignoreCase: false))
                        .FirstOrDefault(t => t != null);

                if (testCasesType == null)
                {
                    EmitPacket(0, "Runner error: type PETEL_VPL.TestCases not found.");
                    return 3;
                }

                var method = testCasesType.GetMethod(
                    methodName,
                    BindingFlags.Public | BindingFlags.Static,
                    binder: null,
                    types: new[] { typeof(VPLTester) },
                    modifiers: null);

                if (method == null || method.ReturnType != typeof(void))
                {
                    EmitPacket(0, $"Runner error: method '{methodName}(VPLTester)' not found.");
                    return 4;
                }

                var createTesterMethod = testCasesType.GetMethod("CreateTester", BindingFlags.Public | BindingFlags.Static);
                if (createTesterMethod == null)
                {
                    EmitPacket(0, "Runner error: CreateTester method not found in TestCases.");
                    return 5;
                }

                var tester = createTesterMethod.Invoke(null, null) as VPLTester;
                if (tester == null)
                {
                    EmitPacket(0, "Runner error: CreateTester did not return a VPLTester.");
                    return 6;
                }

                tester.ShowDetails = false;

                var originalOut = Console.Out;
                var suppressed = new StringWriter();
                Console.SetOut(suppressed);
                try
                {
                    method.Invoke(null, new object[] { tester });
                }
                finally
                {
                    Console.SetOut(originalOut);
                    suppressed.Dispose();
                }

                EmitPacket(tester.GetGrade(), tester.FormatResponse());
                return 0;
            }
            catch (TargetInvocationException tie)
            {
                EmitPacket(0, "Runner exception: " + (tie.InnerException?.Message ?? tie.Message));
                return 10;
            }
            catch (Exception ex)
            {
                EmitPacket(0, "Runner exception: " + ex.Message);
                return 11;
            }
        }

        private static void LoadUserAssemblies(string? probeDir)
        {
            var baseDir = AppContext.BaseDirectory;
            var currentDir = Directory.GetCurrentDirectory();

            var probeDirs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                baseDir,
                currentDir
            };

            if (!string.IsNullOrWhiteSpace(probeDir) && Directory.Exists(probeDir))
                probeDirs.Add(probeDir);

            var parent = new DirectoryInfo(baseDir);
            for (int i = 0; i < 4 && parent.Parent != null; i++)
            {
                parent = parent.Parent;
                probeDirs.Add(parent.FullName);
            }

            var expanded = new HashSet<string>(probeDirs, StringComparer.OrdinalIgnoreCase);
            foreach (var dir in probeDirs)
            {
                expanded.Add(Path.Combine(dir, "bin", "Debug", "net8.0"));
                expanded.Add(Path.Combine(dir, "bin", "Release", "net8.0"));
            }

            string[] assemblyNames =
            [
                "PETEL_MainTester_V2.dll",
                "PETEL_MainTester_V2.exe",
                "StudentAnswer.dll",
                "TeacherAnswer.dll"
            ];

            foreach (var dir in expanded)
            {
                foreach (var name in assemblyNames)
                    LoadAssemblyIfPresent(Path.Combine(dir, name));
            }

            EnsureTypesLoaded(new[] { "StudentAnswer", "PETEL_VPL.TestCases" }, expanded);
        }

        private static void EnsureTypesLoaded(string[] typeNames, IEnumerable<string> probeDirs)
        {
            if (typeNames.All(IsTypeLoaded))
                return;

            foreach (var dir in probeDirs)
            {
                foreach (var file in EnumerateAssemblyFiles(dir))
                {
                    LoadAssemblyIfPresent(file);
                    if (typeNames.All(IsTypeLoaded))
                        return;
                }
            }
        }

        private static IEnumerable<string> EnumerateAssemblyFiles(string dir)
        {
            if (!Directory.Exists(dir))
                yield break;

            IEnumerable<string> files;
            try
            {
                files = Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly);
            }
            catch
            {
                yield break;
            }

            foreach (var file in files)
            {
                if (file.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
                    file.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    yield return file;
            }
        }

        private static bool IsTypeLoaded(string qualifiedName)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (asm.GetType(qualifiedName, throwOnError: false, ignoreCase: false) != null)
                        return true;
                }
                catch { }
            }
            return false;
        }

        private static void LoadAssemblyIfPresent(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return;

                if (IsAssemblyLoaded(path))
                    return;

                Assembly.LoadFrom(path);
            }
            catch { }
        }

        private static bool IsAssemblyLoaded(string path)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    if (string.Equals(asm.Location, path, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                catch { }
            }
            return false;
        }

        private static string? GetArgValue(string[] args, string key)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (!string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (i + 1 < args.Length)
                    return args[i + 1];

                return null;
            }
            return null;
        }

        private static void EmitPacket(int points, string text)
        {
            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(text ?? string.Empty));
            Console.WriteLine($"{ProtocolPrefix}{points}|{payload}");
        }
    }
}
