@echo off
setlocal

set nugetKey=--nugetKey=oy2bgspluqupwcctw2madun54rkybgpevnulxcku2z2rzi
set nugetServer=https://api.nuget.org/v3/index.json
rem set nugetServer=C:/Nuget/repo
set nugetDebug=--nugetDebug=true
set nugetSkipDup=--nugetSkipDup=true

dotnet cake --build=Publish --config=Release %nugetKey% --nugetServer=%nugetServer% %nugetDebug% %nugetSkipDup%
if ERRORLEVEL 1 goto error

endlocal
exit /b 0

:error
endlocal
exit /b 1
