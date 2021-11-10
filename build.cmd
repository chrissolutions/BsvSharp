@echo off
setlocal
if '%root%' == '' set root=.

:: Settings
call %root%\build\buildenv %*
if ERRORLEVEL 1 goto exit

:: Setup domains.
set libs=
set libs=%libs% Core
set libs=%libs% Cryptography
set libs=%libs% Data
set libs=%libs% Authorization
set libs=%libs% Network
set libs=%libs% Blazor
set libs=%libs% Mobile
set libs=%libs% Enterprise\BsvSharp
set libs=%libs% Enterprise\BsvSharp.Api

for %%X in (%libs%) do (
    set rc=0
    pushd %%X
    cd
    echo build.cmd -v %version% -c %configuration% %apiswitch% -s %nugetServer%
    call build.cmd -v %version% -c %configuration% %apiswitch% -s %nugetServer%
    if ERRORLEVEL 1 set rc=1
    popd
    if rc == 1 goto error
)

endlocal
exit /b 0

:error
endlocal
exit /b 1
