using PETEL_VPL;

namespace PETEL_MainTester_V2
{
    internal static class Program
    {
        private static int Main(string[] args) => MainTesterHost.Run(TestCases.CreateTester);
    }
}
