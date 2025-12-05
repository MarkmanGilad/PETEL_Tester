#!/bin/bash
# VPL runner for C# with a precompiled tester library (Tester.exe)
# - Tester.exe.b64 = Base64 of your .NET Framework 4.7.2 tester library (no Main)
# - MainTester.cs + StudentAnswer.cs + TeacherAnswer.cs are compiled into output.exe
# - output.exe references Tester.exe and is run with Mono

# Load common VPL helpers
. common_script.sh

# We only need mono and a C# compiler (mcs or csc)
check_program mono
check_program mcs csc

# Decode a single Base64 artifact and abort with a clear message on failure
decode_b64_or_exit() {
    local src="$1"
    local dest="$2"
    local label="$3"

    # Some VPL uploads may include CERTUTIL headers; strip them before decoding
    local cleaned=$(mktemp)
    sed '/^-----BEGIN/d;/^-----END/d;/^$/d' "$src" | tr -d '\r' > "$cleaned"

    base64 --ignore-garbage -d "$cleaned" > "$dest" 2> decode_error.log
    rm -f "$cleaned"
    if [ -s decode_error.log ]; then
        echo "#!/bin/bash" > vpl_execution
        echo "echo \"### ERROR: Failed to decode $label ($src)\"" >> vpl_execution
        echo "echo \"### decode_error.log:\"" >> vpl_execution
        echo 'cat decode_error.log' >> vpl_execution
        chmod +x vpl_execution
        exit 1
    fi

    rm -f decode_error.log
}

# --- Handle VPL "version" query ---------------------------------------------
if [ "$1" == "version" ] ; then
    echo "#!/bin/bash" > vpl_execution
    echo 'echo "C# (.NET Framework 4.7.2) via Mono, using Tester.exe.b64 as tester library"' >> vpl_execution
    chmod +x vpl_execution
    exit 0
fi

# --- Ensure tester payload exists -------------------------------------------
if [ ! -f Tester.exe.b64 ]; then
    echo "#!/bin/bash" > vpl_execution
    echo 'echo "### ERROR: Tester.exe.b64 not found in VPL directory!"' >> vpl_execution
    echo 'echo "### Files in directory:"' >> vpl_execution
    echo 'ls -la' >> vpl_execution
    chmod +x vpl_execution
    exit 1
fi

# --- Decode tester payload -> Tester.exe ------------------------------------
decode_b64_or_exit "Tester.exe.b64" "Tester.exe" "Tester.exe"
chmod +x Tester.exe

# --- Decode every bundled .dll.b64 dependency --------------------------------
for DLL_B64 in *.dll.b64; do
    [ -e "$DLL_B64" ] || break
    DLL_NAME="${DLL_B64%.b64}"
    decode_b64_or_exit "$DLL_B64" "$DLL_NAME" "$DLL_NAME"
done

# --- Collect C# source files -------------------------------------------------
CS_FILES=$(ls *.cs 2>/dev/null)

if [ -z "$CS_FILES" ]; then
    echo "#!/bin/bash" > vpl_execution
    echo 'echo "### ERROR: No .cs files found (MainTester.cs / StudentAnswer.cs / TeacherAnswer.cs missing?)"' >> vpl_execution
    chmod +x vpl_execution
    exit 1
fi

# --- Choose compiler: prefer mcs, fallback to csc ---------------------------
if command -v mcs >/dev/null 2>&1 ; then
    CSCOMPILER="mcs"
elif command -v csc >/dev/null 2>&1 ; then
    CSCOMPILER="csc"
else
    echo "#!/bin/bash" > vpl_execution
    echo 'echo "### ERROR: No C# compiler (mcs/csc) found on this server"' >> vpl_execution
    chmod +x vpl_execution
    exit 1
fi

# --- Compile MainTester + student code, referencing Tester.exe ---------------
# -out:output.exe : compiled program with Main in MainTester.cs
# -r:Tester.exe   : reference to your tester library (no Main)
$CSCOMPILER -out:output.exe -r:Tester.exe $CS_FILES > compile.out 2>&1
COMPILE_STATUS=$?

if [ $COMPILE_STATUS -ne 0 ] ; then
    # Compilation failed: create execution script that just prints compiler output
    echo "#!/bin/bash" > vpl_execution
    echo 'echo "### COMPILATION ERROR ###"' >> vpl_execution
    echo 'cat compile.out' >> vpl_execution
    chmod +x vpl_execution
    exit 0
fi

# --- Compilation OK: create execution script to run the compiled program ----
echo "#!/bin/bash" > vpl_execution
echo 'mono output.exe "$@"' >> vpl_execution
chmod +x vpl_execution
