# PETEL V2 (MainTester + Runner) – Repository Guide

This document explains the **V2 architecture** (Main process orchestrator + isolated Runner process) and describes the **purpose of each V2 project and file**.

> V2 goal: keep the **original PETEL behavior**: run the **student solution** and **teacher reference solution** with the same inputs, then **compare return value / stdout / parameter side-effects**.
>
> V2 additionally improves robustness by running each functional test in an **isolated Runner process** (so one bad student run won’t break the whole grading run).

---

## What teachers write vs what students submit (IMPORTANT)

### Student submission (unchanged)
- **Student uploads/submits:** `StudentAnswer.cs`
- Contains the student implementation (usually `class StudentAnswer` + the required method).

### Teacher reference solution (unchanged)
- **Teacher provides (uploaded with the tester):** `TeacherAnswer.cs`
- Contains the correct reference implementation (usually `class TeacherAnswer` + the same required method).

---

## Single-file teacher workflow (simple)

Teachers edit **only one file**:

### `TestCases.cs`
`TestCases.cs` contains two things:
1. **Assignment config** (which student/teacher class/method to compare)
2. **The tests themselves** (calls to `tester.TestMethod(...)` / `tester.TestCodeStructure(...)`)

This replaces the old workflow where teachers edited `MainTester.cs` constructor arguments.

---

## High-level flow

1. Teacher uploads/provides:
   - `TeacherAnswer.cs` (reference solution)
   - `TestCases.cs` (assignment config + test definitions)
2. Student submits:
   - `StudentAnswer.cs`
3. `PETEL_MainTester_V2` runs:
   - discovers test methods in `PETEL_VPL.TestCases`
   - runs `Code_...` tests in-process (optional convention)
   - runs all other tests in the isolated Runner process by `--method <name>`
4. Inside each test method, `VPLTester`:
   - invokes **StudentAnswer** and **TeacherAnswer** using the method/class names configured in `TestCases.cs`
   - compares results (return/stdout/params)

---

## V2 Projects

### `PETEL_MainTester_V2` (net8.0 console)
**Role:** orchestrator/aggregator.

Responsibilities:
- Discover teacher test methods via reflection.
- Run `Code_` tests in-process.
- Spawn `Runner` for functional tests.
- Parse Runner output packets.
- Print aggregated formatted output and final grade.


### `PETEL_Runner_V2` (net8.0 console)
**Role:** isolated executor for one teacher test method.

Responsibilities:
- Parse `--method <MethodName>`.
- Locate and invoke that method in `TestCases`.
- Suppress `Console.Out` while running.
- Emit exactly one protocol line: `PETEL_V2|<points>|<base64(text)>`.


### `PETEL_V2_Core` (net8.0 class library)
**Role:** shared library used by both MainTester and Runner.

Key files:
- `PETEL_V2_Core/VPLTester.cs`
  - Same comparison engine as original PETEL.

---

## How to build and run (local development)

### Build everything
From repository root:
- `dotnet build`

### Run MainTester (will spawn Runner)
From repository root:
- `dotnet run --project PETEL_MainTester_V2`

### Run Runner manually (single test method)
From repository root:
- `dotnet run --project PETEL_Runner_V2 -- --method <MethodName>`

---

## Notes for VPL / deployment

- Student submits `StudentAnswer.cs`.
- Teacher uploads `TeacherAnswer.cs` and `TestCases.cs` (plus the V2 tester binaries/scripts).
- MainTester is the entry point; it spawns Runner as needed.
