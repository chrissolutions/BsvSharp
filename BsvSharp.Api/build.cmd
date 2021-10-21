@echo off
setlocal
if '%root%' == '' set root=..\..

:: Type
set type=BsvSharp.Api
set location=Enterprise\%type%
set mapi=CafeLib.BsvSharp.Mapi

:: Settings
call %root%\build\buildenv %*
if ERRORLEVEL 1 goto error
set solution=CafeLib.%type%
set sourcepath=%root%\%location%

:: Setup libraries.
set libs=%solution%.CoinGecko
set libs=%libs% %solution%.CoinMarketCap
set libs=%libs% %solution%.Paymail
set libs=%libs% %solution%.WhatsOnChain
set libs=%libs% %mapi%
set libs=%libs% %mapi%.MatterPool
::

:: Run script to build the libraries
call %root%\build\buildlibs
if ERRORLEVEL 1 goto error

:exit
endlocal
exit /b 0

:error
endlocal
exit /b 1
