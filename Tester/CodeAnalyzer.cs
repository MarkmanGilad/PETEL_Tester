using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Reflection;

namespace PETEL_VPL
{
    /// <summary>
    /// Code structure check types
    /// </summary>
    public enum CodeStructureCheck
    {
        IsRecursive,
        CountForLoop,
        CountWhileLoop,
        CountForEachLoop,
        CountAnyLoop,
        CountIfStatements,
        CountRecursiveCalls,
        CountReturnStatements,
        CountNewNodes,
        CountNewQueue,
        CountNewStack,
        CountNewBinNode,
        CountSetNext,
        CountGetNext,
        HasNestedLoops,
        IsStatic,
        IsPublic,
        IsPrivate,
        IsProtected,
        IsInternal,
        CheckParams,
        CheckReturnType
    }

    /// <summary>
    /// Result of a code structure check
    /// </summary>
    public class CodeCheckResult
    {
        public bool Passed { get; set; }
        public int Count { get; set; }
        public string Description { get; set; }

        public CodeCheckResult(bool passed, int count = 0, string description = "")
        {
            Passed = passed;
            Count = count;
            Description = description;
        }
    }

    /// <summary>
    /// Analyzes C# code structure and patterns using Roslyn syntax analysis
    /// </summary>
    public class CodeAnalyzer
    {
        private readonly string sourceCode;
        private readonly SyntaxTree syntaxTree;
        private readonly CompilationUnitSyntax root;

        /// <summary>
        /// Initialize analyzer from source code file
        /// </summary>
        public CodeAnalyzer(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Source file not found: {filePath}");

            sourceCode = File.ReadAllText(filePath);
            syntaxTree = CSharpSyntaxTree.ParseText(sourceCode);
            root = syntaxTree.GetRoot() as CompilationUnitSyntax;
        }

        /// <summary>
        /// Initialize analyzer from source code string
        /// </summary>
        public static CodeAnalyzer FromCode(string code)
        {
            var analyzer = new CodeAnalyzer();
            analyzer.InitializeFromCode(code);
            return analyzer;
        }

        private CodeAnalyzer() { }

        private void InitializeFromCode(string code)
        {
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, code);
            try
            {
                var analyzer = new CodeAnalyzer(tempFile);
                File.Delete(tempFile);
            }
            catch
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
                throw;
            }
        }

        /// <summary>
        /// Get analyzer for a specific method
        /// </summary>
        public MethodAnalyzer GetMethodAnalyzer(string methodName)
        {
            var method = root.DescendantNodes()
                            .OfType<MethodDeclarationSyntax>()
                            .FirstOrDefault(m => m.Identifier.Text == methodName);

            if (method == null)
                throw new ArgumentException($"Method '{methodName}' not found in source code");

            return new MethodAnalyzer(method);
        }

        /// <summary>
        /// Perform a code structure check on a method
        /// </summary>
        public CodeCheckResult CheckMethodStructure(string methodName, CodeStructureCheck checkType, int? expectedCount = null)
        {
            var methodAnalyzer = GetMethodAnalyzer(methodName);
            return PerformCheck(methodAnalyzer, checkType, expectedCount);
        }

        /// <summary>
        /// Route the enum check to the appropriate method analyzer function
        /// </summary>
        private CodeCheckResult PerformCheck(MethodAnalyzer analyzer, CodeStructureCheck checkType, int? expectedCount = null)
        {
            switch (checkType)
            {
                case CodeStructureCheck.IsRecursive:
                    // Special case: recursion is not a count, it's a boolean check
                    bool isRecursive = analyzer.IsRecursive();
                    return new CodeCheckResult(isRecursive, isRecursive ? 1 : 0, "Is recursive");

                case CodeStructureCheck.IsStatic:
                    bool isStatic = analyzer.IsStatic();
                    return new CodeCheckResult(isStatic, isStatic ? 1 : 0, "Is static");

                case CodeStructureCheck.IsPublic:
                    bool isPublic = analyzer.IsPublic();
                    return new CodeCheckResult(isPublic, isPublic ? 1 : 0, "Is public");

                case CodeStructureCheck.IsPrivate:
                    bool isPrivate = analyzer.IsPrivate();
                    return new CodeCheckResult(isPrivate, isPrivate ? 1 : 0, "Is private");

                case CodeStructureCheck.IsProtected:
                    bool isProtected = analyzer.IsProtected();
                    return new CodeCheckResult(isProtected, isProtected ? 1 : 0, "Is protected");

                case CodeStructureCheck.IsInternal:
                    bool isInternal = analyzer.IsInternal();
                    return new CodeCheckResult(isInternal, isInternal ? 1 : 0, "Is internal");

                case CodeStructureCheck.CountForLoop:
                    int forCount = analyzer.CountForLoops();
                    return new CodeCheckResult(!expectedCount.HasValue || forCount == expectedCount.Value, forCount, $"For loop count: {forCount}");

                case CodeStructureCheck.CountWhileLoop:
                    int whileCount = analyzer.CountWhileLoops();
                    return new CodeCheckResult(!expectedCount.HasValue || whileCount == expectedCount.Value, whileCount, $"While loop count: {whileCount}");

                case CodeStructureCheck.CountForEachLoop:
                    int foreachCount = analyzer.CountForEachLoops();
                    return new CodeCheckResult(!expectedCount.HasValue || foreachCount == expectedCount.Value, foreachCount, $"Foreach loop count: {foreachCount}");

                case CodeStructureCheck.CountAnyLoop:
                    int anyLoopCount = analyzer.CountForLoops() + analyzer.CountWhileLoops() + analyzer.CountForEachLoops();
                    return new CodeCheckResult(!expectedCount.HasValue || anyLoopCount == expectedCount.Value, anyLoopCount, $"Total loop count: {anyLoopCount}");

                case CodeStructureCheck.CountIfStatements:
                    int ifCount = analyzer.CountIfStatements();
                    return new CodeCheckResult(!expectedCount.HasValue || ifCount == expectedCount.Value, ifCount, $"If statement count: {ifCount}");

                case CodeStructureCheck.CountReturnStatements:
                    int returnCount = analyzer.CountReturnStatements();
                    return new CodeCheckResult(!expectedCount.HasValue || returnCount == expectedCount.Value, returnCount, $"Return statement count: {returnCount}");

                case CodeStructureCheck.CountNewNodes:
                    int nodeCount = analyzer.CountNewNodes();
                    return new CodeCheckResult(!expectedCount.HasValue || nodeCount == expectedCount.Value, nodeCount, $"New Node count: {nodeCount}");

                case CodeStructureCheck.CountSetNext:
                    int setNextCount = analyzer.CountSetNext();
                    return new CodeCheckResult(!expectedCount.HasValue || setNextCount == expectedCount.Value, setNextCount, $"SetNext call count: {setNextCount}");

                case CodeStructureCheck.CountGetNext:
                    int getNextCount = analyzer.CountGetNext();
                    return new CodeCheckResult(!expectedCount.HasValue || getNextCount == expectedCount.Value, getNextCount, $"GetNext call count: {getNextCount}");

                case CodeStructureCheck.HasNestedLoops:
                    bool hasNested = analyzer.HasNestedLoops();
                    return new CodeCheckResult(hasNested, hasNested ? 1 : 0, "Has nested loops");

                // NEW CHECKS
                case CodeStructureCheck.CountNewQueue:
                    int queueCount = analyzer.CountNewQueue();
                    return new CodeCheckResult(!expectedCount.HasValue || queueCount == expectedCount.Value, queueCount, $"New Queue count: {queueCount}");

                case CodeStructureCheck.CountNewStack:
                    int stackCount = analyzer.CountNewStack();
                    return new CodeCheckResult(!expectedCount.HasValue || stackCount == expectedCount.Value, stackCount, $"New Stack count: {stackCount}");

                case CodeStructureCheck.CountNewBinNode:
                    int binNodeCount = analyzer.CountNewBinNode();
                    return new CodeCheckResult(!expectedCount.HasValue || binNodeCount == expectedCount.Value, binNodeCount, $"New BinNode count: {binNodeCount}");

                // NEW: count of recursive self-invocations in the method body
                case CodeStructureCheck.CountRecursiveCalls:
                    int recCount = analyzer.CountRecursiveCalls();
                    return new CodeCheckResult(!expectedCount.HasValue || recCount == expectedCount.Value, recCount, $"Recursive call count: {recCount}");

                case CodeStructureCheck.CheckParams:
                    // Compare student method param types with teacher method (PETEL_VPL.TeacherAnswer)
                    string[] studentParamTypes = analyzer.GetParameterTypes();

                    var teacherType = Type.GetType("PETEL_VPL.TeacherAnswer");
                    if (teacherType == null)
                        return new CodeCheckResult(false, 0, "Teacher type 'PETEL_VPL.TeacherAnswer' not found.");

                    var teacherMethod = teacherType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                                                   .FirstOrDefault(m => m.Name == analyzer.GetMethodName());
                    if (teacherMethod == null)
                        return new CodeCheckResult(false, 0, "Teacher method not found on TeacherAnswer.");

                    var teacherParamTypes = teacherMethod.GetParameters().Select(p => FormatTypeName(p.ParameterType)).ToArray();

                    bool countMatches = studentParamTypes.Length == teacherParamTypes.Length;
                    bool typesMatch = countMatches && studentParamTypes.SequenceEqual(teacherParamTypes);

                    string expectedSig = $"({string.Join(", ", teacherParamTypes)})";
                    string actualSig = $"({string.Join(", ", studentParamTypes)})";

                    return new CodeCheckResult(typesMatch, studentParamTypes.Length,
                        typesMatch ? $"Parameter list matches {actualSig}" : $"Wrong parameter list. Expected: {expectedSig}. Actual: {actualSig}.");

                case CodeStructureCheck.CheckReturnType:
                    // Compare student method return type with teacher method return type
                    string studentReturn = analyzer.GetReturnType();

                    var tType = Type.GetType("PETEL_VPL.TeacherAnswer");
                    if (tType == null)
                        return new CodeCheckResult(false, 0, "Teacher type 'PETEL_VPL.TeacherAnswer' not found.");

                    var tMethod = tType.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                                       .FirstOrDefault(m => m.Name == analyzer.GetMethodName());
                    if (tMethod == null)
                        return new CodeCheckResult(false, 0, "Teacher method not found on TeacherAnswer.");

                    string teacherReturn = FormatTypeName(tMethod.ReturnType);

                    bool returnMatches = string.Equals(studentReturn, teacherReturn, StringComparison.Ordinal);
                    string expectedRet = teacherReturn;
                    string actualRet = studentReturn;

                    return new CodeCheckResult(returnMatches, returnMatches ? 1 : 0,
                        returnMatches ? $"Return type matches: {actualRet}" : $"Wrong return type. Expected: {expectedRet}. Actual: {actualRet}.");

                default:
                    throw new ArgumentException($"Unknown check type: {checkType}");
            }
        }

        private static string FormatTypeName(Type t)
        {
            if (!t.IsGenericType)
                return t.Name == "Int32" ? "int" : t.Name;

            var genericTypeName = t.Name.Substring(0, t.Name.IndexOf('`'));
            var args = t.GetGenericArguments().Select(FormatTypeName);
            // FIX: remove stray space before '>' to avoid mismatches
            return $"{genericTypeName}<{string.Join(", ", args)}>";
        }

        /// <summary>
        /// Check if a class exists in the code
        /// </summary>
        public bool HasClass(string className)
        {
            return root.DescendantNodes()
                      .OfType<ClassDeclarationSyntax>()
                      .Any(c => c.Identifier.Text == className);
        }

        /// <summary>
        /// Get all method names in a class
        /// </summary>
        public string[] GetMethodNames(string className)
        {
            var classDecl = root.DescendantNodes()
                               .OfType<ClassDeclarationSyntax>()
                               .FirstOrDefault(c => c.Identifier.Text == className);

            if (classDecl == null)
                return new string[0];

            return classDecl.DescendantNodes()
                           .OfType<MethodDeclarationSyntax>()
                           .Select(m => m.Identifier.Text)
                           .ToArray();
        }
    }

    /// <summary>
    /// Analyzes specific method syntax and patterns
    /// </summary>
    public class MethodAnalyzer
    {
        private readonly MethodDeclarationSyntax method;

        internal MethodAnalyzer(MethodDeclarationSyntax methodSyntax)
        {
            method = methodSyntax ?? throw new ArgumentNullException(nameof(methodSyntax));
        }

        /// <summary>
        /// Expose method name for external comparison
        /// </summary>
        public string GetMethodName() => method.Identifier.Text;

        /// <summary>
        /// Check if method is recursive (calls itself)
        /// </summary>
        public bool IsRecursive()
        {
            return method.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Any(inv => GetInvocationName(inv) == method.Identifier.Text);
        }

        /// <summary>
        /// Count how many direct recursive self-calls exist in the method body.
        /// Handles IdentifierName (Foo(...)) and MemberAccess (this.Foo(...), ClassName.Foo(...)).
        /// </summary>
        public int CountRecursiveCalls()
        {
            string methodName = method.Identifier.Text;

            return method.DescendantNodes()
                         .OfType<InvocationExpressionSyntax>()
                         .Count(inv => GetInvocationName(inv) == methodName);
        }

        private static string GetInvocationName(InvocationExpressionSyntax inv)
        {
            if (inv.Expression is IdentifierNameSyntax id)
                return id.Identifier.Text;

            if (inv.Expression is MemberAccessExpressionSyntax ma)
                return ma.Name.Identifier.Text;

            return null;
        }

        /// <summary>
        /// Check if method is static
        /// </summary>
        public bool IsStatic()
        {
            return method.Modifiers.Any(SyntaxKind.StaticKeyword);
        }

        /// <summary>
        /// Check if method is public
        /// </summary>
        public bool IsPublic()
        {
            return method.Modifiers.Any(SyntaxKind.PublicKeyword);
        }

        /// <summary>
        /// Check if method is private
        /// </summary>
        public bool IsPrivate()
        {
            bool hasPrivateModifier = method.Modifiers.Any(SyntaxKind.PrivateKeyword);
            bool hasNoAccessModifier = !method.Modifiers.Any(m =>
                m.IsKind(SyntaxKind.PublicKeyword) ||
                m.IsKind(SyntaxKind.ProtectedKeyword) ||
                m.IsKind(SyntaxKind.InternalKeyword) ||
                m.IsKind(SyntaxKind.PrivateKeyword));

            return hasPrivateModifier || hasNoAccessModifier;
        }

        /// <summary>
        /// Check if method is protected
        /// </summary>
        public bool IsProtected()
        {
            return method.Modifiers.Any(SyntaxKind.ProtectedKeyword);
        }

        /// <summary>
        /// Check if method is internal
        /// </summary>
        public bool IsInternal()
        {
            return method.Modifiers.Any(SyntaxKind.InternalKeyword);
        }

        /// <summary>
        /// Check if method contains nested loops (any loop inside another loop)
        /// </summary>
        public bool HasNestedLoops()
        {
            var allLoops = method.DescendantNodes()
                                .Where(node => node is ForStatementSyntax ||
                                             node is WhileStatementSyntax ||
                                             node is ForEachStatementSyntax)
                                .ToList();

            foreach (var outerLoop in allLoops)
            {
                var innerLoops = outerLoop.DescendantNodes()
                                        .Where(node => node != outerLoop &&
                                                     (node is ForStatementSyntax ||
                                                      node is WhileStatementSyntax ||
                                                      node is ForEachStatementSyntax))
                                        .ToList();

                if (innerLoops.Any())
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Count how many for loops are in the method
        /// </summary>
        public int CountForLoops()
        {
            return method.DescendantNodes().OfType<ForStatementSyntax>().Count();
        }

        /// <summary>
        /// Count how many while loops are in the method
        /// </summary>
        public int CountWhileLoops()
        {
            return method.DescendantNodes().OfType<WhileStatementSyntax>().Count();
        }

        /// <summary>
        /// Count how many foreach loops are in the method
        /// </summary>
        public int CountForEachLoops()
        {
            return method.DescendantNodes().OfType<ForEachStatementSyntax>().Count();
        }

        /// <summary>
        /// Count how many times 'new Node' is created
        /// </summary>
        public int CountNewNodes()
        {
            return method.DescendantNodes()
                        .OfType<ObjectCreationExpressionSyntax>()
                        .Count(c => c.Type.ToString().Contains("Node"));
        }

        /// <summary>
        /// Count how many times SetNext() is called
        /// </summary>
        public int CountSetNext()
        {
            return method.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Count(i => i.ToString().Contains("SetNext"));
        }

        /// <summary>
        /// Count how many times GetNext() is called
        /// </summary>
        public int CountGetNext()
        {
            return method.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Count(i => i.ToString().Contains("GetNext"));
        }

        /// <summary>
        /// Count how many times a new Queue is created
        /// </summary>
        public int CountNewQueue()
        {
            return method.DescendantNodes()
                        .OfType<ObjectCreationExpressionSyntax>()
                        .Count(c => c.Type.ToString().Contains("Queue"));
        }

        /// <summary>
        /// Count how many times a new Stack is created
        /// </summary>
        public int CountNewStack()
        {
            return method.DescendantNodes()
                        .OfType<ObjectCreationExpressionSyntax>()
                        .Count(c => c.Type.ToString().Contains("Stack"));
        }

        /// <summary>
        /// Count how many times a new BinNode is created
        /// </summary>
        public int CountNewBinNode()
        {
            return method.DescendantNodes()
                        .OfType<ObjectCreationExpressionSyntax>()
                        .Count(c => c.Type.ToString().Contains("BinNode"));
        }

        /// <summary>
        /// Count number of if statements
        /// </summary>
        public int CountIfStatements()
        {
            return method.DescendantNodes().OfType<IfStatementSyntax>().Count();
        }

        /// <summary>
        /// Count number of return statements
        /// </summary>
        public int CountReturnStatements()
        {
            return method.DescendantNodes().OfType<ReturnStatementSyntax>().Count();
        }

        /// <summary>
        /// Get method return type as string
        /// </summary>
        public string GetReturnType()
        {
            return method.ReturnType.ToString();
        }

        /// <summary>
        /// Get number of parameters
        /// </summary>
        public int GetParameterCount()
        {
            return method.ParameterList.Parameters.Count;
        }

        /// <summary>
        /// Get parameter type names as they appear in source (e.g., "Queue<int>").
        /// </summary>
        public string[] GetParameterTypes()
        {
            return method.ParameterList.Parameters
                         .Select(p => p.Type?.ToString() ?? string.Empty)
                         .ToArray();
        }

        /// <summary>
        /// Check if method calls a specific method
        /// </summary>
        public bool CallsMethod(string methodName)
        {
            return method.DescendantNodes()
                        .OfType<InvocationExpressionSyntax>()
                        .Any(inv => inv.ToString().Contains(methodName));
        }
    }
}