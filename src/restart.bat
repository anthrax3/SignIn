@echo off
staradmin kill all

REM Start Launcher
call ..\..\Polyjuice\Launcher\run.bat

call %~dp0run.bat