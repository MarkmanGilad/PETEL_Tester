using System;
using Unit4;
using C = System.Collections.Generic;

namespace PETEL_VPL
{
    public static class TestPrintList
    {
        public static VPLTester CreateTester()
        {
            var tester = new VPLTester(studentMethodName: "PrintList");
            return tester;
        }
        

        // Test 1: Empty list
        public static void Case_2_EmptyList(VPLTester tester)
        {
            var lst1 = Unit4Helper.BuildNodeList<int>(new int[] { 3, 5, -4, 22, 0, 7 });
            tester.TestMethod(
                testName: "Test 1: check regular list",
                points: 8,
                parameters: new object[] { lst1 },
                captureConsoleOutput: true

            );
            var lst2 = Unit4Helper.BuildNodeList<int>(new int[] { });
            tester.TestMethod(
                testName: "Test 2: check Empty list",
                points: 2,
                parameters: new object[] { lst2 },
                captureConsoleOutput: true
            );
        }

    }
}
