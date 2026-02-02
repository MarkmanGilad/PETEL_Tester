using System;
using Unit4;
using C = System.Collections.Generic;

namespace PETEL_VPL
{
    public static class TestCases
    {
        // Teacher-settable runtime messages
        public static string TimeoutComment = "Runtime error: timeout (possible infinite loop or stack overflow).";
        public static string StackOverflowComment = "Runtime error: stack overflow.";

        // Shared exception comments for managed exceptions
        private static readonly C.Dictionary<Type, string> CommonExceptionComments = new C.Dictionary<Type, string>
        {
            { typeof(NullReferenceException), "You advanced past the end of the list (node became null) " },
            { typeof(InvalidOperationException), "You invoked an operation (Pop/Peek/Dequeue) on an empty stack/queue. Check Count > 0 before accessing." }
        };

        public static VPLTester CreateTester()
        {
            var tester = new VPLTester(studentMethodName: "CountValues");
            return tester;
        }
        

        // Test 1: Empty list
        public static void Case_2_EmptyList(VPLTester tester)
        {
            Node<int> list = null;

            tester.TestMethod(
                testName: "Count in empty list",
                points: 10,
                parameters: new object[] { list, 5 },
                exceptionComments: CommonExceptionComments
            );
        }

        // Test 2: Single element - found
        public static void Case_1_SingleElementFound(VPLTester tester)
        {
            int[] arr = { 5 };
            Node<int> list = Unit4Helper.BuildNodeList(arr);

            tester.TestMethod(
                testName: "Count 5 in list [5]",
                points: 10,
                parameters: new object[] { list, 5 }
            );
        }

        // Test 3: Single element - not found
        public static void Case_3_SingleElementNotFound(VPLTester tester)
        {
            int[] arr = { 3 };
            Node<int> list = Unit4Helper.BuildNodeList(arr);

            tester.TestMethod(
                testName: "Count 5 in list [3]",
                points: 10,
                parameters: new object[] { list, 5 }
            );
        }

        // Test 4: Multiple elements - some matches
        public static void Case_4_MultipleMatches(VPLTester tester)
        {
            int[] arr = { 1, 5, 3, 5, 7, 5 };
            Node<int> list = Unit4Helper.BuildNodeList(arr);

            tester.TestMethod(
                testName: "Count 5 in list [1,5,3,5,7,5]",
                points: 15,
                parameters: new object[] { list, 5 }
            );
        }

        // Test 5: No matches
        public static void Case_5_NoMatches(VPLTester tester)
        {
            int[] arr = { 1, 2, 3, 4, 6, 7, 8 };
            Node<int> list = Unit4Helper.BuildNodeList(arr);

            tester.TestMethod(
                testName: "Count 5 in list [1,2,3,4,6,7,8]",
                points: 10,
                parameters: new object[] { list, 5 }
            );
        }

        // Test 6: All elements match
        public static void Case_6_AllMatch(VPLTester tester)
        {
            int[] arr = { 5, 5, 5, 5 };
            Node<int> list = Unit4Helper.BuildNodeList(arr);

            tester.TestMethod(
                testName: "Count 5 in list [5,5,5,5]",
                points: 15,
                parameters: new object[] { list, 5 }
            );
        }

        // Test 7: Large list
        public static void Case_7_LargeList(VPLTester tester)
        {
            int[] arr = { 1, 5, 2, 5, 3, 5, 4, 5, 6, 7, 8, 9, 10 };
            Node<int> list = Unit4Helper.BuildNodeList(arr);
            
            tester.TestMethod(
                testName: "Count 5 in large list",
                points: 10,
                parameters: new object[] { list, 5 }
            );
        }

        // Test 8: Negative numbers
        public static void Case_8_NegativeNumbers(VPLTester tester)
        {
            int[] arr = { -5, -3, -5, 0, 5, -5 };
            Node<int> list = Unit4Helper.BuildNodeList(arr);

            tester.TestMethod(
                testName: "Count -5 in list with negatives",
                points: 10,
                parameters: new object[] { list, -5 }
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
        }

        // Check that solution does NOT use loops
        public static void Code_2_NoForLoop(VPLTester tester)
        {
            tester.TestCodeStructure(
                testName: "Solution must not use for loop",
                points: 5,
                checkType: CodeStructureCheck.CountForLoop,  // Changed from UsesForLoop
                expectedCount: 0,  // Added: expect 0 for loops
                shouldPass: true,  // Changed from false - now checking if count equals 0
                failureMessage: "Your solution should use recursion, not a for loop"
            );
        }

        public static void Code_3_NoWhileLoop(VPLTester tester)
        {
            tester.TestCodeStructure(
                testName: "Solution must not use while loop",
                points: 5,
                checkType: CodeStructureCheck.CountWhileLoop,  // Changed from UsesWhileLoop
                expectedCount: 0,  // Added: expect 0 while loops
                shouldPass: true,  // Changed from false - now checking if count equals 0
                failureMessage: "Your solution should use recursion, not a while loop"
            );
        }

        // Check parameter count
        public static void Code_4_CorrectParameters(VPLTester tester)
        {
            tester.TestCodeStructure(
                testName: "Method must have exactly 2 parameters",
                points: 5,
                checkType: CodeStructureCheck.CheckParams,  // Changed from ParameterCount
                shouldPass: true,
                failureMessage: "CountValues must have exactly 2 parameters (Node<int> head, int value)"
            );
        }

        // test another method
        public static void Case_2_SecondFunction(VPLTester tester)
        {
            var originalStudentMethod = tester.StudentMethodName;
            tester.StudentMethodName = "CountValues2";
            int[] arr = { 1, 5, 2, 5, 3, 5, 4, 5, 6, 7, 8, 9, 10 };
            Node<int> list = Unit4Helper.BuildNodeList(arr);

            tester.TestMethod(
                testName: "OtherFunction test",
                points: 10,
                parameters: new object[] { list, 5 }
            );

            tester.StudentMethodName = originalStudentMethod;
        }

    }
}
