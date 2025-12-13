using System;
using System.IO;
using Unit4;
using C = System.Collections.Generic;

namespace PETEL_VPL
{
    class MainTester
    {
        public static void Main(string[] args)
        {
            // Initialize the tester
            var tester = new VPLTester(
                studentFile: "StudentAnswer.cs",
                studentNamespace: "",
                studentClassName: "StudentAnswer",
                studentMethodName: "Copy",
                teacherNamespace: "PETEL_VPL",
                teacherClassName: "TeacherAnswer",
                teacherMethodName: "Copy",
                showDetails: true
            );

            // Run all test suites
            CaseTester(tester);
            //CodeTester(tester);

            // Display results (VPL parses this output)
            Console.WriteLine("\n" + tester.FormatResponse());
            Console.WriteLine($"Grade :=>> {tester.GetGrade()}");
        }

        
        private static void CaseTester(VPLTester tester)
        {

            // Optional: shared comments for this suite (can also pass per-call)
            var commonExceptionComments = new C.Dictionary<Type, string>
            {
                { typeof(NullReferenceException), "You tried to remove from empty queue" },
                { typeof(InvalidOperationException), "You invoked an operation (Pop/Peek/Dequeue) on an empty stack/queue. Check Count > 0 before accessing." }

            };

            Queue<int> q1 = Unit4Helper.BuildQueue(new int[] { 3, 5, -9, 3, 5, 5, 2, 1, 2 });
            tester.TestMethod(
                testName: "Test 1: check the correct return. Don't check the original Queue ",
                points: 10,
                parameters: new object[] { q1 },
                compareParams: false,
                exceptionComments: commonExceptionComments
            );


            Queue<int> q2 = Unit4Helper.BuildQueue(new int[] { 3, 5, -9, 3, 5, 5, 2, 1, 2 });
            tester.TestMethod(
                testName: "Test 2: check only the original Queue if it changed",
                points: 10,
                parameters: new object[] { q2 },
                compareParams: true, 
                compareReturn:false
            );

            q2 = Unit4Helper.BuildQueue(new int[] { 3, 5, -9, 3, 5, 5, 2, 1, 2 });
            tester.TestMethod(
                testName: "Test 2: check both return and original",
                points: 10,
                parameters: new object[] { q2 },
                compareParams: true,
                compareReturn: true
            );
        }

        private static void CodeTester(VPLTester tester)
        {

            // Initialize code analyzer (handles errors internally)
            tester.InitializeCodeAnalyzer();

            // NEW: Check student method parameter list matches teacher method
            tester.TestCodeStructure(
                testName: "Test Params: method signature matches teacher",
                points: 5,
                checkType: CodeStructureCheck.CheckParams,
                shouldPass: true,
                failureMessage: "Wrong parameter list. Ensure the method has the same parameters as the teacher (name, count, and types)."
            );

            // NEW: Check return type matches teacher method
            tester.TestCodeStructure(
                testName: "Test Return Type: matches teacher",
                points: 5,
                checkType: CodeStructureCheck.CheckReturnType,
                shouldPass: true,
                failureMessage: "Wrong return type. Ensure the method returns the same type as the teacher."
            );

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

            // Test 13: Check for if statements (input validation and comparison logic)
            tester.TestCodeStructure(
                testName: "Test 13: Uses if statements for logic",
                points: 5,
                checkType: CodeStructureCheck.CountIfStatements,
                failureMessage: "Method should use if statements for conditional logic"
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
