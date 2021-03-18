
@ECHO OFF

SET /P CustomerCode="Customer code: "

if not exist "C:\QA\" mkdir C:\QA
if not exist "C:\QA\Logs" mkdir C:\QA\Logs
SET ThisScriptsDirectory=%~dp0
SET PowerShellScriptPath=%ThisScriptsDirectory%\Install\Install.ps1

PowerShell -NoProfile -ExecutionPolicy Bypass -Command "& {Start-Process PowerShell -ArgumentList '-NoProfile -ExecutionPolicy Bypass -File """"%PowerShellScriptPath%"""" -customerCode """"%CustomerCode%"""" -installRoot """"C:\QA"""" -logPath """"C:\QA\Logs"""" ' -Verb RunAs}";
