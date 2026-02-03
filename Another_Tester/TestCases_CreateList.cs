using System;
using Unit4;
using C = System.Collections.Generic;

namespace PETEL_VPL
{
    public static class TestCases_CreateList
    {
        public static VPLTester CreateTester()
        {
            var tester = new VPLTester(studentMethodName: "CreateList");
            return tester;
        }
        
        public static void Case_1(VPLTester tester)
        {
            // Optional: shared comments for this suite (can also pass per-call)
            tester.TestMethod(
                testName: "Test 1: check building a list",
                points: 6,
                parameters: new object[] { new int[] { 3, 5, -9, 3, 5, 5, 2, 1, 2 } },
                compareParams: false

            );
        }

        public static void Case_2(VPLTester tester)
        {
            tester.TestMethod(
                testName: "Test 2: check list with one element array",
                points: 2,
                parameters: new object[] { new int[] { 0 } }
            );
        }

        public static void Case_3(VPLTester tester)
        {
            tester.TestMethod(
                testName: "Test 3: check list from empty array",
                points: 2,
                parameters: new object[] { new int[] { } },
                compareParams: true
            );
        }

        public static void CodeTester(VPLTester tester)
        {


            // Test 11: Check that the method uses exactly one loop (O(n) complexity)
            tester.TestCodeStructure(
                testName: "Test 11: Uses exactly one loop",
                points: 10,
                checkType: CodeStructureCheck.CountAnyLoop,
                expectedCount: 1,
                failureMessage: "Method must use exactly one loop for O(n) time complexity"
            );

            // Test 12: Verify no nested loops (ensures O(n) not O(n²))
            tester.TestCodeStructure(
                testName: "Test 12: No nested loops (O(n) complexity)",
                points: 10,
                checkType: CodeStructureCheck.HasNestedLoops,
                shouldPass: false,  // We want HasNestedLoops to return FALSE
                failureMessage: "Method must not have nested loops to maintain O(n) complexity"
            );

            // Test 14: Verify method is not recursive (should be iterative)
            tester.TestCodeStructure(
                testName: "Test 14: Not recursive (iterative solution)",
                points: 5,
                checkType: CodeStructureCheck.IsRecursive,
                shouldPass: false,  // We want IsRecursive to return FALSE
                failureMessage: "Method should not be recursive for this problem"
            );

            // Test 15: Check return statements
            tester.TestCodeStructure(
                testName: "Test 15: Has return statements",
                points: 5,
                checkType: CodeStructureCheck.CountReturnStatements,
                failureMessage: "Method must have return statements"
            );

            // Test 16: NEW - Check if method is public
            tester.TestCodeStructure(
                testName: "Test 16: Method is public",
                points: 5,
                checkType: CodeStructureCheck.IsPublic,
                shouldPass: true,
                failureMessage: "Method must be public"
            );

            // Test 17: NEW - Check if method is static
            tester.TestCodeStructure(
                testName: "Test 17: Method is static",
                points: 5,
                checkType: CodeStructureCheck.IsStatic,
                shouldPass: true,
                failureMessage: "Method must be static"
            );

            // Test 18: NEW - Verify method is NOT private
            tester.TestCodeStructure(
                testName: "Test 18: Method is not private",
                points: 5,
                checkType: CodeStructureCheck.IsPrivate,
                shouldPass: false,  // We want IsPrivate to return FALSE
                failureMessage: "Method should not be private"
            );
        }

    }
}
