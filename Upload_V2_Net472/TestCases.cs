using System;
using Unit4;
using C = System.Collections.Generic;

namespace PETEL_VPL
{
    public static class TestCases
    {
        public static VPLTester CreateTester()
        {
            var tester = new VPLTester(studentMethodName: "CountValues");
            return tester;
        }
        

        // Test 1: Empty list
        public static void Case_1(VPLTester tester)
        {
            Node<int> list = null;

            tester.TestMethod(
                testName: "Count in empty list",
                points: 10,
                parameters: new object[] { list, 5 }
            );
            int[] arr = { 5 };
            Node<int> list = Unit4Helper.BuildNodeList(arr);

            tester.TestMethod(
                testName: "Count 5 in list [5]",
                points: 10,
                parameters: new object[] { list, 5 }
            );
        }

        // Test 2: All elements match
        public static void Case_2_AllMatch(VPLTester tester)
        {
            int[] arr = { 5, 5, 5, 5 };
            list = Unit4Helper.BuildNodeList(arr);

            tester.TestMethod(
                testName: "Count 5 in list [5,5,5,5]",
                points: 15,
                parameters: new object[] { list, 5 }
            );
        }

        // Check that solution uses recursion
        public static void Code_1_MustUseRecursion(VPLTester tester)
        {
            tester.TestCodeStructure(
                testName: "Solution must use recursion",
                points: 20,
                checkType: CodeStructureCheck.IsRecursive,  // Changed from UsesRecursion
                shouldPass: true,
                failureMessage: "Your solution must use recursion to traverse the list"
            );
            tester.TestCodeStructure(
                testName: "Solution must not use for loop",
                points: 5,
                checkType: CodeStructureCheck.CountForLoop,  // Changed from UsesForLoop
                expectedCount: 0,  // Added: expect 0 for loops
                shouldPass: true,  // Changed from false - now checking if count equals 0
                failureMessage: "Your solution should use recursion, not a for loop"
            );
                tester.TestCodeStructure(
                testName: "Method must have exactly 2 parameters",
                points: 5,
                checkType: CodeStructureCheck.CheckParams,  // Changed from ParameterCount
                shouldPass: true,
                failureMessage: "CountValues must have exactly 2 parameters (Node<int> head, int value)"
            );
        }

    }
}
