using System;
using Unit4;

namespace PETEL_VPL
{
    /// <summary>
    /// Teacher-authored test cases.
    ///
    /// Teachers edit ONLY this file:
    /// 1) Update `CreateTester()` configuration (student/teacher method name)
    /// 2) Write tests (methods) that call `tester.TestMethod(...)` and `tester.TestCodeStructure(...)`
    ///
    /// Signature rule for tests: public static void MethodName(VPLTester tester)
    /// </summary>
    public static class TestCases
    {
        // ==========================================================
        // 1) ASSIGNMENT CONFIG
        // ==========================================================
        public static VPLTester CreateTester()
        {
            return new VPLTester(studentMethodName: "CountValues");
        }

        // ==========================================================
        // 2) FUNCTIONAL TESTS
        // ==========================================================

        // Test 1: Empty list
        public static void Case_EmptyList(VPLTester tester)
        {
            Node<int>? list = null;
            
            tester.TestMethod(
                testName: "Count in empty list",
                points: 10,
                parameters: new object[] { list, 5 }
            );
        }

        // Test 2: Single element - found
        public static void Case_SingleElementFound(VPLTester tester)
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
        public static void Case_SingleElementNotFound(VPLTester tester)
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
        public static void Case_MultipleMatches(VPLTester tester)
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
        public static void Case_NoMatches(VPLTester tester)
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
        public static void Case_AllMatch(VPLTester tester)
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
        public static void Case_LargeList(VPLTester tester)
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
        public static void Case_NegativeNumbers(VPLTester tester)
        {
            int[] arr = { -5, -3, -5, 0, 5, -5 };
            Node<int> list = Unit4Helper.BuildNodeList(arr);
            
            tester.TestMethod(
                testName: "Count -5 in list with negatives",
                points: 10,
                parameters: new object[] { list, -5 }
            );
        }

        // ==========================================================
        // 3) CODE STRUCTURE TESTS
        // ==========================================================

        // Check that solution uses recursion
        public static void Code_MustUseRecursion(VPLTester tester)
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
        public static void Code_NoForLoop(VPLTester tester)
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

        public static void Code_NoWhileLoop(VPLTester tester)
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
        public static void Code_CorrectParameters(VPLTester tester)
        {
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
