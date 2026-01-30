using System;

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
        // 1) ASSIGNMENT CONFIG (teacher edits this method)
        // ==========================================================
        public static VPLTester CreateTester()
        {
            // Simple: Both student and teacher have method named "Main"
            return new VPLTester(studentMethodName: "Main");
            
            // If teacher method has different name:
            // return new VPLTester(
            //     studentMethodName: "Add",
            //     teacherMethodName: "AddSolution"
            // );
            
            // With custom classes/namespaces:
            // return new VPLTester(
            //     studentMethodName: "Calculate",
            //     teacherNamespace: "Solutions",
            //     teacherClassName: "MathSolutions"
            // );
        }

        // ==========================================================
        // 2) FUNCTIONAL TESTS (Case_*
        // ==========================================================

        // Example 1: Simple test with parameters
        public static void Case_BasicTest(VPLTester tester)
        {
            tester.TestMethod(
                testName: "Add two numbers",
                points: 10,
                parameters: new object[] { 5, 3 }
            );
        }

        // Example 2: Test with arrays
        public static void Case_ArrayTest(VPLTester tester)
        {
            int[] arr = { 1, 2, 3, 4, 5 };
            tester.TestMethod(
                testName: "Sum array elements",
                points: 15,
                parameters: new object[] { arr }
            );
        }

        // Example 3: Test with console input
        public static void Case_InputTest(VPLTester tester)
        {
            tester.TestMethod(
                testName: "Read and process input",
                points: 10,
                consoleInput: "5\n3\n",
                captureConsoleOutput: true
            );
        }

        // Example 4: Test parameter modification
        public static void Case_ModifyArray(VPLTester tester)
        {
            int[] arr = { 1, 2, 3 };
            tester.TestMethod(
                testName: "Modify array in place",
                points: 10,
                parameters: new object[] { arr },
                compareParams: true,
                compareReturn: false
            );
        }

        // Example 5: Multiple tests with different method
        public static void Case_MultipleMethods(VPLTester tester)
        {
            // Test Add method
            tester.SetStudentMethod("Add");
            tester.TestMethod(
                testName: "Test Add(5, 3)",
                points: 5,
                parameters: new object[] { 5, 3 }
            );

            // Switch to Multiply method
            tester.SetStudentMethod("Multiply");
            tester.TestMethod(
                testName: "Test Multiply(5, 3)",
                points: 5,
                parameters: new object[] { 5, 3 }
            );
        }

        // Example 6: Test without showing details
        public static void Case_QuietTest(VPLTester tester)
        {
            tester.ShowDetails = false;
            tester.TestMethod(
                testName: "Simple test without details",
                points: 5,
                parameters: new object[] { 10 }
            );
            tester.ShowDetails = true; // Restore default
        }

        // ==========================================================
        // 3) CODE STRUCTURE TESTS (Code_*
        // ==========================================================

        // Example 1: Check for loop usage
        public static void Code_CheckForLoop(VPLTester tester)
        {
            tester.TestCodeStructure(
                testName: "Must use for loop",
                points: 5,
                checkType: CodeStructureCheck.UsesForLoop,
                shouldPass: true,
                failureMessage: "Your solution must use a for loop"
            );
        }

        // Example 2: Prohibit while loop
        public static void Code_NoWhileLoop(VPLTester tester)
        {
            tester.TestCodeStructure(
                testName: "Should not use while loop",
                points: 5,
                checkType: CodeStructureCheck.UsesWhileLoop,
                shouldPass: false,
                failureMessage: "Your solution should not use a while loop"
            );
        }

        // Example 3: Check recursion
        public static void Code_MustUseRecursion(VPLTester tester)
        {
            tester.TestCodeStructure(
                testName: "Must use recursion",
                points: 10,
                checkType: CodeStructureCheck.UsesRecursion,
                shouldPass: true,
                failureMessage: "Your solution must be recursive"
            );
        }

        // Example 4: Check multiple methods
        public static void Code_CheckMultipleMethods(VPLTester tester)
        {
            // Check Main method
            tester.SetStudentMethod("Main");
            tester.TestCodeStructure(
                testName: "Main must use for loop",
                points: 5,
                checkType: CodeStructureCheck.UsesForLoop
            );

            // Check Helper method
            tester.SetStudentMethod("Helper");
            tester.TestCodeStructure(
                testName: "Helper must not use recursion",
                points: 5,
                checkType: CodeStructureCheck.UsesRecursion,
                shouldPass: false
            );
        }

        // Example 5: Check parameter count
        public static void Code_CheckParameterCount(VPLTester tester)
        {
            tester.TestCodeStructure(
                testName: "Method must have exactly 2 parameters",
                points: 5,
                checkType: CodeStructureCheck.ParameterCount,
                expectedCount: 2,
                failureMessage: "Method signature is incorrect"
            );
        }

        // Example 6: Advanced custom checks using method analyzer
        public static void Code_AdvancedCheck(VPLTester tester)
        {
            var analyzer = tester.GetStudentMethodAnalyzer();

            // Check if method calls Console.WriteLine
            if (!analyzer.CallsMethod("Console.WriteLine"))
            {
                tester.AddInfo("Warning: Method doesn't print output");
            }

            // Check if method uses specific variable names
            // Add your custom logic here
        }

        // ==========================================================
        // 4) COMBINED TESTS
        // ==========================================================

        // Example: Test both functionality and structure
        public static void Combined_FullValidation(VPLTester tester)
        {
            // First test functionality
            tester.TestMethod(
                testName: "Functional test: Sum(1,2,3,4,5)",
                points: 10,
                parameters: new object[] { new[] { 1, 2, 3, 4, 5 } }
            );

            // Then check code structure
            tester.TestCodeStructure(
                testName: "Structure: Must use for loop",
                points: 5,
                checkType: CodeStructureCheck.UsesForLoop
            );

            tester.TestCodeStructure(
                testName: "Structure: Should not use LINQ",
                points: 5,
                checkType: CodeStructureCheck.UsesLinq,
                shouldPass: false
            );
        }
    }
}
