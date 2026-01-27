#!/bin/bash
# This file is part of VPL for Moodle - http://vpl.dis.ulpgc.es/
# Script for running C# language
# Copyright (C) 2019 Juan Carlos Rodríguez-del-Pino
# License http://www.gnu.org/copyleft/gpl.html GNU GPL v3 or later
# Author Juan Carlos Rodríguez-del-Pino <jcrodriguez@dis.ulpgc.es>

# @vpl_script_description Using csc or mcs
# load common script and check programs
. common_script.sh
check_program mono
check_program csc mcs
if [ "$1" == "version" ] ; then
	echo "#!/bin/bash" > vpl_execution
	echo "$PROGRAM --version" >> vpl_execution
	chmod +x vpl_execution
	exit
fi 
[ "$PROGRAM" == "mcs" ] && export PKGDOTNET="-pkg:dotnet"
get_source_files cs
OUTPUTFILE=output.exe

# --- Pull preinstalled DLLs from the VPL host if available --------------------
SERVER_DLL_DIR="/usr/lib/DLL"
copy_server_dll_if_missing() {
	local dll_name="$1"
	if [ ! -f "$dll_name" ] && [ -f "$SERVER_DLL_DIR/$dll_name" ]; then
		cp "$SERVER_DLL_DIR/$dll_name" "$dll_name"
	fi
}

if [ -d "$SERVER_DLL_DIR" ]; then
	for REQUIRED_DLL in \
		Microsoft.CodeAnalysis.dll \
		Microsoft.CodeAnalysis.CSharp.dll \
		System.Buffers.dll \
		System.Collections.Immutable.dll \
		System.Memory.dll \
		System.Numerics.Vectors.dll \
		System.Reflection.Metadata.dll \
		System.Runtime.CompilerServices.Unsafe.dll \
		System.Threading.Tasks.Extensions.dll
	do
		copy_server_dll_if_missing "$REQUIRED_DLL"
	done
	export MONO_PATH="$(pwd):$SERVER_DLL_DIR:${MONO_PATH:-}"
fi

# Force using the old system NUnit (2.6.3) if available.
NUNITLIBFILE="/usr/lib/cli/nunit.framework-2.6.3/nunit.framework.dll"
if [ ! -f "$NUNITLIBFILE" ]; then
	NUNITLIBFILE=$(ls /usr/lib/cli/nunit.framework*/nunit.framework.dll 2>/dev/null | tail -n 1)
fi
if [ -f "$NUNITLIBFILE" ]; then
	export NUNITLIB="-r:$NUNITLIBFILE"
fi

# Generate file with source files
generate_file_of_files .vpl_source_files
# Detect NUnit handled above (forced to old system NUnit when available)

if [ -f "./Microsoft.CodeAnalysis.dll" ]; then
	export MICROSOFTLIB="-r:$(pwd)/Microsoft.CodeAnalysis.dll"
fi
if [ -f "./Microsoft.CodeAnalysis.CSharp.dll" ]; then
	export MICROSOFTLIB1="-r:$(pwd)/Microsoft.CodeAnalysis.CSharp.dll"
fi

# Roslyn targets netstandard2.0; on Mono we must reference netstandard.dll.
NETSTANDARD_FACADE=""
for CANDIDATE in \
	/usr/lib/mono/4.5/netstandard.dll \
	/usr/lib/mono/4.5/Facades/netstandard.dll \
	/usr/lib/mono/4.6.1-api/netstandard.dll \
	/usr/lib/mono/4.6.1-api/Facades/netstandard.dll \
	/usr/lib/mono/4.7.1-api/netstandard.dll \
	/usr/lib/mono/4.7.1-api/Facades/netstandard.dll \
	/usr/lib/mono/4.7.2-api/netstandard.dll \
	/usr/lib/mono/4.7.2-api/Facades/netstandard.dll \
	/usr/lib/mono/4.8-api/netstandard.dll \
	/usr/lib/mono/4.8-api/Facades/netstandard.dll
do
	if [ -f "$CANDIDATE" ]; then
		NETSTANDARD_FACADE="$CANDIDATE"
		break
	fi
done
if [ "$NETSTANDARD_FACADE" != "" ]; then
	export NETSTANDARDLIB="-r:$NETSTANDARD_FACADE"
fi


# Compile
export MONO_ENV_OPTIONS=--gc=sgen
EXECUTABLE=false
$PROGRAM $PKGDOTNET $NUNITLIB $NETSTANDARDLIB $MICROSOFTLIB $MICROSOFTLIB1 $CSharp_DLL_REFERENCE -out:$OUTPUTFILE -lib:/usr/lib/mono @.vpl_source_files &>.vpl_compilation_message
if [ -f $OUTPUTFILE ] ; then
	EXECUTABLE=true
else
	# Try to compile as dll
	OUTPUTFILE=output.dll
	if [ "$NUNITLIB" != "" ] ; then
		$PROGRAM $PKGDOTNET $NUNITLIB $NETSTANDARDLIB $MICROSOFTLIB $MICROSOFTLIB1 -out:$OUTPUTFILE -target:library -lib:/usr/lib/mono @.vpl_source_files &> /dev/null
	fi
fi
rm .vpl_source_files
if [ -f $OUTPUTFILE ] ; then
	cat common_script.sh > vpl_execution
	chmod +x vpl_execution
	echo "export MONO_ENV_OPTIONS=--gc=sgen" >> vpl_execution
	# Detect NUnit
	grep -E "nunit\.framework" $OUTPUTFILE &>/dev/null
	if [ "$?" -eq "0" ]	; then
		echo "nunit-console -nologo -trace=off $OUTPUTFILE" >> vpl_execution
	fi
	if [ "$EXECUTABLE" == "true" ] ; then
		echo "mono $OUTPUTFILE \$@" >> vpl_execution
		grep -E "System\.Windows\.Forms" $OUTPUTFILE &>/dev/null
		if [ "$?" -eq "0" ]	; then
			mv vpl_execution vpl_wexecution
		fi
	fi
else
	cat .vpl_compilation_message
fi