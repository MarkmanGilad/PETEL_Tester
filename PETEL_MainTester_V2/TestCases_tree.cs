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

        // Test 1: check the correct return value
        public static void Case_1(VPLTester tester)
        {
            var commonExceptionComments = new C.Dictionary<Type, string>
            {
                { typeof(NullReferenceException), "You advanced past the end of the list (node became null) " },
            };

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

        // Test 11: Does the code use recursion
        public static void Code_1_MustUseRecursion(VPLTester tester)
        {
            tester.InitializeCodeAnalyzer();

            tester.TestCodeStructure(
                testName: "Test 11: Does the code recursive",
                points: 5,
                checkType: CodeStructureCheck.IsRecursive,
                failureMessage: "Method must be recursive"
            );
        }

        // Test 12: Method must include two recursive calls
        public static void Code_2_DoubleRecursion(VPLTester tester)
        {
            tester.InitializeCodeAnalyzer();

            tester.TestCodeStructure(
                testName: "Test 12: Method must include two recursive calls",
                points: 5,
                checkType: CodeStructureCheck.CountRecursiveCalls,
                expectedCount: 2,
                failureMessage: "Method did not include two recursive calls"
            );
        }
    }
}
