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
            var tester = new VPLTester(studentMethodName: "DelNodeInt");
            return tester;
        }


        // Test 1: Empty list
        public static void Case_1_regularlist(VPLTester tester)
        {
            //Node<int> list1 = Unit4Helper.BuildNodeList(new int[] {1,2,3,4,5,6});
            //Node<int> node = list1.GetNext().GetNext().GetNext();

            //tester.TestMethod(
            //    testName: "test dell 4",
            //    points: 10,
            //    parameters: new object[] { list1, node}
            //);

            NodeInteger list1 = Unit4Helper.BuildNodeIntegerList(new int[] { 1, 2, 3, 4, 5, 6 });
            NodeInteger node = list1.GetNext().GetNext().GetNext();

            tester.TestMethod(
                testName: "test dell 4",
                points: 10,
                parameters: new object[] { list1, node }
            );

            //int[] arr = { 5 };
            //Node<int> list2 = Unit4Helper.BuildNodeList(arr);

            //tester.TestMethod(
            //    testName: "Count 5 in list [5]",
            //    points: 10,
            //    parameters: new object[] { list2, 5 }
            //);
        }

        // Test 2: Sum Array
        //public static void Case_2_SumArray(VPLTester tester)
        //{
        //    int[] arr = { 1, 2, 3, 4, 5, 6 };
        //    Node<int> list = Unit4Helper.BuildNodeList(arr);

        //    tester.TestMethod(
        //        testName: "Sum numbers in Array [1, 2, 3, 4, 5, 6]",
        //        points: 15,
        //        parameters: new object[] { list }       // ����� ���� ����� ���
        //    );
        //}

        public static void Code_1_New(VPLTester tester)
        {
            tester.TestCodeTokenExists(
                testName: "Solution uses new",
                points: 5,
                tokenText: "new",
                failureMessage: "Use the new keyword to create the required node."
            );
        }

        public static void Code_2_UsesNewStack(VPLTester tester)
        {
            tester.TestCodeStructure(
                testName: "Solution creates a Stack",
                points: 5,
                checkType: CodeStructureCheck.CountNewStack,
                failureMessage: "Create at least one new Stack in your solution."
            );
        }

        //public static void Code_1_MustUseRecursion(VPLTester tester)
        //{
        //    tester.TestCodeStructure(
        //        testName: "Solution must use recursion",
        //        points: 20,
        //        checkType: CodeStructureCheck.IsRecursive,  // Changed from UsesRecursion
        //        shouldPass: true,
        //        failureMessage: "Your solution must use recursion to traverse the list"
        //    );
        //    tester.TestCodeStructure(
        //        testName: "Solution must not use for loop",
        //        points: 5,
        //        checkType: CodeStructureCheck.CountForLoop,  // Changed from UsesForLoop
        //        expectedCount: 0,  // Added: expect 0 for loops
        //        shouldPass: true,  // Changed from false - now checking if count equals 0
        //        failureMessage: "Your solution should use recursion, not a for loop"
        //    );
        //    tester.TestCodeStructure(
        //        testName: "Method must have exactly 1 parameter",
        //        points: 5,
        //        checkType: CodeStructureCheck.CheckParams,  // Changed from ParameterCount
        //        shouldPass: true,
        //        failureMessage: "Count Values must have exactly 1 parameter (int[] array)"
        //    );
        //}

    }
}
