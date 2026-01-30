using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Unit4;

namespace PETEL_VPL
{
    public class VPLTester
    {
        private int grade = 0;
        private readonly List<string> testResults = [];
        private readonly ObjectComparer comparer;
        private CodeAnalyzer? studentCodeAnalyzer;
        private readonly int timeoutMilliseconds;

        public int TimeoutMilliseconds => timeoutMilliseconds;

        public string StudentSourceFilePath { get; set; }
        public string StudentNamespace { get; set; }
        public string StudentClassName { get; set; }
        public string StudentMethodName { get; set; }
        public string TeacherNamespace { get; set; }
        public string TeacherClassName { get; set; }
        public string TeacherMethodName { get; set; }

        public bool ShowDetails { get; set; } = true; // Default to true

        public VPLTester(
            string studentMethodName,
            string? teacherMethodName = null,
            string studentFile = "StudentAnswer.cs",
            string studentNamespace = "",
            string studentClassName = "StudentAnswer",
            string teacherNamespace = "",
            string teacherClassName = "TeacherAnswer",
            int timeoutMilliseconds = 500)
        {
            StudentSourceFilePath = GetStudentSourcePath(studentFile);
            StudentNamespace = studentNamespace;
            StudentClassName = studentClassName;
            StudentMethodName = studentMethodName;
            TeacherNamespace = teacherNamespace;
            TeacherClassName = teacherClassName;
            TeacherMethodName = teacherMethodName ?? studentMethodName;
            this.timeoutMilliseconds = timeoutMilliseconds;
            comparer = new ObjectComparer();
            
            // Initialize code analyzer automatically
            InitializeCodeAnalyzer();
        }

        private static string GetStudentSourcePath(string studentFile)
        {
            var baseDir = AppContext.BaseDirectory;
            var currentDir = Directory.GetCurrentDirectory();

            string[] possiblePaths =
            {
                studentFile,
                Path.Combine(currentDir, studentFile),
                Path.Combine(baseDir, studentFile),
                Path.Combine(baseDir, "..", studentFile),
                Path.Combine(baseDir, "..", "..", studentFile),
                Path.Combine(baseDir, "..", "..", "..", studentFile),
                Path.Combine(baseDir, "..", "..", "..", "..", studentFile)
            };

            foreach (var p in possiblePaths)
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(p) && File.Exists(p))
                        return Path.GetFullPath(p);
                }
                catch
                {
                    // ignore and continue
                }
            }

            return studentFile;
        }

        public void AddInfo(string message)
        {
            testResults.Add($"Comment :=>>{message}: success. 0 points\n");
        }

        public void InitializeCodeAnalyzer()
        {
            try
            {
                if (!File.Exists(StudentSourceFilePath))
                    return;

                studentCodeAnalyzer = new CodeAnalyzer(StudentSourceFilePath);
            }
            catch
            {
                studentCodeAnalyzer = null;
            }
        }

        public void TestMethod(
            string testName,
            int points,
            object[]? parameters = null,
            object? consoleInput = null,
            bool captureConsoleOutput = false,
            bool compareParams = true,
            Dictionary<Type, string>? exceptionComments = null,
            bool compareReturn = true)
        {
            object[] originalParams = parameters ?? [];

            var config = new TestExecutionConfig(testName, points)
            {
                Parameters = originalParams,
                OriginalParameters = ObjectCloning.DeepCloneArray(originalParams),
                ConsoleInput = ProcessConsoleInput(consoleInput),
                CaptureOutput = captureConsoleOutput,
                CompareParams = compareParams,
                ExceptionComments = exceptionComments,
                CompareReturn = compareReturn
            };

            ExecuteTest(config);
        }

        public void TestCodeStructure(
            string testName,
            int points,
            CodeStructureCheck checkType,
            bool shouldPass = true,
            int? expectedCount = null,
            string? failureMessage = null)
        {
            TestAssertionException? exception = null;

            try
            {
                if (studentCodeAnalyzer == null)
                    throw new TestAssertionException("Code analyzer not initialized. Call InitializeCodeAnalyzer() first.");

                var result = studentCodeAnalyzer.CheckMethodStructure(StudentMethodName, checkType, expectedCount);
                bool actualPassed = shouldPass ? result.Passed : !result.Passed;

                if (!actualPassed)
                {
                    string message;
                    if (!string.IsNullOrEmpty(failureMessage))
                    {
                        var desc = result.Description ?? string.Empty;
                        message = string.IsNullOrEmpty(desc) ? failureMessage : failureMessage + ": " + desc;
                    }
                    else
                    {
                        message = $"Code structure requirement not met: {result.Description}";
                    }

                    if (expectedCount.HasValue && result.Count != expectedCount.Value)
                        message += $" (Expected: {expectedCount.Value}, Actual: {result.Count})";

                    throw new TestAssertionException(message);
                }

                grade += points;
            }
            catch (TestAssertionException e)
            {
                exception = e;
            }
            catch (Exception e)
            {
                exception = new TestAssertionException($"Error during code analysis: {e.Message}", e);
            }

            var config = new TestExecutionConfig(testName, points);
            testResults.Add(FormatResult(config, exception));
        }

        public MethodAnalyzer GetStudentMethodAnalyzer()
        {
            if (studentCodeAnalyzer == null)
                throw new InvalidOperationException("Code analyzer not initialized. Call InitializeCodeAnalyzer() first.");

            return studentCodeAnalyzer.GetMethodAnalyzer(StudentMethodName);
        }

        private string? ProcessConsoleInput(object? consoleInput)
        {
            if (consoleInput == null) return null;
            if (consoleInput is string s) return EnsureNewline(s);
            if (consoleInput is string[] sa)
            {
                var sb = new StringBuilder();
                foreach (var v in sa) sb.Append(EnsureNewline(v));
                return sb.ToString();
            }
            if (consoleInput is List<string> list)
            {
                var sb = new StringBuilder();
                foreach (var v in list) sb.Append(EnsureNewline(v));
                return sb.ToString();
            }
            return EnsureNewline(consoleInput.ToString() ?? string.Empty);
        }

        private static string EnsureNewline(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (!input.EndsWith("\n") && !input.EndsWith("\r\n")) return input + "\n";
            return input;
        }

        private void ExecuteTest(TestExecutionConfig config)
        {
            TestAssertionException? exception = null;

            try
            {
                bool needsFunctionalComparison = config.CompareReturn || config.CompareParams;
                if (needsFunctionalComparison)
                {
                    TextWriter originalOut = Console.Out;
                    StringWriter suppress = new();
                    Console.SetOut(suppress);
                    try
                    {
                        CompareReturnValues(config);
                    }
                    finally
                    {
                        Console.SetOut(originalOut);
                        suppress.Dispose();
                    }
                }

                if (config.CaptureOutput)
                    CompareConsoleOutputs(config);

                grade += config.Points;
            }
            catch (TestAssertionException e) { exception = e; }
            catch (Exception e)
            {
                exception = new TestAssertionException($"Error during test execution: {e.InnerException?.Message ?? e.Message}", e);
            }

            testResults.Add(FormatResult(config, exception));
        }

        private void CompareReturnValues(TestExecutionConfig config)
        {
            object[] teacherParams = ObjectCloning.DeepCloneArray(config.Parameters);

            object? studentResult = InvokeMethod(StudentNamespace, StudentClassName, StudentMethodName, config.Parameters, config.ConsoleInput);
            object? teacherResult = InvokeMethod(TeacherNamespace, TeacherClassName, TeacherMethodName, teacherParams, config.ConsoleInput);

            if (config.CompareReturn)
            {
                bool bothNull = teacherResult == null && studentResult == null;
                if (!bothNull && !comparer.AreEqual(teacherResult!, studentResult!))
                {
                    throw new TestAssertionException(
                        "Return value check:\n" +
                        "Expected: " + Snapshot(teacherResult) + "\n" +
                        "Actual:   " + Snapshot(studentResult) + "\n" +
                        "Explanation: Returned value differs from the expected result.");
                }
            }

            if (config.CompareParams)
            {
                if (!comparer.AreEqual(teacherParams, config.Parameters))
                {
                    var sb = new StringBuilder();
                    sb.AppendLine("Input parameter state after call does not match the requirement:");
                    for (int i = 0; i < teacherParams.Length; i++)
                    {
                        sb.AppendLine($"p{i}: expected={Snapshot(teacherParams[i])} | actual={Snapshot(config.Parameters[i])}");
                    }

                    throw new TestAssertionException(sb.ToString().TrimEnd());
                }
            }
        }

        private void CompareConsoleOutputs(TestExecutionConfig config)
        {
            object[] teacherParams = ObjectCloning.DeepCloneArray(config.Parameters);

            string studentOutput = CaptureConsoleOutput(() =>
                InvokeMethod(StudentNamespace, StudentClassName, StudentMethodName, config.Parameters, config.ConsoleInput));

            string teacherOutput = CaptureConsoleOutput(() =>
                InvokeMethod(TeacherNamespace, TeacherClassName, TeacherMethodName, teacherParams, config.ConsoleInput));

            if (!comparer.AreEqual(teacherOutput.Trim(), studentOutput.Trim()))
                throw new TestAssertionException(
                    "Stdout check:\n" +
                    "Expected output:\n" + teacherOutput + "\n\n" +
                    "Actual output:\n" + studentOutput + "\n" +
                    "Explanation: Printed output does not match the expected text/format.");

            if (config.CompareParams && !comparer.AreEqual(teacherParams, config.Parameters))
            {
                var sb = new StringBuilder();
                sb.AppendLine("Input parameter state after call does not match the requirement:");
                for (int i = 0; i < teacherParams.Length; i++)
                {
                    sb.AppendLine($"p{i}: expected={Snapshot(teacherParams[i])} | actual={Snapshot(config.Parameters[i])}");
                }
                throw new TestAssertionException(sb.ToString().TrimEnd());
            }
        }

        private static string CaptureConsoleOutput(Action action)
        {
            var originalOut = Console.Out;
            StringWriter? sw = null;
            try
            {
                sw = new StringWriter();
                Console.SetOut(sw);
                action();
                Console.Out.Flush();
                return sw.ToString();
            }
            finally
            {
                Console.SetOut(originalOut);
                sw?.Dispose();
            }
        }

        private Type? InferElementTypeFromParams(object[]? parameters)
        {
            if (parameters == null || parameters.Length == 0) return null;
            var p = parameters[0];
            if (p == null) return null;
            var pt = p.GetType();
            if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Unit4.Node<>))
                return pt.GetGenericArguments()[0];
            return null;
        }

        private Type? InferElementTypeFromTeacherSignature(string methodName, int paramCount, int paramIndex)
        {
            try
            {
                var teacherType = Type.GetType($"{TeacherNamespace}.{TeacherClassName}");
                if (teacherType == null) return null;

                var candidate = teacherType
                    .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                    .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == paramCount);

                if (candidate == null) return null;

                var paramType = candidate.GetParameters()[paramIndex].ParameterType;
                if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Unit4.Node<>))
                    return paramType.GetGenericArguments()[0];
            }
            catch { }
            return null;
        }

        private object? InvokeMethod(string namespaceName, string className, string methodName, object[] parameters, string? consoleInput)
        {
            Type? type = ResolveType(namespaceName, className);
            if (type == null)
                throw new TestAssertionException($"Class '{className}' not found in namespace '{namespaceName}'");

            var methods = type
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(m => m.Name == methodName && m.GetParameters().Length == (parameters?.Length ?? 0))
                .ToArray();

            if (methods.Length == 0)
                throw new TestAssertionException($"Method '{methodName}' not found in class '{className}'");

            var method = methods[0];

            if (method.ContainsGenericParameters || method.IsGenericMethodDefinition)
            {
                var genArgs = method.GetGenericArguments();
                if (genArgs.Length == 1)
                {
                    Type? elementType = InferElementTypeFromParams(parameters);
                    if (elementType == null && namespaceName == StudentNamespace && className == StudentClassName)
                        elementType = InferElementTypeFromTeacherSignature(methodName, parameters?.Length ?? 0, 0);

                    if (elementType == null)
                        throw new TestAssertionException("Unable to infer generic type argument for method invocation (null or open generic).");

                    method = method.MakeGenericMethod([elementType]);
                }
                else
                    throw new TestAssertionException("Unsupported generic method with multiple generic type parameters.");
            }

            var originalIn = Console.In;
            try
            {
                if (consoleInput != null)
                    Console.SetIn(new StringReader(consoleInput));

                Exception? taskException = null;

                var task = Task.Run(() =>
                {
                    try
                    {
                        if (method.IsStatic)
                            return method.Invoke(null, parameters);

                        object instance = Activator.CreateInstance(type)!;
                        return method.Invoke(instance, parameters);
                    }
                    catch (Exception ex)
                    {
                        taskException = ex;
                        return null;
                    }
                });

                if (!task.Wait(timeoutMilliseconds))
                    throw new TestAssertionException($"Method execution exceeded time limit of {timeoutMilliseconds}ms. Possible infinite loop or excessive computation detected.");

                if (taskException != null)
                    throw taskException;

                return task.Result;
            }
            finally
            {
                Console.SetIn(originalIn);
            }
        }

        private static Type? ResolveType(string namespaceName, string className)
        {
            string qualifiedName = string.IsNullOrWhiteSpace(namespaceName)
                ? className
                : $"{namespaceName}.{className}";

            var type = Type.GetType(qualifiedName, throwOnError: false);
            if (type != null) return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    type = assembly.GetType(qualifiedName, throwOnError: false, ignoreCase: false);
                    if (type != null) return type;
                }
                catch { }
            }

            return null;
        }

        public string FormatResponse() => string.Join("\n", testResults);
        public int GetGrade() => grade;

        private string FormatResult(TestExecutionConfig config, TestAssertionException? e)
        {
            var sb = new StringBuilder();
            var gradeStr = e == null ? config.Points.ToString() : "0";

            sb.Append($"Comment :=>>{config.TestName}");
            sb.Append($": {(e == null ? "success" : "failure")}. {gradeStr} points");

            if (e != null)
            {
                sb.AppendLine();
                sb.AppendLine("<|--");
                sb.AppendLine(e.Message);
                sb.AppendLine("--|>");
            }

            sb.AppendLine();
            return sb.ToString();
        }

        private string Snapshot(object? value)
        {
            if (value == null) return "null";
            var t = value.GetType();

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Node<>))
                return "[" + TraverseNodeList(value, 256) + "]";

            if (value is Array arr)
            {
                var sbArr = new StringBuilder();
                sbArr.Append("[");
                for (int i = 0; i < arr.Length; i++)
                {
                    if (i > 0) sbArr.Append(", ");
                    sbArr.Append(Snapshot(arr.GetValue(i)));
                }
                sbArr.Append("]");
                return sbArr.ToString();
            }

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Unit4.Queue<>))
                return "Queue<" + SnapshotQueue(value, 256) + ">";

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Unit4.Stack<>))
                return "Stack<" + SnapshotStack(value, 256) + ">";

            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Unit4.BinNode<>))
                return SnapshotBinNode(value, 0, 64);

            return value.ToString() ?? string.Empty;
        }

        private string TraverseNodeList(object head, int max)
        {
            var type = head.GetType();
            var getValue = type.GetMethod("GetValue")!;
            var getNext = type.GetMethod("GetNext")!;

            var sb = new StringBuilder();
            var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);

            object? current = head;
            int count = 0;
            bool first = true;

            while (current != null && count < max)
            {
                if (!seen.Add(current))
                {
                    sb.Append(", ...cycle...");
                    break;
                }

                var val = getValue.Invoke(current, null);
                if (!first) sb.Append(", ");
                sb.Append(Snapshot(val));
                first = false;
                count++;

                current = getNext.Invoke(current, null);
            }

            if (count >= max)
                sb.Append(", ...truncated...");

            return sb.ToString();
        }

        private static string SnapshotQueue(object queueObj, int maxItems)
        {
            var qt = queueObj.GetType();
            var isEmpty = qt.GetMethod("IsEmpty")!;
            var clone = qt.GetMethod("Clone");
            var remove = qt.GetMethod("Remove")!;

            object temp = clone != null ? clone.Invoke(queueObj, [queueObj])! : queueObj;

            var sb = new StringBuilder();
            sb.Append("[");
            int shown = 0;
            bool first = true;

            while (!(bool)isEmpty.Invoke(temp, null)! && shown < maxItems)
            {
                var item = remove.Invoke(temp, null);
                if (!first) sb.Append(", ");
                sb.Append(item == null ? "null" : item.ToString());
                first = false;
                shown++;
            }

            if (!(bool)isEmpty.Invoke(temp, null)!)
                sb.Append(", ...truncated...");

            sb.Append("]");
            return sb.ToString();
        }

        private static string SnapshotStack(object stackObj, int maxItems)
        {
            var st = stackObj.GetType();
            var isEmpty = st.GetMethod("IsEmpty")!;
            var pop = st.GetMethod("Pop")!;
            var push = st.GetMethod("Push")!;

            var values = new List<object?>();
            while (!(bool)isEmpty.Invoke(stackObj, null)! && values.Count < maxItems)
                values.Add(pop.Invoke(stackObj, null));

            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < values.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(values[i] == null ? "null" : values[i]!.ToString());
            }
            if (!(bool)isEmpty.Invoke(stackObj, null)!)
                sb.Append(", ...truncated...");
            sb.Append("]");

            for (int i = values.Count - 1; i >= 0; i--)
                push.Invoke(stackObj, [values[i]]);

            return sb.ToString();
        }

        private string SnapshotBinNode(object nodeObj, int depth, int maxDepth)
        {
            if (depth > maxDepth) return "...depth-limit...";

            var t = nodeObj.GetType();
            var getValue = t.GetMethod("GetValue")!;
            var getLeft = t.GetMethod("GetLeft")!;
            var getRight = t.GetMethod("GetRight")!;

            var val = getValue.Invoke(nodeObj, null);
            var left = getLeft.Invoke(nodeObj, null);
            var right = getRight.Invoke(nodeObj, null);

            return "(" + Snapshot(val) + " " + (left == null ? "null" : SnapshotBinNode(left, depth + 1, maxDepth)) + " " + (right == null ? "null" : SnapshotBinNode(right, depth + 1, maxDepth)) + ")";
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new();
            private ReferenceEqualityComparer() { }
            public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }

    public class TestAssertionException : Exception
    {
        public TestAssertionException(string message) : base(message) { }
        public TestAssertionException(string message, Exception innerException) : base(message, innerException) { }
    }

    public class TestExecutionConfig
    {
        public string TestName { get; }
        public int Points { get; }
        public object[] Parameters { get; set; } = [];
        public object[]? OriginalParameters { get; set; }
        public string? ConsoleInput { get; set; }
        public bool CaptureOutput { get; set; }
        public bool CompareParams { get; set; }
        public Dictionary<Type, string>? ExceptionComments { get; set; }
        public bool CompareReturn { get; set; }

        public TestExecutionConfig(string testName, int points)
        {
            TestName = testName;
            Points = points;
        }
    }
}
