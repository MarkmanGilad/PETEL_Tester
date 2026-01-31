#!/bin/sh

set -eu

# VPL runner for the PETEL .NET 8 tester.
# Teacher uploads:
# - tester_payload.tar.gz  (recommended: self-contained linux-x64 publish packed as tar.gz)
# - vpl_run.sh (and optionally vpl_evaluate.sh)
# - TeacherAnswer.cs + TestCases.cs
# Student submits their .cs file(s) via VPL.

VPL_EXEC_READY=0

write_vpl_placeholder() {
    {
        echo "#!/bin/sh"
        echo "echo \"### vpl_run.sh started but did not finish.\""
        echo "echo \"### If you see this, the runner crashed early.\""
    } > vpl_execution
    chmod +x vpl_execution
}

write_vpl_exec() {
    line="$1"
    {
        echo "#!/bin/sh"
        echo "set -eu"
        echo "$line"
    } > vpl_execution
    chmod +x vpl_execution
    VPL_EXEC_READY=1
}

write_vpl_error() {
    msg="$1"
    {
        echo "#!/bin/sh"
        echo "echo \"$msg\""
        echo "echo \"### Files in directory:\""
        echo "ls -la"
    } > vpl_execution
    chmod +x vpl_execution
    VPL_EXEC_READY=1
}

on_exit() {
    status=$?
    if [ "$status" -ne 0 ]; then
        if [ "${VPL_EXEC_READY:-0}" -eq 0 ]; then
            write_vpl_error "### ERROR: vpl_run.sh failed (exit $status)"
        fi
    fi
}

write_vpl_placeholder
trap on_exit EXIT

# NOTE: We intentionally do not source common_script.sh.
# Some VPL images ship a helper that exits early, which prevents vpl_execution
# from being generated. This runner is self-contained.

require_program() {
    prog="$1"
    if command -v "$prog" >/dev/null 2>&1; then
        return 0
    fi
    return 1
}

if [ "${1:-}" = "version" ]; then
    write_vpl_exec 'echo "C# (.NET 8) via dotnet; runs PETEL_MainTester_V2 from tester_payload.tar.gz"'
    exit 0
fi

PAYLOAD="${PAYLOAD:-}"
RUNTIME_DIR="${RUNTIME_DIR:-.petel_runtime}"

if [ -z "$PAYLOAD" ]; then
    if [ -f "tester_payload.tar" ]; then
        PAYLOAD="tester_payload.tar"
    elif [ -f "tester_payload.tar.gz" ]; then
        PAYLOAD="tester_payload.tar.gz"
    elif [ -f "tester_payload.zip" ]; then
        PAYLOAD="tester_payload.zip"
    else
        PAYLOAD="tester_payload.tar"
    fi
fi

# Some environments set TAR_OPTIONS=--gzip (or similar), causing tar to always
# pipe through gzip even when not requested.
unset TAR_OPTIONS

# Check for payload + tar first
if [ ! -f "$PAYLOAD" ]; then
    write_vpl_error "### ERROR: $PAYLOAD not found"
    exit 1
fi

if ! require_program tar; then
    write_vpl_error "### ERROR: tar not found (needed to extract $PAYLOAD)"
    exit 1
fi

# Extract payload FRESHLY so we can inspect it for self-contained vs framework-dependent
rm -rf "$RUNTIME_DIR"
mkdir -p "$RUNTIME_DIR"
magic_hex=$(dd if="$PAYLOAD" bs=1 count=2 2>/dev/null | od -An -tx1 2>/dev/null | tr -d ' \n')
magic4=$(dd if="$PAYLOAD" bs=1 count=4 2>/dev/null | od -An -tx1 2>/dev/null | tr -d ' \n')

if [ "$magic4" = "504b0304" ]; then
    # ZIP archive
    if command -v unzip >/dev/null 2>&1; then
        if ! unzip -q "$PAYLOAD" -d "$RUNTIME_DIR" 2>/tmp/unzip_error.log; then
            write_vpl_error "### ERROR: Failed to extract ZIP payload with unzip"
            if [ -f /tmp/unzip_error.log ]; then
                echo "### unzip error:" >&2
                cat /tmp/unzip_error.log >&2
            fi
            exit 1
        fi
    elif command -v busybox >/dev/null 2>&1; then
        if ! busybox unzip -q "$PAYLOAD" -d "$RUNTIME_DIR" 2>/tmp/unzip_error.log; then
            write_vpl_error "### ERROR: Failed to extract ZIP payload with busybox unzip"
            if [ -f /tmp/unzip_error.log ]; then
                echo "### busybox unzip error:" >&2
                cat /tmp/unzip_error.log >&2
            fi
            exit 1
        fi
    elif command -v python3 >/dev/null 2>&1; then
        python3 - <<'PY'
import zipfile, sys
zip_path = sys.argv[1]
out_dir = sys.argv[2]
with zipfile.ZipFile(zip_path) as zf:
    zf.extractall(out_dir)
PY
        if [ $? -ne 0 ]; then
            write_vpl_error "### ERROR: Failed to extract ZIP payload with python3"
            exit 1
        fi
    else
        write_vpl_error "### ERROR: ZIP payload detected but no unzip/busybox/python3 available"
        exit 1
    fi
elif [ "$magic_hex" = "1f8b" ]; then
    # gzip tar
    if ! tar -xzf "$PAYLOAD" -C "$RUNTIME_DIR" 2>/tmp/tar_error.log; then
        write_vpl_error "### ERROR: Failed to extract gzip tarball $PAYLOAD"
        if [ -f /tmp/tar_error.log ]; then
            echo "### tar -xzf error:" >&2
            cat /tmp/tar_error.log >&2
        fi
        exit 1
    fi
else
    # not gzip: try to extract after renaming to .tar (avoids extension-based auto-compress)
    PAYLOAD_TAR="$RUNTIME_DIR/_payload.tar"
    cp "$PAYLOAD" "$PAYLOAD_TAR" 2>/dev/null || PAYLOAD_TAR="$PAYLOAD"
    if tar --no-auto-compress -xf "$PAYLOAD_TAR" -C "$RUNTIME_DIR" 2>/tmp/tar_error2.log; then
        :
    elif tar -xf "$PAYLOAD_TAR" -C "$RUNTIME_DIR" 2>/tmp/tar_error3.log; then
        :
    else
        size_bytes=$(wc -c < "$PAYLOAD" 2>/dev/null || echo "unknown")
        head_hex=$(dd if="$PAYLOAD" bs=1 count=16 2>/dev/null | od -An -tx1 2>/dev/null | tr -d ' \n')
        write_vpl_error "### ERROR: Failed to extract payload. Not a valid tar. Size=$size_bytes bytes, head=$head_hex"
        if [ -f /tmp/tar_error2.log ]; then
            echo "### tar --no-auto-compress -xf error:" >&2
            cat /tmp/tar_error2.log >&2
        fi
        if [ -f /tmp/tar_error3.log ]; then
            echo "### tar -xf error:" >&2
            cat /tmp/tar_error3.log >&2
        fi
        exit 1
    fi
fi

MAIN_EXECUTABLE=""
if [ -f "$RUNTIME_DIR/PETEL_MainTester_V2" ]; then
    MAIN_EXECUTABLE="$RUNTIME_DIR/PETEL_MainTester_V2"
elif [ -f "$RUNTIME_DIR/main/PETEL_MainTester_V2" ]; then
    MAIN_EXECUTABLE="$RUNTIME_DIR/main/PETEL_MainTester_V2"
fi

RUNNER_EXECUTABLE=""
if [ -f "$RUNTIME_DIR/PETEL_Runner_V2" ]; then
    RUNNER_EXECUTABLE="$RUNTIME_DIR/PETEL_Runner_V2"
elif [ -f "$RUNTIME_DIR/runner/PETEL_Runner_V2" ]; then
    RUNNER_EXECUTABLE="$RUNTIME_DIR/runner/PETEL_Runner_V2"
fi

PAYLOAD_IS_SELF_CONTAINED=0
if [ -n "$MAIN_EXECUTABLE" ]; then
    PAYLOAD_IS_SELF_CONTAINED=1
fi

DOTNET=""
# Only search for system dotnet if we DON'T have a self-contained executable
if [ "$PAYLOAD_IS_SELF_CONTAINED" -eq 0 ]; then
    if require_program dotnet; then
        DOTNET="dotnet"
    else
        # Some VPL images provide dotnet via vpl_environment.sh
        if [ -f ./vpl_environment.sh ]; then
            set +e
            . ./vpl_environment.sh >/dev/null 2>&1
            set -e
        fi
        if require_program dotnet; then
            DOTNET="dotnet"
        else
            if [ -n "${DOTNET_ROOT:-}" ] && [ -x "$DOTNET_ROOT/dotnet" ]; then
                DOTNET="$DOTNET_ROOT/dotnet"
            fi
        fi
        if [ -z "$DOTNET" ]; then
            for cand in \
                /usr/bin/dotnet \
                /usr/local/bin/dotnet \
                /usr/share/dotnet/dotnet \
                /usr/lib/dotnet/dotnet \
                /usr/lib/NET8.0/dotnet \
                /usr/lib/NET8.0/NetCore/dotnet \
                /usr/lib/NET8.0/AspNetCore/dotnet \
                "$HOME/.dotnet/dotnet"
            do
                if [ -x "$cand" ]; then
                    DOTNET="$cand"
                    break
                fi
            done
            
            # Last ditch search: look everywhere (excluding virtual fs)
            if [ -z "$DOTNET" ]; then
                 FOUND=$(find / -path /proc -prune -o -path /sys -prune -o -path /dev -prune -o -name dotnet -type f -perm /111 -print 2>/dev/null | head -n 1)
                 if [ -n "$FOUND" ]; then
                     DOTNET="$FOUND"
                 fi
            fi
        fi
    fi

    if [ -z "$DOTNET" ]; then
        # Gather debug info for the error message
        DEBUG_MSG="Search for 'dotnet' failed in PATH and common locations."
        if [ -d "/usr/lib/NET8.0" ]; then
            DEBUG_MSG="$DEBUG_MSG /usr/lib/NET8.0 exists."
        fi
        
        write_vpl_error "### ERROR: 'dotnet' executable not found. logic requires .NET 8 SDK/Runtime properly installed with 'dotnet' on PATH or in standard locations. 
        Suggestion: Ask the VPL administrator for the path to 'dotnet', or republish your tester as Self-Contained (linux-x64) so it doesn't need a pre-installed runtime.
        Debug: $DEBUG_MSG"
        exit 1
    fi
fi

# If the VPL image has these DLLs preinstalled, copy them next to our app so
# the deps.json resolution can find them.
SERVER_DLL_DIRS="/usr/lib/DLL /usr/local/lib/DLL /usr/lib/NET8.0/NetCore /usr/lib/NET8.0/AspNetCore /usr/lib/NET8.0"

copy_server_dll_if_missing() {
    dll_name="$1"
    dest="$RUNTIME_DIR/$dll_name"
    if [ -f "$dest" ]; then
        return 0
    fi
    for d in $SERVER_DLL_DIRS; do
        if [ -f "$d/$dll_name" ]; then
            cp "$d/$dll_name" "$dest"
            return 0
        fi
    done
    return 0
}

for REQUIRED_DLL in \
    Microsoft.CodeAnalysis.dll \
    Microsoft.CodeAnalysis.CSharp.dll \
    System.Collections.Immutable.dll \
    System.Reflection.Metadata.dll
do
    copy_server_dll_if_missing "$REQUIRED_DLL"
done

# Basic teacher/student file sanity (the tester itself may handle discovery too)
if [ ! -f TeacherAnswer.cs ] || [ ! -f TestCases.cs ]; then
    write_vpl_error "### ERROR: TeacherAnswer.cs and/or TestCases.cs not found (teacher files)"
    exit 1
fi

found_student=0
for f in ./*.cs; do
    [ -f "$f" ] || continue
    base=$(basename "$f")
    if [ "$base" != "TeacherAnswer.cs" ] && [ "$base" != "TestCases.cs" ]; then
        found_student=1
        break
    fi
done
if [ "$found_student" -eq 0 ]; then
    write_vpl_error "### ERROR: No student .cs file found (expected at least one *.cs besides TeacherAnswer.cs/TestCases.cs)"
    exit 1
fi

# Run from the submission working directory so relative file lookups resolve.
if [ "$PAYLOAD_IS_SELF_CONTAINED" -eq 1 ]; then
    chmod +x "$MAIN_EXECUTABLE" 2>/dev/null || true
    if [ -n "$RUNNER_EXECUTABLE" ]; then
        chmod +x "$RUNNER_EXECUTABLE" 2>/dev/null || true
    fi

    # If the tester internally tries to launch `dotnet <Runner>.dll`, provide a shim.
    if ! require_program dotnet; then
        DOTNET_SHIM="$RUNTIME_DIR/dotnet"
        {
            echo "#!/bin/sh"
            echo "set -eu"
            echo "arg1=\"\${1:-}\""
            echo "shift || true"
            echo "consider=\"\${arg1##*/}\""
            echo "base=\"\${consider%.dll}\""
            echo "if [ -x \"$RUNTIME_DIR/\$base\" ]; then exec \"$RUNTIME_DIR/\$base\" \"\$@\"; fi"
            echo "if [ -x \"$RUNTIME_DIR/runner/\$base\" ]; then exec \"$RUNTIME_DIR/runner/\$base\" \"\$@\"; fi"
            echo "echo \"### ERROR: dotnet shim could not run \$arg1\""
            echo "exit 127"
        } > "$DOTNET_SHIM"
        chmod +x "$DOTNET_SHIM" 2>/dev/null || true
        PATH="$RUNTIME_DIR:$RUNTIME_DIR/main:$RUNTIME_DIR/runner:$PATH"
        export PATH
    fi

    write_vpl_exec "\"$MAIN_EXECUTABLE\" \"\$@\""
else
    # Framework-dependent payload (requires dotnet)
    MAIN_DLL="$RUNTIME_DIR/PETEL_MainTester_V2.dll"
    if [ ! -f "$MAIN_DLL" ]; then
        MAIN_DLL="$RUNTIME_DIR/main/PETEL_MainTester_V2.dll"
    fi
    if [ ! -f "$MAIN_DLL" ]; then
        write_vpl_error "### ERROR: PETEL_MainTester_V2.dll not found inside payload (root or main/)"
        exit 1
    fi
    write_vpl_exec "\"$DOTNET\" \"$MAIN_DLL\" \"\$@\""
fi
