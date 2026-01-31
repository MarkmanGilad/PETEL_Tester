@echo off
setlocal
cd /d "%~dp0"

echo Packing payload from current folder files only...

if exist tester_payload.tar del /f /q tester_payload.tar
tar -cf tester_payload.tar PETEL_MainTester_V2.dll PETEL_MainTester_V2.deps.json PETEL_MainTester_V2.runtimeconfig.json PETEL_Runner_V2.dll PETEL_Runner_V2.deps.json PETEL_Runner_V2.runtimeconfig.json PETEL_V2_Core.dll
if errorlevel 1 (
  echo Failed to create tester_payload.tar
  exit /b 1
)

echo Created tester_payload.tar
