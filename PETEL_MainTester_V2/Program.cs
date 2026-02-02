using PETEL_VPL;

namespace PETEL_MainTester_V2
{
    internal static class Program
    {
        private const string TestCasesTypeName = "PETEL_VPL.TestCases";

        private static int Main(string[] args) => MainTesterHost.Run(TestCases.CreateTester, TestCasesTypeName);
    }
}
