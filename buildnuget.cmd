@echo off
setlocal

rem set nugetKey=--nugetKey=oy2didvqmsgou2rjhdxamlnijbmytzlefgg4jmyd453iwa
rem set nugetServer=https://api.nuget.org/v3/index.json
set nugetServer=C:/Nuget/repo
rem set nugetDebug=--nugetDebug=true
rem set nugetSkipDup=--nugetSkipDup=true

dotnet cake --build=Publish --config=Release %nugetKey% --nugetServer=%nugetServer% %nugetDebug% %nugetSkipDup%
if ERRORLEVEL 1 goto error

endlocal
exit /b 0

:error
endlocal
exit /b 1
