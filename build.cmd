@echo off
setlocal
if '%root%' == '' set root=.

:: Settings
call %root%\build\buildenv %*
if ERRORLEVEL 1 goto exit

:: Setup domains.
set libs=
set libs=%libs% BsvSharp
set libs=%libs% BsvSharp.Api

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
