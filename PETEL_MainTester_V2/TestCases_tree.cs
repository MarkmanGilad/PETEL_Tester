using System;
using Unit4;
using C = System.Collections.Generic;

namespace PETEL_VPL
{
    public static class TestCases_tree
    {
        
        public static VPLTester CreateTester()
        {
            var tester = new VPLTester(studentMethodName: "DFS_FindMax");
            return tester;
        }
        private static readonly C.Dictionary<Type, string> CommonExceptionComments = new C.Dictionary<Type, string>
        {
            { typeof(NullReferenceException), "You advanced past the end of the list (node became null) " },
        };


        // Test 1: Empty list
        public static void Case_1(VPLTester tester)
        {
            string path = Unit4Helper.GetTreeFilePath("tree.txt");
            BinNode<int> tree = Unit4Helper.BuildBinaryTree<int>(path);

            tester.TestMethod(
                testName: "Test 1: check the correct return",
                points: 10,
                parameters: new object[] { tree },
                compareParams: true,
                exceptionComments: commonExceptionComments
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
        
        // Test 6: All elements match
        public static void Case_3_AllMatch(VPLTester tester)
        {
            int[] arr = { 5, 5, 5, 5 };
            Node<int> list = Unit4Helper.BuildNodeList(arr);

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

        // Check parameter count
        public static void Code_3_CorrectParameters(VPLTester tester)
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
