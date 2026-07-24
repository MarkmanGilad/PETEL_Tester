# PeTel Tester V2 — engineering summary

This document describes the V2 implementation and its execution/deployment model. It is intended for engineers maintaining the tester, not for task authors or students.

## Scope and projects

V2 targets **.NET Framework 4.7.2** so that it can run under Mono in PeTel/Moodle VPL:

| Project | Role |
| --- | --- |
| `PETEL_MainTester_V2` | Main executable. Discovers tests, runs every functional case in an isolated runner process, runs code-structure tests locally, and writes VPL-format results. |
| `PETEL_Runner_V2` | Child executable. Runs one `Case_...` method and returns a machine-readable result packet. |
| `PETEL_V2_Core` | Shared framework: test API, runner hosting, reflection invocation, input cloning/comparison, Unit4 data structures/helpers, and Roslyn source analysis. |
| `Upload_V2_Net472` | Files uploaded to each VPL question: common payload/scripts plus the task-specific teacher answer, tests, and entry point. |

The expected task types are loaded through reflection. The default test class is `PETEL_VPL.TestCases`; teacher and student defaults are `TeacherAnswer` and `StudentAnswer` in the global namespace.

## End-to-end execution

```text
PETEL_MainTester_V2/Program
  -> MainTesterHost.Run(CreateTester, "PETEL_VPL.TestCases")
     -> enumerate public static void methods(TestCases(VPLTester))
        -> Code_*: execute in the main process
        -> every other case: start PETEL_Runner_V2 with --method <case>
           -> runner loads task assemblies and invokes the one case
           -> VPLTester invokes student and teacher methods, compares results
           -> runner emits PETEL_V2|<points>|<base64 response>
     -> aggregate points and print `Grade :=>> <total>`
```

Functional cases run in a separate process. `MainTesterHost` enforces `VPLTester.TimeoutMilliseconds` (default 500 ms), kills a timed-out runner, detects stack-overflow exits where possible, and turns runner failures into controlled VPL feedback. Isolation prevents a crash, an infinite loop, or static state in one case from breaking subsequent cases.

## `PETEL_Runner_V2`

`Program.cs` is the active runner implementation.

- Parses `--method`, optional `--testCases`, and optional `--probeDir`.
- Searches the runner directory, working directory, probe directory, parents, and `bin/Debug|Release/net472` locations for the main/test/student/teacher assemblies. It loads assemblies with `Assembly.LoadFrom`.
- Finds `TestCases.CreateTester()` and a public static `void CaseName(VPLTester)` method through reflection.
- Creates a tester, invokes that single test method, then serializes `GetGrade()` and `FormatResponse()` as one protocol line: `PETEL_V2|points|base64(UTF-8 feedback)`.
- Exceptions are converted to protocol failures rather than raw console output.

`SingleTestRunner.cs` is a legacy/alternate helper that finds `PETEL_VPL.TestCases` and invokes one method. It is not called by the current `Program.cs` flow.

## `PETEL_V2_Core`

### Main orchestration — `MainTesterHost`

`MainTesterHost.Run` identifies test methods by signature, sorted by name. Methods whose names begin with `Code_` are executed in-process; all other matching methods are sent to the runner. It locates the runner as `Runner.exe`, `PETEL_Runner_V2.exe`, or a `.dll` launched through `mono`/`dotnet`, depending on the deployment environment.

The runner packet avoids accidental corruption of result parsing by student output. The host decodes the packet, wraps nonstandard errors as VPL comments, and aggregates grades.

### Test API and runtime comparison — `VPLTester`

Task test methods call:

```csharp
tester.TestMethod(testName, points, parameters,
                  consoleInput, captureConsoleOutput,
                  compareParams, exceptionComments, compareReturn);
```

`TestMethod` constructs a `TestExecutionConfig` and then runs the student method and teacher method by reflection. `CreateTester()` configures method, class, namespace, file, and timeout defaults. Generic student methods are supported for the limited inferred-type cases handled by `InvokeMethod`.

For ordinary return-value comparison:

1. `teacherParams` is made with `ObjectCloning.DeepCloneArray(config.Parameters)`.
2. The student is invoked with the original parameter array.
3. The teacher is invoked with `teacherParams`.
4. Return values are compared when `compareReturn` is true.
5. Post-call parameter states are compared when `compareParams` is true (default).

Thus the teacher is normally protected from mutation of the input object by the student, while the optional parameter comparison detects whether student mutation matched the teacher's required side effect.

`captureConsoleOutput` invokes both methods again while redirecting `Console.Out`, then compares trimmed output and, optionally, parameter state. Note that this second pass takes its clone at the start of the console-comparison pass; if a preceding return comparison already mutated `config.Parameters`, the console pass begins from that mutated state. `TestExecutionConfig.OriginalParameters` is cloned when the test is registered but is not currently used by the comparison flow.

`TestCodeStructure` delegates to `CodeAnalyzer`. Results and exceptions are formatted as VPL `Comment :=>>` blocks. The framework supports custom messages for expected exception types, and `TestCases` may expose `TimeoutComment` and `StackOverflowComment` for failures that happen outside normal test execution.

### Cloning — `ObjectCloning`

The cloner handles value types/strings, Unit4 `Node<T>`, `Queue<T>`, `Stack<T>`, `BinNode<T>`, arrays, and arbitrary objects through reflection over instance fields. Queue and stack cloning temporarily drains and restores the source while building the clone.

`DeepCloneArray` currently calls `DeepClone` separately for each parameter. This protects the teacher from basic student mutation, but it does **not preserve aliases between parameters**. For example, if parameters contain a list head and a pointer to a node inside that list, the pointer is separately cloned and no longer points inside the cloned list. A future graph-aware implementation should share one original-to-clone reference map across all parameters and register clones before cloning their child references.

### Equality and display — `ObjectComparer`

`ObjectComparer` provides the framework's structural comparisons for return values and parameter postconditions. `VPLTester` also has snapshot formatting for diagnostics, including Unit4 linked lists, queues, stacks, and binary trees. Snapshotting tries to avoid destructive observation of queues by using `Clone` when present.

### Unit4 support — `Unit4` and helpers

`Unit4.cs` contains the Unit4 data structures used in school tasks (`Node<T>`, `Queue<T>`, `Stack<T>`, `BinNode<T>`) and helper operations. The `Unit4Helper` APIs build structures from arrays/files and convert/print structures for test authoring and diagnostics. The tester's cloning and snapshot code explicitly recognizes these structures.

### Static analysis — `CodeAnalyzer`

`CodeAnalyzer` uses Roslyn (`Microsoft.CodeAnalysis.Common` and `Microsoft.CodeAnalysis.CSharp`, version 4.10.0) to parse the student source. It exposes method-level checks through `MethodAnalyzer` and `CodeStructureCheck`, including recursion, loops, signature/parameter counts, and related syntax/structure requirements. It analyzes source only; it does not execute the student method.

## Local example — `PETEL_MainTester_V2`

This project is the local executable and an example task.

### `Program.cs`

The program fixes the default test type name to `PETEL_VPL.TestCases`, obtains its public static `CreateTester` method, converts it to `Func<VPLTester>`, and passes it to `MainTesterHost.Run`. To use a differently named test class, change `TestCasesTypeName` and rebuild the executable/payload configuration consistently.

### `TestCases.cs`

The example defines `PETEL_VPL.TestCases`:

- `CreateTester()` creates a `VPLTester` for `StudentAnswer.CountValues` and, by default, compares it against `TeacherAnswer.CountValues`.
- `Case_2_EmptyList` and `Case_2_AllMatch` build `Node<int>` inputs using `Unit4Helper.BuildNodeList` and call `TestMethod` with points and parameters.
- `Code_1_MustUseRecursion` calls `TestCodeStructure` to require recursion, forbid `for` loops, and validate the two-parameter signature.
- `TimeoutComment` and `StackOverflowComment` provide task-specific messages used by the host after runner failures.

Other `TestCases*.cs` files are alternative local examples. Only the class named by `Program.cs` is selected.

### `TeacherAnswer.cs`

Contains the reference implementation used as the expected behavior. In the active sample, `CountValues(Node<int> head, int value)` recursively counts matching list values. The file also contains additional reference methods (`DFS_FindMax`, `CopyStack`, and `PrintList`) that can be selected by different test cases.

### `StudentAnswer.cs`

Is a local stand-in for a student submission. It includes one correct `CountValues` implementation and deliberately incorrect alternatives used during tester development, plus sample tree/stack/list methods. It is not uploaded to a VPL question; students submit their own source under the filename/class configuration expected by `CreateTester()`.

## `Upload_V2_Net472`

This directory is the VPL upload staging area. It contains common infrastructure files and the task-specific C# files. The three common files are reused for every V2 task.

### `tester_payload.tar`

The payload is a tar archive of the runtime framework required in VPL. Its current contents are:

```text
PETEL_Runner_V2.exe
PETEL_Runner_V2.exe.config
PETEL_V2_Core.dll
```

The payload must contain the runner, core library, and every required runtime dependency for the built version of the tester. In environments where Roslyn or other dependencies are not installed by the VPL host, those DLLs must be supplied by the payload or copied from the server location used by `vpl_run.sh`. Rebuild/repack the payload whenever the runner/core or their deployment dependencies change.

### `vpl_evaluate.sh`

This is the Moodle VPL evaluation entry point. It copies `vpl_run.sh` to `vpl_evaluate2.sh` and executes that copy. The indirection is VPL-oriented; the compilation/execution work is in `vpl_run.sh`.

### `vpl_run.sh`

This script runs in the VPL Moodle sandbox.

1. Loads VPL's `common_script.sh`, verifies `mono` and a C# compiler (`csc` or `mcs`), and collects all submitted/teacher `.cs` source files.
2. Extracts `tester_payload.tar` into the working directory. It sets `MONO_PATH` so Mono can resolve the runner and core DLL from that extracted directory.
3. Optionally copies Roslyn/support DLLs from `/usr/lib/DLL` and adds that directory to `MONO_PATH`.
4. Locates `netstandard.dll` and the legacy NUnit DLL when available, then compiles all VPL C# sources with the extracted DLLs as references.
5. If compilation succeeds, writes VPL's `vpl_execution` script, which runs `output.exe` under Mono (or NUnit if detected).
6. `output.exe` is built from the task's `Program.cs`, `TeacherAnswer.cs`, `TestCases.cs`, and the student's submitted `.cs` file(s). The executable starts `MainTesterHost`, which invokes the extracted runner for each functional case.

The actual VPL upload additionally needs the task-specific `Program.cs`, `TeacherAnswer.cs`, and `TestCases.cs`. Do not upload the local `StudentAnswer.cs`.
