@echo off
staradmin kill all

REM Start Launcher
call "%~dp0..\..\Launcher\run.bat"

call "%~dp0run.bat"