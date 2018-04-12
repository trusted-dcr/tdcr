@echo off

set config=
set args=

if "%1"=="-r"        goto RELEASE
if "%1"=="/r"        goto RELEASE
if "%1"=="--release" goto RELEASE
goto DEBUG

:DEBUG
set config=Debug
set args=%*
goto RUN

:RELEASE
for /f "tokens=1,* delims= " %%a in ("%*") do set ALL_BUT_FIRST=%%b
set config=Release
set args=%ALL_BUT_FIRST%
goto RUN

:RUN
@echo on
dotnet run --no-build --configuration %config% --project TDCR.Console\TDCR.Console.csproj -- %args%
@echo off
goto END

:END
