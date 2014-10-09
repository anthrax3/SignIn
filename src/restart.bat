@echo off
staradmin kill all

REM Start Launcher
cd c:\github\Polyjuice\Launcher
call run.bat

REM Start Sign-In 
cd c:\github\Polyjuice\SignInApp\src
call run.bat