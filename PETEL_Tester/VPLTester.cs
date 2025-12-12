using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unit4; // Add at top with other using directives

namespace PETEL_VPL
{
    public class VPLTester
    {
        private int grade = 0;
        private List<string> testResults = new List<string>();
        private ObjectComparer comparer;
        private CodeAnalyzer studentCodeAnalyzer;
        private int timeoutMilliseconds;

        private string StudentSourceFilePath { get; }
        private string StudentNamespace { get; }
        private string StudentClassName { get; }
        private string StudentMethodName { get; }
        private string TeacherNamespace { get; }
        private string TeacherClassName { get; }
        private string TeacherMethodName { get; }

        // Show details on both success and failure
        public bool ShowDetails { get; set; }

        public VPLTester(string studentFile, string studentNamespace, string studentClassName,
                         string studentMethodName, string teacherNamespace, string teacherClassName,
                         string teacherMethodName, bool showDetails = false, int timeoutMilliseconds = 2000)
        {
            StudentSourceFilePath = GetStudentSourcePath(studentFile);
            StudentNamespace = studentNamespace;
            StudentClassName = studentClassName;
            StudentMethodName = studentMethodName;
            TeacherNamespace = teacherNamespace;
            TeacherClassName = teacherClassName;
            TeacherMethodName = teacherMethodName;
            ShowDetails = showDetails;
            this.timeoutMilliseconds = timeoutMilliseconds;
            comparer = new ObjectComparer();
        }

        /// <summary>
        /// Initialize code analyzer for student's source file
        /// </summary>
        public void InitializeCodeAnalyzer()
        {
            try
            {
                if (!File.Exists(StudentSourceFilePath))
                {
                    Console.WriteLine($"Warning: Source file not found at '{StudentSourceFilePath}'");
                    Console.WriteLine("Continuing with functional tests only...\n");
                    return;
                }

                studentCodeAnalyzer = new CodeAnalyzer(StudentSourceFilePath);
                //Console.WriteLine("Code analyzer initialized successfully!\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not initialize code analyzer: {ex.Message}");
                Console.WriteLine("Continuing with functional tests only...\n");
                studentCodeAnalyzer = null;
            }
        }

        #region Public Test Methods

        /// <summary>
        /// Unified test method with optional parameters and flags
        /// </summary>
        /// <param name="testName">Name of the test</param>
        /// <param name="points">Points awarded for passing the test</param>
        /// <param name="parameters">Method parameters (optional)</param>
        /// <param name="consoleInput">Console input - can be a single string, string array, or List<string>. Newlines are automatically added.</param>
        /// <param name="captureConsoleOutput">Whether to capture and compare console output instead of return values</param>
        /// <param name="compareParams">Whether to verify that student and teacher produce the same parameter modifications (default: true)</param>
        /// <param name="exceptionComments">Optional mapping from exception type to teacher comment for this test</param>
        public void TestMethod(string testName, int points,
            object[] parameters = null, object consoleInput = null,
            bool captureConsoleOutput = false, bool compareParams = true,
            Dictionary<Type, string> exceptionComments = null)
        {
            object[] originalParams = parameters ?? new object[] { };

            var config = new TestExecutionConfig(testName, points)
            {
                Parameters = originalParams,
                OriginalParameters = ObjectCloning.DeepCloneArray(originalParams),  // ✅ Clone for display
                ConsoleInput = ProcessConsoleInput(consoleInput),
                CaptureOutput = captureConsoleOutput,
                CompareParams = compareParams,
                ExceptionComments = exceptionComments
            };
            ExecuteTest(config);
        }

        /// <summary>
        /// Test code structure requirements 
        /// </summary>
        public void TestCodeStructure(string testName, int points,
            CodeStructureCheck checkType, bool shouldPass = true, int? expectedCount = null, string failureMessage = null)
        {
            TestAssertionException exception = null;

            try
            {
                if (studentCodeAnalyzer == null)
                    throw new TestAssertionException("Code analyzer not initialized. Call InitializeCodeAnalyzer() first.");

                var result = studentCodeAnalyzer.CheckMethodStructure(StudentMethodName, checkType, expectedCount);

                // Invert the result if shouldPass is false (for negative checks)
                bool actualPassed = shouldPass ? result.Passed : !result.Passed;

                if (!actualPassed)
                {
                    string message;
                    if (!string.IsNullOrEmpty(failureMessage))
                    {
                        // Append analyzer description (e.g., expected vs actual) to custom message
                        string desc = result.Description ?? string.Empty;
                        if (!string.IsNullOrEmpty(desc))
                        {
                            // Avoid duplicate phrasing like "Wrong parameter list" twice
                            var fm = failureMessage.Trim();
                            var dd = desc.Trim();
                            if (fm.StartsWith("Wrong parameter list", StringComparison.OrdinalIgnoreCase) && dd.StartsWith("Wrong parameter list", StringComparison.OrdinalIgnoreCase))
                            {
                                // Preserve the 'Expected:' label when trimming
                                int expectedIdx = dd.IndexOf("Expected:", StringComparison.OrdinalIgnoreCase);
                                if (expectedIdx >= 0)
                                    dd = dd.Substring(expectedIdx).Trim();
                                else
                                {
                                    // Fallback: remove the duplicate prefix phrase
                                    dd = dd.Replace("Wrong parameter list.", string.Empty).Trim();
                                }
                            }
                            message = string.IsNullOrEmpty(dd) ? fm : fm + ": " + dd;
                        }
                        else
                        {
                            message = failureMessage;
                        }
                    }
                    else
                    {
                        message = $"Code structure requirement not met: {result.Description}";
                    }

                    if (expectedCount.HasValue && result.Count != expectedCount.Value)
                    {
                        message += $" (Expected: {expectedCount.Value}, Actual: {result.Count})";
                    }
                    throw new TestAssertionException(message);
                }

                // Test passed - add points
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
            testResults.Add(FormatResult(config, exception, null, null, null, null));
        }

        /// <summary>
        /// Get method analyzer for advanced custom checks
        /// Example: if (analyzer.CallsMethod("Console.WriteLine")...
        /// </summary>
        public MethodAnalyzer GetStudentMethodAnalyzer()
        {
            if (studentCodeAnalyzer == null)
                throw new InvalidOperationException("Code analyzer not initialized. Call InitializeCodeAnalyzer() first.");

            return studentCodeAnalyzer.GetMethodAnalyzer(StudentMethodName);
        }

        #endregion

        #region Input Processing

        /// <summary>
        /// Process console input - accepts string, string[], or List<string>
        /// Automatically adds newline characters if not present
        /// </summary>
        private string ProcessConsoleInput(object consoleInput)
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
            return EnsureNewline(consoleInput.ToString());
        }

        /// <summary>
        /// Ensures a string ends with a newline character
        /// </summary>
        private string EnsureNewline(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (!input.EndsWith("\n") && !input.EndsWith("\r\n")) return input + "\n";
            return input;
        }

        #endregion

        #region Core Execution Logic

        // Core method that executes the test based on configuration
        private void ExecuteTest(TestExecutionConfig config)
        {
            TestAssertionException exception = null;
            object studentResult = null;
            object teacherResult = null;
            string studentOutput = null;
            string teacherOutput = null;

            try
            {
                if (config.CaptureOutput)
                    (studentOutput, teacherOutput) = CompareConsoleOutputs(config);
                else
                    (studentResult, teacherResult) = CompareReturnValues(config);

                // Test passed - add points
                grade += config.Points;
            }
            catch (TestAssertionException e) { exception = e; }
            catch (Exception e)
            {
                exception = new TestAssertionException($"Error during test execution: {e.InnerException?.Message ?? e.Message}", e);
            }

            testResults.Add(FormatResult(config, exception, studentResult, teacherResult, studentOutput, teacherOutput));
        }

        // Compare return values from student and teacher methods
        private (object studentResult, object teacherResult) CompareReturnValues(TestExecutionConfig config)
        {
            object[] teacherParams = ObjectCloning.DeepCloneArray(config.Parameters);

            object studentResult = InvokeMethod(
                StudentNamespace, StudentClassName, StudentMethodName,
                config.Parameters, config.ConsoleInput);

            object teacherResult = InvokeMethod(
                TeacherNamespace, TeacherClassName, TeacherMethodName,
                teacherParams, config.ConsoleInput);

            // Compare return values
            if (!comparer.AreEqual(teacherResult, studentResult))
            {
                string expected = Snapshot(teacherResult);
                string actual = Snapshot(studentResult);
                throw new TestAssertionException(
                    "Return value check:\n" +
                    "Expected: " + expected + "\n" +
                    "Actual:   " + actual + "\n" +
                    "Explanation: Returned value differs from the expected result.");
            }

            // Compare parameter modifications if requested
            if (config.CompareParams)
            {
                if (!comparer.AreEqual(teacherParams, config.Parameters))
                {
                    var sb = new StringBuilder();

                    // Return first + explanation
                    sb.AppendLine("Return value check:");
                    sb.AppendLine("Expected: " + Snapshot(teacherResult));
                    sb.AppendLine("Actual:   " + Snapshot(studentResult));
                    sb.AppendLine("Explanation: Returned value matches the expected result.");
                    sb.AppendLine();

                    // Then parameters
                    sb.AppendLine("Input parameter state after call does not match the requirement:");
                    for (int i = 0; i < teacherParams.Length; i++)
                    {
                        string exp = Snapshot(teacherParams[i]);
                        string act = Snapshot(config.Parameters[i]);
                        sb.AppendLine($"p{i}: expected={exp} | actual={act}");
                    }

                    throw new TestAssertionException(sb.ToString().TrimEnd());
                }
            }

            return (studentResult, teacherResult);
        }

        // Compare console outputs from student and teacher methods
        private (string studentOutput, string teacherOutput) CompareConsoleOutputs(TestExecutionConfig config)
        {
            // Clone parameters BEFORE student execution
            object[] teacherParams = ObjectCloning.DeepCloneArray(config.Parameters);

            // Student runs FIRST on ORIGINAL parameters
            string studentOutput = CaptureConsoleOutput(() =>
                InvokeMethod(StudentNamespace, StudentClassName, StudentMethodName,
                    config.Parameters, config.ConsoleInput));

            // Teacher runs on CLONED ORIGINAL parameters
            string teacherOutput = CaptureConsoleOutput(() =>
                InvokeMethod(TeacherNamespace, TeacherClassName, TeacherMethodName,
                    teacherParams, config.ConsoleInput));

            // Compare console output
            if (!comparer.AreEqual(teacherOutput.Trim(), studentOutput.Trim()))
                throw new TestAssertionException(
                    "Stdout check:\n" +
                    "Expected output:\n" + teacherOutput + "\n\n" +
                    "Actual output:\n" + studentOutput + "\n" +
                    "Explanation: Printed output does not match the expected text/format.");

            // Compare parameter modifications if requested
            if (config.CompareParams && !comparer.AreEqual(teacherParams, config.Parameters))
            {
                var sb = new StringBuilder();
                sb.AppendLine("Input parameter state after call does not match the requirement:");
                for (int i = 0; i < teacherParams.Length; i++)
                {
                    string exp = Snapshot(teacherParams[i]);
                    string act = Snapshot(config.Parameters[i]);
                    sb.AppendLine($"p{i}: expected={exp} | actual={act}");
                }
                throw new TestAssertionException(sb.ToString().TrimEnd());
            }

            return (studentOutput, teacherOutput);
        }

        /// <summary>
        /// Helper method to capture console output - FIXED to ensure proper restoration
        /// </summary>
        private string CaptureConsoleOutput(Action action)
        {
            var originalOut = Console.Out;
            StringWriter sw = null;
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

        private Type InferElementTypeFromParams(object[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return null;
            var p = parameters[0];
            if (p == null) return null;
            var pt = p.GetType();
            if (pt.IsGenericType && pt.GetGenericTypeDefinition() == typeof(Unit4.Node<>))
                return pt.GetGenericArguments()[0];
            return null;
        }

        private Type InferElementTypeFromTeacherSignature(string methodName, int paramCount, int paramIndex)
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

        // Replace the existing InvokeMethod with this version

        private object InvokeMethod(string namespaceName, string className, string methodName, object[] parameters, string consoleInput)
        {
            Type type = ResolveType(namespaceName, className);
            if (type == null)
                throw new TestAssertionException($"Class '{className}' not found in namespace '{namespaceName}'");

            // Prefer method candidates by name and parameter count
            var methods = type
                .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)
                .Where(m => m.Name == methodName && m.GetParameters().Length == (parameters?.Length ?? 0))
                .ToArray();

            if (methods.Length == 0)
                throw new TestAssertionException($"Method '{methodName}' not found in class '{className}'");

            // Pick the first candidate (typical classroom projects have a single matching signature)
            var method = methods[0];

            // If the method is a generic definition (e.g., Length<T>(Node<T> head)), close it with the right T
            if (method.ContainsGenericParameters || method.IsGenericMethodDefinition)
            {
                var genArgs = method.GetGenericArguments();
                if (genArgs.Length == 1)
                {
                    // 1) Try to infer T from the runtime parameter (Node<T> head)
                    Type elementType = InferElementTypeFromParams(parameters);

                    // 2) If parameter is null (empty list), infer from the teacher method signature
                    if (elementType == null && namespaceName == StudentNamespace && className == StudentClassName)
                        elementType = InferElementTypeFromTeacherSignature(methodName, parameters?.Length ?? 0, 0);

                    if (elementType == null)
                        throw new TestAssertionException("Unable to infer generic type argument for method invocation (null or open generic).");

                    method = method.MakeGenericMethod(new[] { elementType });
                }
                else
                    throw new TestAssertionException("Unsupported generic method with multiple generic type parameters.");
            }

            var originalIn = Console.In;
            try
            {
                if (consoleInput != null)
                    Console.SetIn(new StringReader(consoleInput));

                object result = null;
                Exception taskException = null;

                var task = Task.Run(() =>
                {
                    try
                    {
                        if (method.IsStatic)
                            return method.Invoke(null, parameters);
                        else
                        {
                            object instance = Activator.CreateInstance(type);
                            return method.Invoke(instance, parameters);
                        }
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

                result = task.Result;
                return result;
            }
            finally
            {
                Console.SetIn(originalIn);
            }
        }

        private Type ResolveType(string namespaceName, string className)
        {
            string qualifiedName = string.IsNullOrWhiteSpace(namespaceName)
                ? className
                : $"{namespaceName}.{className}";

            var type = Type.GetType(qualifiedName, throwOnError: false);
            if (type != null)
                return type;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    type = assembly.GetType(qualifiedName, throwOnError: false, ignoreCase: false);
                    if (type != null)
                        return type;
                }
                catch
                {
                    // Ignore and continue searching other assemblies
                }
            }

            return null;
        }

        /// <summary>
        /// Finds the student source file path.
        /// Compatible with both local testing (Visual Studio) and Moodle VPL.
        /// Returns the first existing path found, or a default path if none exist.
        /// </summary>
        public string GetStudentSourcePath(string StudentFile)
        {
            // Try multiple possible locations in order of preference
            string[] possiblePaths = new[]
            {
                // Moodle VPL: Files are in current directory
                StudentFile,
                
                // Local Visual Studio: Navigate up from bin/Debug
                Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName,
                    StudentFile
                ),
                
                // Alternative: Check parent directory
                Path.Combine("..", StudentFile),
                
                // Alternative: Check two levels up
                Path.Combine("..", "..", StudentFile)
            };

            // Return the first path that exists
            foreach (var path in possiblePaths)
            {
                try
                {
                    if (File.Exists(path))
                    {
                        return Path.GetFullPath(path);
                    }
                }
                catch
                {
                    // Continue to next path if this one causes an error
                    continue;
                }
            }

            // Return default path for VPL if none found (error handling in InitializeCodeAnalyzer)
            return StudentFile;
        }

        #endregion

        #region Output Formatting

        public string FormatResult(TestExecutionConfig config, TestAssertionException e,
            object studentResult, object teacherResult, string studentOutput, string teacherOutput)
        {
            var sb = new StringBuilder();
            var gradeStr = e == null ? config.Points.ToString() : "0";

            sb.Append($"Comment :=>>{config.TestName}");
            if (ShowDetails)
            {
                var headerParts = new List<string>();
                if (config.OriginalParameters != null && config.OriginalParameters.Length > 0)
                    headerParts.Add($"params: {FormatParamsSeparated(config.OriginalParameters)}");
                if (!string.IsNullOrEmpty(config.ConsoleInput))
                    headerParts.Add($"input: \"{NormalizeForDisplay(config.ConsoleInput)}\"");
                if (headerParts.Count > 0)
                {
                    sb.Append(" | ");
                    sb.Append(string.Join(" | ", headerParts));
                }
            }
            sb.Append($": {(e == null ? "success" : "failure")}. {gradeStr} points");

            if (e != null)
            {
                sb.AppendLine();
                sb.AppendLine("<|--");

                // Print failure message only; details live in the message itself
                sb.AppendLine(e.Message);

                // Append teacher-friendly note if mapping exists
                if (config.ExceptionComments != null && config.ExceptionComments.Count > 0)
                {
                    // Unwrap to root runtime exception
                    Exception root = e.InnerException ?? (Exception)e;
                    if (root is AggregateException ae && ae.InnerExceptions.Count == 1)
                        root = ae.InnerExceptions[0];
                    if (root is TargetInvocationException tie && tie.InnerException != null)
                        root = tie.InnerException;

                    if (root != null &&
                        config.ExceptionComments.TryGetValue(root.GetType(), out var teacherComment) &&
                        !string.IsNullOrWhiteSpace(teacherComment))
                    {
                        sb.AppendLine();
                        sb.AppendLine("Teacher note: " + teacherComment);
                    }
                }

                sb.AppendLine("--|>");
            }
            else if (ShowDetails)
            {
                sb.AppendLine();
                sb.AppendLine("<|--");
                if (config.CaptureOutput && studentOutput != null)
                    sb.AppendLine("output: \"" + NormalizeForDisplay(studentOutput) + "\"");
                else if (studentResult != null)
                    sb.AppendLine("return: " + Snapshot(studentResult));
                sb.AppendLine("--|>");
            }

            sb.AppendLine();
            return sb.ToString();
        }

        // Formats parameters as p0=..., p1=..., p2=... with list brackets when needed
        private string FormatParamsSeparated(object[] parameters)
        {
            var parts = new List<string>();
            for (int i = 0; i < parameters.Length; i++)
            {
                parts.Add($"p{i}={FormatParameterForHeader(parameters[i])}");
            }
            return string.Join(", ", parts);
        }

        // Header-friendly parameter formatting: show Node<T> with brackets
        private string FormatParameterForHeader(object param)
        {
            if (param == null) return "null";
            var t = param.GetType();
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Node<>))
                return Unit4Helper.NodeListToString(param, includeBrackets: true);
            return FormatParameter(param);
        }

        // SINGLE-LINE HELPER (minimal addition)
        private static string SingleLine(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var cleaned = s
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Replace("\t", " ");
            while (cleaned.Contains("  "))
                cleaned = cleaned.Replace("  ", " ");
            return cleaned.Trim();
        }

        /// <summary>
        /// Formats a parameter for compact display (used elsewhere)
        /// </summary>
        private string FormatParameter(object param)
        {
            if (param == null) return "null";
            var t = param.GetType();
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Node<>))
                return Unit4Helper.NodeListToString(param, includeBrackets: false);
            if (param is string str) return $"\"{SingleLine(str)}\"";
            if (param is Array arr)
            {
                var elements = new List<string>();
                foreach (var item in arr) elements.Add(FormatParameter(item));
                return $"[{string.Join(", ", elements)}]";
            }
            // Ensure complex structure ToString (e.g. Stack<Queue<int>>) rendered on one line
            return SingleLine(param.ToString());
        }

        /// <summary>
        /// Formats parameters array for error messages
        /// </summary>
        private string FormatParametersForError(object[] parameters)
        {
            if (parameters == null || parameters.Length == 0) return "  (no parameters)";
            var sb = new StringBuilder();
            for (int i = 0; i < parameters.Length; i++)
                sb.AppendLine($"  [{i}]: {FormatParameter(parameters[i])}");
            return sb.ToString();
        }

        public string FormatResponse() => string.Join("\n", testResults);
        public int GetGrade() => grade;

        // Add this helper to sanitize console text for display
        private static string NormalizeForDisplay(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            // Normalize CRLF to LF, then escape control chars and quotes
            var normalized = text
                .Replace("\r\n", "\n")
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("\"", "\\\"");
            return normalized.TrimEnd(); // avoid trailing whitespace/newlines
        }
        #endregion

        #region Snapshot Helpers (Fix cycle false-positive)
        private string Snapshot(object value)
        {
            if (value == null) return "null";
            var t = value.GetType();

            // Unit4.Node<T> as linear list
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Node<>))
                return "[" + TraverseNodeList(value, 256) + "]";

            // Arrays (primitive or nested)
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

            // Explicitly match Unit4.Queue<T> (avoid System.Collections.Generic.Queue<T>)
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Unit4.Queue<>))
                return "Queue<" + SnapshotQueue(value, 256) + ">";

            // Explicitly match Unit4.Stack<T>
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Unit4.Stack<>))
                return "Stack<" + SnapshotStack(value, 256) + ">";

            // Explicitly match Unit4.BinNode<T>
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Unit4.BinNode<>))
                return SnapshotBinNode(value, 0, 64);

            return value.ToString();
        }

        private string TraverseNodeList(object head, int max)
        {
            if (head == null) return "";
            var type = head.GetType();
            var getValue = type.GetMethod("GetValue");
            var getNext = type.GetMethod("GetNext");

            var sb = new StringBuilder();
            var seen = new HashSet<object>(ReferenceEqualityComparer.Instance);

            object current = head;
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
                // IMPORTANT: recurse for nested structures
                sb.Append(Snapshot(val));
                first = false;
                count++;

                current = getNext.Invoke(current, null);
            }

            if (count >= max)
                sb.Append(", ...truncated...");

            return sb.ToString();
        }

        private string SnapshotQueue(object queueObj, int maxItems)
        {
            var qt = queueObj.GetType();
            var isEmpty = qt.GetMethod("IsEmpty");
            var clone = qt.GetMethod("Clone");
            var remove = qt.GetMethod("Remove");

            // Non-destructive: work on a clone if available
            object temp = clone != null ? clone.Invoke(queueObj, new[] { queueObj }) : queueObj;

            var sb = new StringBuilder();
            sb.Append("[");
            int shown = 0;
            bool first = true;

            while (!(bool)isEmpty.Invoke(temp, null) && shown < maxItems)
            {
                var item = remove.Invoke(temp, null);
                if (!first) sb.Append(", ");
                sb.Append(Snapshot(item));
                first = false;
                shown++;
            }

            if (!(bool)isEmpty.Invoke(temp, null))
                sb.Append(", ...truncated...");

            sb.Append("]");
            return sb.ToString();
        }

        private string SnapshotStack(object stackObj, int maxItems)
        {
            var st = stackObj.GetType();
            var isEmpty = st.GetMethod("IsEmpty");
            var pop = st.GetMethod("Pop");
            var push = st.GetMethod("Push");

            var values = new List<object>();
            // Pop to get from top downward
            while (!(bool)isEmpty.Invoke(stackObj, null) && values.Count < maxItems)
                values.Add(pop.Invoke(stackObj, null));

            // Build snapshot: values[0] is top
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < values.Count; i++)
            {
                if (i > 0) sb.Append(", ");
                sb.Append(Snapshot(values[i]));
            }
            // If there were more elements beyond the limit, we can't know here; assume truncated if stack not empty
            if (!(bool)isEmpty.Invoke(stackObj, null))
                sb.Append(", ...truncated...");
            sb.Append("]");

            // Restore stack (push back in reverse)
            for (int i = values.Count - 1; i >= 0; i--)
                push.Invoke(stackObj, new[] { values[i] });

            return sb.ToString();
        }

        private string SnapshotBinNode(object nodeObj, int depth, int maxDepth)
        {
            if (nodeObj == null) return "null";
            if (depth > maxDepth) return "...depth-limit...";

            var t = nodeObj.GetType();
            var getValue = t.GetMethod("GetValue");
            var getLeft = t.GetMethod("GetLeft");
            var getRight = t.GetMethod("GetRight");

            var val = getValue.Invoke(nodeObj, null);
            var left = getLeft.Invoke(nodeObj, null);
            var right = getRight.Invoke(nodeObj, null);

            // Preorder: (value left right)
            return "("
                + Snapshot(val) + " "
                + SnapshotBinNode(left, depth + 1, maxDepth) + " "
                + SnapshotBinNode(right, depth + 1, maxDepth) + ")";
        }

        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            private ReferenceEqualityComparer() { }

            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
        #endregion
    }

    // Custom exception class to replace NUnit's AssertionException
    public class TestAssertionException : Exception
    {
        public TestAssertionException(string message) : base(message) { }
        public TestAssertionException(string message, Exception innerException) : base(message, innerException) { }
    }

    // DTO to encapsulate test execution configuration
    public class TestExecutionConfig
    {
        public object[] Parameters { get; set; }
        public object[] OriginalParameters { get; set; }  // ✅ Added for display
        public string ConsoleInput { get; set; }
        public bool CaptureOutput { get; set; }
        public bool CompareParams { get; set; }
        public string TestName { get; set; }
        public int Points { get; set; }

        // NEW: Optional per-test exception comment mapping
        public Dictionary<Type, string> ExceptionComments { get; set; }

        public TestExecutionConfig(string testName, int points)
        {
            TestName = testName;
            Points = points;
            Parameters = new object[] { };
            OriginalParameters = new object[] { };  // ✅ Initialize
            ConsoleInput = null;
            CaptureOutput = false;
            CompareParams = true;  // Default to true
            ExceptionComments = null; // Optional
        }
    }
}
