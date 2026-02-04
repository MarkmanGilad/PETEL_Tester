using System;
using Unit4;
using C = System.Collections.Generic;

namespace PETEL_VPL
{
    public static class TestCases
    {
        // Teacher-settable runtime messages
        public static string TimeoutComment =
            "Runtime error: timeout (possible infinite loop). If you used recursion, verify the stop condition and that each call moves toward it (e.g., head = head.GetNext()).";

        public static string StackOverflowComment =
            "Runtime error: stack overflow. Your recursion did not stop (stop condition failed) or the recursive call did not advance toward the stop condition (e.g., you called CountValues(head, ...) instead of CountValues(head.GetNext(), ...)).";


        public static VPLTester CreateTester()
        {
            var tester = new VPLTester(studentMethodName: "CountValues");
            return tester;
        }
        

        // Test 1: Empty list
        public static void Case_2_EmptyList(VPLTester tester)
        {
            Node<int> list1 = null;

            tester.TestMethod(
                testName: "Count in empty list",
                points: 10,
                parameters: new object[] { list1, 5 }
            );

            int[] arr = { 5 };
            Node<int> list2 = Unit4Helper.BuildNodeList(arr);

            tester.TestMethod(
                testName: "Count 5 in list [5]",
                points: 10,
                parameters: new object[] { list2, 5 }
            );
        }

        // Test 2: All elements match
        public static void Case_2_AllMatch(VPLTester tester)
        {
            int[] arr = { 5, 5, 5, 5 };
            Node<int> list = Unit4Helper.BuildNodeList(arr);

            tester.TestMethod(
                testName: "Count 5 in list [5,5,5,5]",
                points: 15,
                parameters: new object[] { list, 5 }
            );
        }

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
