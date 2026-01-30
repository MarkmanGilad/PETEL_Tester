using System;
using System.Linq;
using System.Reflection;

namespace PETEL_Runner_V2
{
    /// <summary>
    /// Runs a single test method in isolation. Called by main runner as child process.
    /// Usage: SingleTestRunner.Run("Case_EmptyList")
    /// </summary>
    internal static class SingleTestRunner
    {
        public static int Run(string testMethodName)
        {
            Console.Error.WriteLine($"[CHILD] SingleTestRunner.Run started for {testMethodName}");
            try
            {
                var testCasesType = AppDomain.CurrentDomain.GetAssemblies()
                    .Select(a => a.GetType("PETEL_VPL.TestCases", throwOnError: false, ignoreCase: false))
                    .FirstOrDefault(t => t != null);

                if (testCasesType == null)
                {
                    Console.WriteLine("Comment :=>>Framework error: TestCases type not found: failure. 0 points\n<|--\nTestCases type not found\n--|>\n");
                    return 1;
                }

                var method = testCasesType.GetMethod(testMethodName, BindingFlags.Public | BindingFlags.Static);
                if (method == null)
                {
                    Console.WriteLine("Comment :=>>Framework error: Test method not found: failure. 0 points\n<|--\nTest method '" + testMethodName + "' not found\n--|>\n");
                    return 2;
                }

                var createTesterMethod = testCasesType.GetMethod("CreateTester", BindingFlags.Public | BindingFlags.Static);
                if (createTesterMethod == null)
                {
                    Console.WriteLine("Comment :=>>Framework error: CreateTester not found: failure. 0 points\n<|--\nCreateTester not found\n--|>\n");
                    return 3;
                }

                Console.Error.WriteLine($"[CHILD] Creating tester and invoking {testMethodName}");
                var tester = createTesterMethod.Invoke(null, null);

                method.Invoke(null, new object[] { tester });

                var getGradeMethod = tester.GetType().GetMethod("GetGrade");
                var formatResponseMethod = tester.GetType().GetMethod("FormatResponse");

                int grade = (int)getGradeMethod.Invoke(tester, null);
                string response = (string)formatResponseMethod.Invoke(tester, null);

                Console.Write(response);
                Console.Error.WriteLine($"[CHILD] Test completed, exiting with grade {grade}");
                return grade;
            }
            catch (TargetInvocationException tie)
            {
                Console.Error.WriteLine($"[CHILD] TargetInvocationException: {tie.InnerException?.GetType().Name} - {tie.InnerException?.Message}");
                Console.WriteLine("Comment :=>>Framework error: Test crashed: failure. 0 points\n<|--\n" + (tie.InnerException?.Message ?? tie.Message) + "\n--|>\n");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[CHILD] Exception: {ex.GetType().Name} - {ex.Message}");
                Console.WriteLine("Comment :=>>Framework error: Unexpected error: failure. 0 points\n<|--\n" + ex.Message + "\n--|>\n");
                return 0;
            }
        }
    }
}
