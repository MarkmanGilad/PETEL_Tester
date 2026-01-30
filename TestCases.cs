using System;

namespace PETEL_VPL
{
    /// <summary>
    /// Teacher-authored test cases.
    ///
    /// Teachers edit ONLY this file:
    /// 1) Update `CreateTester(...)` configuration (student/teacher file/class/method)
    /// 2) Write tests (methods) that call `tester.TestMethod(...)` and `tester.TestCodeStructure(...)`
    ///
    /// Signature rule for tests: public static void MethodName(VPLTester tester)
    /// </summary>
    public static class TestCases
    {
        // ==========================================================
        // 1) ASSIGNMENT CONFIG (teacher edits this method)
        // ==========================================================
        public static VPLTester CreateTester(bool showDetails = false)
        {
            return new VPLTester(
                // Student submission
                studentFile: "StudentAnswer.cs",
                studentNamespace: "",
                studentClassName: "StudentAnswer",
                studentMethodName: "Main", // <-- change to the required student method (e.g., "Copy")

                // Teacher reference solution
                teacherNamespace: "PETEL_VPL",
                teacherClassName: "TeacherAnswer",
                teacherMethodName: "Main", // <-- change to the required teacher method (e.g., "Copy")

                showDetails: showDetails);
        }

        // ==========================================================
        // 2) TESTS (teacher edits this section)
        // ==========================================================

        // Example placeholder so the projects compile.
        // Replace with real tests.
        public static void Case_Sample(VPLTester tester)
        {
            tester.TestMethod(
                testName: "Sample test (replace me)",
                points: 0,
                parameters: Array.Empty<object>(),
                compareParams: false,
                compareReturn: false
            );
        }

        // Optional: Code_ methods run in Main process (no separate Runner process).
        public static void Code_Sample(VPLTester tester)
        {
            tester.InitializeCodeAnalyzer();

            // Example: replace with your own structure checks
            // tester.TestCodeStructure(...);
        }
    }
}
