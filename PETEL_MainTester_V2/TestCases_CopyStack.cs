using System;
using Unit4;
using C = System.Collections.Generic;

namespace PETEL_VPL
{
    public static class TestCases_CopyStack
    {
        private static readonly C.Dictionary<Type, string> CommonExceptionComments = new C.Dictionary<Type, string>
        {
            { typeof(NullReferenceException), "You invoked an operation (Pop/Peek/Dequeue) on an empty stack/queue" },
        };

        public static VPLTester CreateTester()
        {
            var tester = new VPLTester(studentMethodName: "CopyStack");
            return tester;
        }

        // Test 1: check the correct return with populated stack
        public static void Case_1(VPLTester tester)
        {
            var s1 = Unit4Helper.BuildStack(new int[] { 3, 5, -9, 3, 5, 5, 2, 1, 2 });

            tester.TestMethod(
                testName: "Test 1: check the correct return",
                points: 10,
                parameters: new object[] { s1 },
                compareParams: true,
                exceptionComments: CommonExceptionComments
            );
        }

        // Test 2: Empty stack
        public static void Case_2(VPLTester tester)
        {
            var s2 = Unit4Helper.BuildStack(new int[] { });

            tester.TestMethod(
                testName: "Test 2: Empty array",
                points: 10,
                parameters: new object[] { s2 },
                compareParams: true,
                exceptionComments: CommonExceptionComments
            );
        }
    }
}
