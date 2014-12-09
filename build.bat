@echo off

REM Echo Getting Source from GIT
rem call git --git-dir="%~dp0.git" pull || (GOTO ERROR)

REM call git --git-dir="%~dp0.git" --work-tree="%~dp0" pull && (echo success) || (GOTO ERROR)
SET FOUND_SOURCE=0
REM Echo Building Source
FOR %%i IN (%~dp0src\*.csproj) DO ( 
SET FOUND_SOURCE=1
REM msbuild %~dp0\src\AppStoreClient.csproj && (GOTO ERROR)
msbuild %%i || (GOTO ERROR)
)

IF "%FOUND_SOURCE%"=="0" ( echo No source found, check batch argument
GOTO ERROR  )

GOTO END

:ERROR
echo Error building!

:END


