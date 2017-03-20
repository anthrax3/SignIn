@ECHO OFF

:: Set up the env to use Msbuild 14.0
CALL "%VS140COMNTOOLS%\vsvars32.bat"

:: Try to restore packages. This is only needed when the solution depends on NuGet packages
PUSHD %~dp0\tools
WHERE nuget.exe >nul 2>nul
IF %ERRORLEVEL% EQU 0 nuget.exe restore ..
IF NOT EXIST "..\packages\" (ECHO Error: Get nuget.exe or build the sln in VS to restore the packages && EXIT /B 1)
POPD

PUSHD %~dp0
msbuild
POPD
