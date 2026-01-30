using System;
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

                // Single source of truth: teachers configure the assignment inside TestCases.cs
                var tester = TestCases.CreateTester(showDetails: false);

                // Suppress any stdout from student code or teacher test.
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
