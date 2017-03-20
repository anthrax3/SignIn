@ECHO OFF

IF "%CONFIGURATION%"=="" SET CONFIGURATION=Debug

star --resourcedir="%~dp0src\SignIn\wwwroot" "%~dp0bin/%CONFIGURATION%/SignIn.exe"
