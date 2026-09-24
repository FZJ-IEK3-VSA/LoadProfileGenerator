REM Builds a new release version of the LoadProfileGenerator for all target platforms, and creates new python bindings

REM use the repository root as working directory
set "srcdirectory=%~dp0"
cd /D %srcdirectory%

REM read the .NET version from Directory.Build.props
for /f "usebackq delims=" %%v in (`dotnet msbuild ReleaseMaker -getProperty:LpgDotnetVersion`) do set "dotnetversion=%%v"
if not defined dotnetversion goto :error
echo Using .NET version %dotnetversion%

REM increment the LPG build number
REM dotnet build VersionIncreaser --configuration Debug
REM VersionIncreaser\bin\Debug\%dotnetversion%\versionincreaser.exe


REM clear bin directories
rmdir /S /Q SimulationEngine\bin
rmdir /S /Q LoadProfileGenerator\bin
rmdir /S /Q SimEngine2\bin

REM build simengine2 for Linux and Windows, and LoadProfileGenerator for Windows
dotnet publish LoadProfileGenerator --configuration Release --no-self-contained --verbosity quiet -p:DebugType=none || goto :error
dotnet publish SimulationEngine     --configuration Release --no-self-contained --verbosity quiet -p:DebugType=none --runtime win-x64 || goto :error
dotnet publish SimEngine2           --configuration Release --self-contained    --verbosity quiet -p:DebugType=none --runtime win-x64 || goto :error
dotnet publish SimEngine2           --configuration Release --self-contained    --verbosity quiet -p:DebugType=none --runtime linux-x64 || goto :error
pause

REM run release checks and collect release files
dotnet build ReleaseMaker --configuration Debug -t:rebuild  -v:m || goto :error
cd /D %srcdirectory%\ReleaseMaker\bin\Debug\%dotnetversion%-windows
releasemaker || goto :error
pause


REM create new python bindings for the pylpg
set "releasedirectory=%srcdirectory%\LPGRelease\release_directories"
set "pylpgdirectory=%srcdirectory%\LPGRelease\pylpg\"

cd /D %releasedirectory%\windows
simulationengine CreatePythonBindings || goto :error
move lpgdata.py %pylpgdirectory%
move lpgpythonbindings.py %pylpgdirectory%
pause
exit /b 0

REM error handling
:error
echo.
echo *** RELEASE FAILED (exit code %errorlevel%) ***
pause
exit /b 1