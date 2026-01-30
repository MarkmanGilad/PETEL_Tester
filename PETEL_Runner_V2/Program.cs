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
        private static int Main(string[] args)
        {
            try
            {
                var testCasesType = Type.GetType("PETEL_VPL.TestCases")
                    ?? AppDomain.CurrentDomain.GetAssemblies()
                        .Select(a => a.GetType("PETEL_VPL.TestCases", throwOnError: false, ignoreCase: false))
                        .FirstOrDefault(t => t != null);

                if (testCasesType == null)
                {
                    Console.WriteLine("Comment :=>>Framework error: type PETEL_VPL.TestCases not found: failure. 0 points\n");
                    Console.WriteLine("Grade :=>> 0");
                    return 3;
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

                if (testMethods.Count == 0)
                {
                    Console.WriteLine("Comment :=>>Framework error: no test methods found in TestCases: failure. 0 points\n");
                    Console.WriteLine("Grade :=>> 0");
                    return 5;
                }

                int total = 0;
                var outputBlocks = new System.Collections.Generic.List<string>();

                foreach (var method in testMethods)
                {
                    var (pts, txt) = RunTestMethod(method);
                    total += pts;
                    outputBlocks.Add(txt);
                }

                foreach (var block in outputBlocks)
                    Console.Write(block);

                Console.WriteLine($"Grade :=>> {total}");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Comment :=>>Runner exception: failure. 0 points\n<|--\n" + ex.Message + "\n--|>\n");
                Console.WriteLine("Grade :=>> 0");
                return 11;
            }
        }

        private static (int points, string text) RunTestMethod(MethodInfo method)
        {
            try
            {
                var tester = TestCases.CreateTester();

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
