using System;

namespace PETEL_VPL
{
    // Minimal stubs to keep V2 core compiling.
    // Replace with real analyzer implementation when ported.

    public enum CodeStructureCheck
    {
        HasNestedLoops
    }

    public sealed class CodeStructureResult
    {
        public bool Passed { get; init; }
        public int Count { get; init; }
        public string? Description { get; init; }
    }

    public sealed class MethodAnalyzer
    {
    }

    public sealed class CodeAnalyzer
    {
        private readonly string sourcePath;

        public CodeAnalyzer(string studentSourceFilePath)
        {
            sourcePath = studentSourceFilePath;
        }

        public CodeStructureResult CheckMethodStructure(string methodName, CodeStructureCheck checkType, int? expectedCount)
        {
            // Stub: Always pass.
            return new CodeStructureResult { Passed = true, Count = 0, Description = "Analyzer not implemented in V2 core yet" };
        }

        public MethodAnalyzer GetMethodAnalyzer(string methodName) => new();
    }
}
