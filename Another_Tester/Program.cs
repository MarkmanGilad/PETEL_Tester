using PETEL_VPL;
using System;
using System.Linq;
using System.Reflection;

namespace PETEL_MainTester_V2
{
    internal static class Program
    {
        private const string TestCasesTypeName = "PETEL_VPL.TestCases_CreateList";

        private static int Main(string[] args)
        {
            var testCasesType = Type.GetType(TestCasesTypeName)
                ?? throw new InvalidOperationException($"Test class '{TestCasesTypeName}' not found.");

            var createTesterMethod = testCasesType.GetMethod("CreateTester", BindingFlags.Public | BindingFlags.Static)
                ?? throw new InvalidOperationException($"CreateTester method not found in '{TestCasesTypeName}'.");

            var createTesterDelegate = (Func<VPLTester>)Delegate.CreateDelegate(typeof(Func<VPLTester>), createTesterMethod);

            return MainTesterHost.Run(createTesterDelegate, TestCasesTypeName);
        }
    }
}
