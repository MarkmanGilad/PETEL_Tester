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
                // Run all tests in-process (Code_ and Case_ tests)
                var (pts, txt) = RunInProcessTest(method);
                total += pts;
                outputBlocks.Add(txt);
            }

            foreach (var block in outputBlocks)
                Console.Write(block);

            Console.WriteLine($"Grade :=>> {total}");
            return 0;
        }

        private static (int points, string text) RunInProcessTest(MethodInfo method)
        {
            try
            {
                // Create tester instance (analyzer auto-initializes in constructor)
                var tester = TestCases.CreateTester();

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
    }
}
