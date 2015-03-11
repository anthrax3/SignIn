@echo off

call "%~dp0\build.bat" || (GOTO ERROR)
call "%~dp0\package.bat" || (GOTO ERROR)

GOTO :EOF

:ERROR

GOTO :EOF




