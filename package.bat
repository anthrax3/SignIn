@echo off
SETLOCAL

REM Create empty folder where will will create the file structure to be packed (zipped)
IF EXIST "%~dp0temp\NUL" (
	rd "%~dp0temp" /S /Q
) else (
 md "%~dp0temp"
)

REM Prepare Executables
md "%~dp0temp\app"
xcopy "%~dp0src\bin\Debug\*.*" "%~dp0temp\app"

REM Prepare wwwroot
md "%~dp0temp\wwwroot"
xcopy "%~dp0src\wwwroot" "%~dp0temp\wwwroot" /s /e

REM Copy icon and config
xcopy "%~dp0src\package\*.png" "%~dp0temp"
xcopy "%~dp0src\package\*.config" "%~dp0temp"

REM Get folder name for the zip name
for %%a in ("%~dp0.") do set currentfolder=%%~na


IF NOT EXIST "%~dp0dist" (
 md "%~dp0dist"
)

REM Zipp-it
CD "%~dp0temp"

zip -r "..\dist\%currentfolder%" *.*  || (GOTO ERROR)

REM TODO Jump back to previous dir
GOTO :CLEANUP

:ERROR

:CLEANUP
CD "%~dp0"
rd "%~dp0temp" /S /Q

:END

ENDLOCAL
