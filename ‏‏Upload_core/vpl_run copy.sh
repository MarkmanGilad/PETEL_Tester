#!/bin/bash
# VPL runner for C# with a precompiled tester library (Tester.exe)
# - Tester.exe.b64 = Base64 of your .NET Framework 4.7.2 tester library (no Main)
# - MainTester.cs + StudentAnswer.cs + TeacherAnswer.cs are compiled into output.exe
# - output.exe references Tester.exe and is run with Mono

# Load common VPL helpers
. common_script.sh

# We require the .NET SDK/runtime (dotnet) for .NET Core / .NET 8
check_program dotnet

# Optional: prefer dotnet runtime when available (for .NET Core / .NET 8)
# Don't `check_program` because it's optional; only detect at runtime.
DOTNET_AVAILABLE=0
if command -v dotnet >/dev/null 2>&1 ; then
    DOTNET_AVAILABLE=1
fi

# Pull student-submitted C# files from the Moodle sandbox
get_source_files cs
generate_file_of_files .vpl_source_files

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
    echo 'echo "C# (.NET 8.0) via dotnet SDK (no .NET Framework)"' >> vpl_execution
    chmod +x vpl_execution
    exit 0
fi

# --- No .NET Framework tester: skip Tester.exe handling ---------------------
# This VPL variant targets .NET Core / .NET 8 only. Do not install or expect
# Tester.exe.b64 (old .NET Framework payload).

# --- Decode every bundled .dll.b64 dependency --------------------------------
for DLL_B64 in *.dll.b64; do
    [ -e "$DLL_B64" ] || break
    DLL_NAME="${DLL_B64%.b64}"
    decode_b64_or_exit "$DLL_B64" "$DLL_NAME" "$DLL_NAME"
done

# --- Configure .NET 8.0 locations (if admin provided them) ------------------
# If the host placed .NET 8 under /usr/lib/NET 8.0, prefer those. Also ensure
# the `dotnet` command is available (required by this script).
NET8_NETCORE_DIR="/usr/lib/NET 8.0/NetCore"
NET8_ASPNETCORE_DIR="/usr/lib/NET 8.0/AspNetCore"

# If a custom dotnet layout exists there, add it to DOTNET_ROOT and PATH so
# `dotnet` can find runtimes/sdks. This is safe even when system dotnet exists.
if [ -d "$NET8_NETCORE_DIR" ]; then
    export DOTNET_ROOT="$NET8_NETCORE_DIR"
    export PATH="$NET8_NETCORE_DIR:$PATH"
fi
if [ -d "$NET8_ASPNETCORE_DIR" ]; then
    export DOTNET_ROOT="${DOTNET_ROOT:-}$NET8_ASPNETCORE_DIR"
    export PATH="$NET8_ASPNETCORE_DIR:$PATH"
fi

# --- Collect C# source files -------------------------------------------------
if [ ! -s .vpl_source_files ]; then
    echo "#!/bin/bash" > vpl_execution
    echo 'echo "### ERROR: No .cs files found (MainTester.cs / StudentAnswer.cs / TeacherAnswer.cs missing?)"' >> vpl_execution
    chmod +x vpl_execution
    exit 1
fi
cp .vpl_source_files vpl_sources.list

# --- Build with `dotnet` using a temporary SDK project ---------------------
# Create a temporary project targeting net8.0, copy student sources, publish.
TMP_PROJ_DIR="vpl_dotnet_proj"
PUBLISH_OUT="vpl_dotnet_publish"
rm -rf "$TMP_PROJ_DIR" "$PUBLISH_OUT"
mkdir -p "$TMP_PROJ_DIR"

# Minimal SDK project file
cat > "$TMP_PROJ_DIR/vpl.csproj" <<'XML'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>
    <AssemblyName>output</AssemblyName>
  </PropertyGroup>
</Project>
XML

# Copy student sources into project
while IFS= read -r src; do
    cp "$src" "$TMP_PROJ_DIR/"
done < .vpl_source_files

# Run dotnet publish (framework-dependent) -> produces output.dll to run with `dotnet`
pushd "$TMP_PROJ_DIR" >/dev/null 2>&1
dotnet publish -c Release -o ../"$PUBLISH_OUT" > ../compile.out 2>&1
COMPILE_STATUS=$?
popd >/dev/null 2>&1

if [ $COMPILE_STATUS -ne 0 ] ; then
    echo "#!/bin/bash" > vpl_execution
    echo 'echo "### COMPILATION ERROR ###"' >> vpl_execution
    echo 'cat compile.out' >> vpl_execution
    chmod +x vpl_execution
    exit 0
fi

# --- Compilation OK: create execution script to run the compiled program ----
echo "#!/bin/bash" > vpl_execution
echo 'dotnet "$(pwd)/vpl_dotnet_publish/output.dll" "$@"' >> vpl_execution
chmod +x vpl_execution
