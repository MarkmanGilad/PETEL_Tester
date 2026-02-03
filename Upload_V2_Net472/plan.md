# VPL Upload Plan (net472) — `tester_payload.tar` + Main Tester Sources

This document summarizes the purpose of each uploaded file and the execution plan on Moodle VPL (running on .NET Framework 4.7.2 via Mono on Linux).

## Upload Set (what will be present in the VPL sandbox)

### A) `tester_payload.tar`
**Purpose:** A single tar archive containing the *precompiled* tester runtime binaries, so the VPL compile step only compiles the assignment-specific tester sources and student code.

**Contents (minimum):**
- `PETEL_Runner_V2.exe`  
  Runner entry point. Executes a specific test method and prints a single protocol line to stdout (`PETEL_V2|...`).
- `PETEL_Runner_V2.exe.config`  
  Runtime config (binding redirects / config). Keep with the exe.
- `PETEL_V2_Core.dll`  
  Shared core logic used by the runner/tester (grading utilities, protocol helpers, etc.).
- Any required dependency `.dll` files not guaranteed on the VPL host (optional but recommended to avoid missing-assembly errors).

**Notes:**
- `*.pdb` are not needed for VPL execution.
- The archive is unpacked at runtime by `vpl_run.sh`.

---

### B) Main tester files (compiled on VPL)
These are the assignment-specific files that define which tests run and how grading is computed.

- `TestCases.cs`  
  **Purpose:** Defines the public static test methods to run (e.g., `public static void Q1(VPLTester t)`), usually under a known type name (e.g., `PETEL_VPL.TestCases_*`).  
  The runner invokes one method specified by `--method`.

- `Program.cs`  
  **Purpose:** Entry point for the “main tester host” layer on VPL. Selects the correct `TestCases` type name and delegates execution to the runner/host mechanism.

- `TeacherAnswer.cs`  
  **Purpose:** Reference/expected implementation used to compute expected results or to compare with `StudentAnswer` in tests. Not submitted by students; uploaded by the question author.

> Student submission typically provides `StudentAnswer.cs` (not part of the upload set above but expected from the VPL “required files” configuration).

---

### C) VPL scripts

- `vpl_run.sh`  
  **Purpose:** The main VPL execution script. It must:
  1. Prepare dependencies (unpack `tester_payload.tar`, ensure dlls are discoverable).
  2. Collect student `.cs` files from the sandbox.
  3. Compile `Program.cs` + `TestCases.cs` + `TeacherAnswer.cs` + student files into `output.exe` (net472 on Mono).
  4. Run `output.exe` with `mono` and emit the grade/output in the expected format.

- `vpl_evaluate.sh`  
  **Purpose:** Wrapper that triggers `vpl_run.sh` in “evaluate” mode (see `Upload_Dafna` behavior).

## Execution Model Note: Runner Runs in a Separate Process
The system is designed so the *runner* (`PETEL_Runner_V2.exe`) executes in a **separate OS process** from the main tester/host process. Practically this means:

- The main tester/host compiles and orchestrates the test run, but the actual test execution happens inside the runner process.
- If the runner (child process) crashes early, it may produce **no output**, and the main tester/host (parent process) may appear to “hang” until a **timeout** occurs.
- Therefore, correctness depends on:
  - shipping all runtime dependencies next to the runner and/or setting `MONO_PATH`,
  - ensuring the runner can load `TestCases`/student assemblies at runtime.

## Execution Plan on VPL (unpack → compile → run)

### Step 1 — Unpack payload
`vpl_run.sh` should:
- Create a directory (or use current directory) for runtime binaries.
- Unpack the payload:
  - `tar -xf tester_payload.tar`
- Ensure the runner/core dlls end up next to the compiled `output.exe` *or* set:
  - `MONO_PATH="$(pwd):$MONO_PATH"`

### Step 2 — Ensure required DLLs are available
Following the `Upload_Dafna` pattern:
- Optionally copy preinstalled DLLs from `/usr/lib/DLL` if present (Roslyn/netstandard-related, etc.).
- Prefer having all required dlls either:
  - unpacked from `tester_payload.tar`, or
  - copied from `/usr/lib/DLL`.

### Step 3 — Compile the “main tester” + student submission
`vpl_run.sh` should:
- Use `common_script.sh` + `get_source_files cs` + `generate_file_of_files .vpl_source_files`
- Compile into: `output.exe`
- Reference local dependencies as needed (at least unpacked core/runner assemblies and any other required dlls).

### Step 4 — Run the compiled tester
`vpl_run.sh` should:
- Execute `mono output.exe`
- The compiled program should:
  - start orchestration,
  - spawn/invoke the runner in a separate process,
  - run the correct `TestCases` method(s),
  - print the final grade/output (PETEL packet format) to stdout.

### Step 5 — Evaluate wrapper
`vpl_evaluate.sh` should:
- Invoke `vpl_run.sh` (or copy it like `Upload_Dafna` does) and run it.

## Expected Behavior / Contract
- All runtime artifacts needed to execute the tests must be available after unpacking `tester_payload.tar` (or copied from the server DLL folder).
- Output must be compatible with the PETEL protocol line (`PETEL_V2|points|base64(payload)`).

## Reference
- Use `Upload_Dafna/vpl_run.sh` as the baseline script structure (tool checks, source collection, DLL copying, compilation, mono execution).