using System;
using Unit4;
using C = System.Collections.Generic;

namespace PETEL_VPL
{
    public static class TestCases_RangeList
    {
        public static VPLTester CreateTester()
        {
            var tester = new VPLTester(studentMethodName: "CreateRangeList");
            return tester;
        }
        

        // Test 1: Empty list
        public static void Case_2_EmptyList(VPLTester tester)
        {
            // Optional: shared comments for this suite (can also pass per-call)
            var commonExceptionComments = new C.Dictionary<Type, string>
            {
                { typeof(NullReferenceException), "You advanced past the end of the list (node became null) " },
            };
            Node<int> lst = Unit4Helper.BuildNodeList(new int[] { 3, 4, 5, 12, 19, 20, 100, 101, 102, 103, 104 });

            tester.TestMethod(
                testName: "Test 1: Build the range list from the question example",
                points: 10,
                parameters: new object[] { lst },
                compareParams: false,
                exceptionComments: commonExceptionComments
            );
        }

        

    }
}
