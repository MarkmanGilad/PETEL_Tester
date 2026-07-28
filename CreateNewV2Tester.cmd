@echo off
setlocal
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0CreateNewV2Tester.ps1"
set exitCode=%ERRORLEVEL%
echo.
if not "%exitCode%"=="0" echo The tester project was not created. See the message above.
pause
exit /b %exitCode%
