@echo off

if "%1"=="handshake"  goto HANDSHAKE

if "%1"=="-h"         goto HELP
if "%1"=="/h"         goto HELP
if "%1"=="--help"     goto HELP

goto HELP

:HANDSHAKE
  call tdcr start --port 8555 --rpc-port 8554
  call tdcr start --port 8666 --rpc-port 8665

  call tdcr exec --rpc-port 8554 ConnectTo 127.0.0.1 8666

  call tdcr stop --rpc-port 8554
  call tdcr stop --rpc-port 8665
goto END

:HELP
  echo TDCR experiments
  echo Usage: exp [command^|experiment]
  echo.

  echo Commands:
  echo   /h^|-h^|--help    Display this help message.
  echo.

  echo Experiments:
  echo   handshake       Start two daemons and force them to perform a handshake.
goto END

:END
