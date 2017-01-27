@ECHO OFF

:: Set up the env to use Msbuild 14.0
IF "%VS140COMNTOOLS%" EQU "" (
    SET "ERRORMSG=Msbuild 14.0 seems to be not installed)"
    SET ERRORLEVEL=2
    GOTO ERROR
)
CALL "%VS140COMNTOOLS%\vsvars32.bat"

PUSHD %~dp0
msbuild
POPD
GOTO END

:ERROR
ECHO %ERRORMSG%
EXIT /B %ERRORLEVEL%

:END
