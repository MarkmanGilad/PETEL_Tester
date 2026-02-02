using System;
using System.IO;
using PETEL_VPL;

namespace Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            PrintRecursiveCallCount("DFS_FindMax");
        }

        private static void PrintRecursiveCallCount(string methodName)
        {
            string solutionRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", ".."));
            string studentFile = Path.Combine(solutionRoot, "PETEL_MainTester_V2", "StudentAnswer.cs");

            Console.WriteLine($"Reading file: {studentFile}");
            Console.WriteLine($"File exists: {File.Exists(studentFile)}");
            Console.WriteLine("\n--- FILE CONTENT ---");
            Console.WriteLine(File.ReadAllText(studentFile));
            Console.WriteLine("--- END FILE ---\n");

            var analyzer = new CodeAnalyzer(studentFile);
            int count = analyzer.GetMethodAnalyzer(methodName).CountRecursiveCalls();

            Console.WriteLine($"Recursive call count for {methodName}: {count}");
        }
    }
}
