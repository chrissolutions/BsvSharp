@echo off
set root=%~d0%~p0
set root=%root:\build\=%

:: verify environment
if not '%configuration%' == '' goto exit
if not '%msbld%' == '' goto exit
if not '%pack%' == '' goto exit
if not '%nuget%' == '' goto exit

:: Settings
set apikey=
set apiswitch=
set version=
set debug=
set configuration=Debug
set msbld=dotnet build
set msbuild=msbuild.exe
set pack=dotnet pack
set nuget=dotnet nuget
set nugetpack=nuget pack

:: Parse arguments
if '%1' == '' goto usage
:nextarg
set arg=%1
if '%arg%' == '' goto start
if '%arg%' == '-v' set version=%2&&shift&&shift&&goto nextarg
if '%arg%' == '/v' set version=%2&&shift&&shift&&goto nextarg
if '%arg%' == '-c' set configuration=%2&&shift&&shift&&goto nextarg
if '%arg%' == '/c' set configuration=%1&&shift&&shift&&goto nextarg
if '%arg%' == '-k' set apikey=%2&&shift&&shift&&goto nextarg
if '%arg%' == '/k' set apikey=%2&&shift&&shift&&goto nextarg
if '%arg%' == '-s' set nugetServer=%2&&shift&&shift&&goto nextarg
if '%arg%' == '/s' set nugetServer=%2&&shift&&shift&&goto nextarg
if '%arg%' == '-s' set nugetServer=%2&&shift&&shift&&goto nextarg
if '%arg%' == '/s' set nugetServer=%2&&shift&&shift&&goto nextarg
if '%arg%' == '-d' set debug=true&&shift&&goto nextarg
if '%arg%' == '/d' set debug=true&&shift&&goto nextarg
goto usage

:start
if '%version%' == '' goto usage
if not '%configuration%' == 'Debug' if not '%configuration%' == 'Release' goto usage
if not '%apikey%' == '' set apiswitch=-k %apikey%
if '%configuration%' == '' set configuration=Debug
if '%nugetServer%' == '' set nugetServer=C:\Nuget\repo
set libPath=bin\%configuration%
goto exit

:usage
echo build -v ^<version number^> [-c ^<configuration^> Debug is default] [-k ^<apikey^>] [-s ^<nugetServer^> C:\Nuget\repo is default]
goto error

:exit
exit /b 0

:error
exit /b 1
