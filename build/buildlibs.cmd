@echo off

echo Build %solution% ...
for %%X in (%libs%) do (
    echo %msbld% %sourcepath%\%%X\%%X.csproj -p:Version=%version% -p:PackageVersion=%version% -p:Configuration=%configuration%
    %msbld% %sourcepath%\%%X\%%X.csproj -p:Version=%version% -p:PackageVersion=%version% -p:Configuration=%configuration%
    if ERRORLEVEL 1 goto error
)

echo Create Nuget Packages for %solution% ...
for %%X in (%libs%) do (
    echo %pack% %sourcepath%\%%X\%%X.csproj -p:Version=%version% -p:PackageVersion=%version% -p:Configuration=%configuration%
    %pack% %sourcepath%\%%X\%%X.csproj -p:Version=%version% -p:PackageVersion=%version% -p:Configuration=%configuration%
    if ERRORLEVEL 1 goto error
)

echo Push Packages to Nuget repository ...
for %%X in (%libs%) do (
    echo %nuget% push %sourcepath%\%%X\%libPath%\%%X.%version%.nupkg %apiswitch% -s %nugetServer% %skipdup%
    if '%debug%' == '' %nuget% push %sourcepath%\%%X\%libPath%\%%X.%version%.nupkg %apiswitch% -s %nugetServer% %skipdup%
    if ERRORLEVEL 1 goto error
)

exit /b 0

:error
exit /b 1
