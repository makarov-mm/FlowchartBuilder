@echo off
setlocal

rem Builds FlowchartBuilder in Release (output: ..\Build\Release) and, if Inno Setup is installed,
rem packs it into an installer (output: Output\FlowchartBuilderSetup-<version>.exe).
rem Set ISCC to the full path of ISCC.exe to use a specific Inno Setup compiler.

pushd "%~dp0"

dotnet build ..\FlowchartBuilder.slnx -c Release
if errorlevel 1 goto failed

if not defined ISCC for %%I in (ISCC.exe) do set "ISCC=%%~$PATH:I"
if not defined ISCC call :find "%ProgramFiles%\Inno Setup 7"
if not defined ISCC call :find "%ProgramFiles(x86)%\Inno Setup 7"
if not defined ISCC call :find "%LocalAppData%\Programs\Inno Setup 7"
if not defined ISCC call :find "%ProgramFiles(x86)%\Inno Setup 6"
if not defined ISCC call :find "%LocalAppData%\Programs\Inno Setup 6"

if not defined ISCC (
    echo Inno Setup was not found, so no installer was made. The application is in Build\Release.
    goto done
)

"%ISCC%" FlowchartBuilder.iss
if errorlevel 1 goto failed

:done
popd
exit /b 0

:failed
popd
exit /b 1

:find
if exist "%~1\ISCC.exe" set "ISCC=%~1\ISCC.exe"
exit /b 0
