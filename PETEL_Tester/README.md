Below are the **updated V2 development steps** using the **best selector**: **Main passes the test method name to Runner** (not an index/number). Teachers stay completely unaware of processes and don’t need to manage ordering for correctness.

---

# PETEL V2 App Development Steps (method-name selection)

## Design summary

* Teacher writes tests only in `TestCases.cs`, using your familiar `tester.TestMethod(...)` and `tester.TestCodeStructure(...)`.
* **MainTester** discovers all teacher test methods and runs each functional test in a **separate Runner process**, passing `--method <name>`.
* **Runner** finds that method by reflection, runs it, outputs one result packet.
* Main aggregates points and prints Moodle/VPL output.
* Ordering is handled in Main (optional numbering for display only).

---

# Phase 1 — Create projects/files

## 1) Create two console executables

1. **PETEL_MainTester_V2** → produces `MainTester.exe`
2. **PETEL_Runner_V2** → produces `Runner.exe`

> This keeps things clean and easy to compile in VPL and in Visual Studio.

## 2) Create one teacher-edited file

* `TestCases.cs` (included in both MainTester and Runner builds)

## 3) Create a shared “PETEL core” folder used by both projects

Copy/reuse from your current PETEL repository:

* `VPLTester.cs` (core engine: comparison, formatting, exception handling)
* `ObjectComparer.cs`
* `ObjectCloning.cs`
* `Unit4Helper.cs`
* Snapshot helpers (Node/Queue/Stack/BinNode snapshot code you already wrote)
* Any other utilities your VPLTester needs

MainTester-only (optional, but recommended):

* CodeAnalyzer classes/files

> Use the same namespace in both projects (e.g., `PETEL_VPL`) to avoid type resolution issues.

---

# Phase 2 — Define the teacher-facing conventions (keep it simple)

## 4) Teacher writes only `TestCases` methods

In `TestCases.cs`, teacher writes:

* `public static void <AnyName>(VPLTester tester)`

Inside each method they call:

* `tester.TestMethod(...)` for functional checks (student execution)
* `tester.TestCodeStructure(...)` for code checks (analysis)

**No process knowledge. No flags.**

## 5) Enforce a strict signature to avoid overload ambiguity

Framework rule:

* Only methods with signature `(VPLTester tester)` are considered “tests”
* Return type must be `void`
* Must be `public static`
* No overloads allowed for the same name with same signature (C# already prevents exact duplicates)

This guarantees that passing `--method Name` maps to exactly one method.

---

# Phase 3 — Decide what runs in separate process (automatic)

## 6) Automatic isolation rule (teacher unaware)

You want teachers not to care about processes, so enforce:

* **All calls to `tester.TestMethod(...)` run in Runner (isolated).**
* **All calls to `tester.TestCodeStructure(...)` run in Main (in-process).**

How to implement this cleanly:

* Runner is used to execute **test methods** that contain student execution.
* Code-structure tests are executed directly in Main by invoking test methods that only do code checks, OR by executing code-check sections in Main.

Simplest teacher convention:

* Teachers put functional tests in methods named `Case_...`
* Teachers put code tests in methods named `Code_...`

But if you don’t want naming rules, you can also:

* Execute *all* test methods in runner and just disable analyzer in runner mode (still works, just slower).
* Recommended hybrid:

  * Main runs `Code_` tests in-process
  * Runner runs all other tests

(Teacher still doesn’t need to know “process”, it’s just naming for organization.)

---

# Phase 4 — MainTester: discover methods and orchestrate

## 7) Discover teacher test methods

MainTester uses reflection to:

1. Find type `PETEL_VPL.TestCases`
2. Select methods:

   * public static
   * return void
   * parameters == (VPLTester)
3. Determine execution type:

   * if method name starts with `Code_` → in-process
   * else → runner

## 8) Determine output order (display only)

Because we’re selecting by method name, ordering is now purely a UI concern.

Choose one of these:

* **Sort by method name** (simple, deterministic)
* **Sort by explicit teacher number** (optional):

  * teacher calls `tester.SetTestNumber(n)` at top of each method, or you use `[Order(n)]`
  * Main sorts by `(number, methodName)`
* **Keep reflection discovery order** as fallback (not guaranteed, but harmless now)

Important: order does NOT affect correctness anymore (runner selection is by name).

## 9) Run tests

MainTester creates a “main aggregation” structure:

* totalGrade
* list of formatted test outputs (strings)

For each test method:

* If Code test:

  * invoke method in-process with a `VPLTester` configured for code analysis
  * append its formatted output
  * add points
* Else (functional test):

  * spawn Runner process with `--method <MethodName>`
  * parse its result packet
  * append output + points
  * handle crash/timeout as 0 points failure message

At the end:

* print combined test outputs
* print `Grade :=>> X`

---

# Phase 5 — Runner: run one method, return one packet

## 10) Runner command line

Runner accepts:

* `--method <MethodName>`
* optional: `--timeout <ms>` (or Runner uses baked-in timeout)

## 11) Runner logic

Runner does:

1. Find `TestCases` type
2. Find the method by name and signature `(VPLTester)`
3. Create a `VPLTester` instance (same config as your existing usage)
4. Globally suppress output while running:

   * redirect `Console.Out` to a buffer so student prints cannot corrupt protocol
5. Invoke the teacher test method (it will call `tester.TestMethod(...)` etc.)
6. Restore output
7. Print exactly one protocol line:

   * `PETEL_V2|<points>|<base64(formattedText)>`
8. Exit

Crash handling:

* If stack overflow happens, Runner may die → Main sees “no protocol” and reports a crash.

---

# Phase 6 — Make minimal changes to reuse your existing `VPLTester`

## 12) VPLTester should support “student method switching”

You requested this earlier: teacher can test multiple student methods.
Steps:

* Add a public setter like `SetStudentMethod(string name)` (and optionally class/ns setters).
* Teacher can call it inside a test method before `TestMethod(...)`.

## 13) Prevent runner noise

* Ensure `InitializeCodeAnalyzer()` does not print to stdout in Runner (either skip analyzer in runner or route logs to a file only in debug mode).
* Keep `TestMethod` output only via your existing formatted result blocks.

---

# Phase 7 — VPL build integration

## 14) VPL compile

Your `vpl_run.sh` compiles two exes:

* `MainTester.exe`
* `Runner.exe`

Then runs:

* `mono MainTester.exe`

Ensure Runner.exe is in same directory (or provide full path when launching).

---

# Phase 8 — Teacher example (the only code you need to show teachers)

```csharp
using System;
using Unit4;
using C = System.Collections.Generic;

namespace PETEL_VPL
{
    public static class TestCases
    {
        // Functional test (will run in Runner automatically)
        public static void Case_Test1_ReturnValue(VPLTester tester)
        {
            // Optional: test another student method
            // tester.SetStudentMethod("countRemoveItem");

            var comments = new C.Dictionary<Type, string>
            {
                { typeof(NullReferenceException), "You advanced past the end of the list (node became null)." },
                { typeof(InvalidOperationException), "You used Pop/Peek/Dequeue on an empty stack/queue." }
            };

            Queue<int> q = Unit4Helper.BuildQueue(new int[] { 3, 5, -9, 3, 5, 5, 2, 1, 2 });

            tester.TestMethod(
                testName: "Return value is correct",
                points: 10,
                parameters: new object[] { q, 5 },
                compareParams: false,
                exceptionComments: comments
            );
        }

        // Functional test (Runner)
        public static void Case_Test2_NoMutation(VPLTester tester)
        {
            Queue<int> q = Unit4Helper.BuildQueue(new int[] { 1, 4, 4, 1, 5, -9, -9, 1, -9 });

            tester.TestMethod(
                testName: "Queue is not modified",
                points: 10,
                parameters: new object[] { q, 1 },
                compareParams: true
            );
        }

        // Code test (Main process, no Runner)
        public static void Code_NoNestedLoops(VPLTester tester)
        {
            tester.InitializeCodeAnalyzer();

            tester.TestCodeStructure(
                testName: "No nested loops (O(n))",
                points: 10,
                checkType: CodeStructureCheck.HasNestedLoops,
                shouldPass: false,
                failureMessage: "Must not have nested loops"
            );
        }
    }
}
```

---

If you want, I can also specify (still without code) **exactly how MainTester should locate Runner.exe on Windows vs VPL/Mono**, and what environment differences to handle (paths, mono command, timeouts, stderr).
