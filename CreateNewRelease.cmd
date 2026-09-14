REM Builds a new release version of the LoadProfileGenerator for all target platforms, and creates new python bindings

REM get the path to the current directory
set "srcdirectory=%~dp0"
set "dotnetversion=net10.0"

REM increment the LPG build number
REM dotnet build VersionIncreaser --configuration Debug
REM VersionIncreaser\bin\Debug\%dotnetversion%\versionincreaser.exe


REM clear bin directories
rmdir /S /Q SimulationEngine\bin
rmdir /S /Q WpfApplication1\bin
rmdir /S /Q SimEngine2\bin

REM build simengine2 for Linux and Windows, and LoadProfileGenerator for Windows
dotnet publish WpfApplication1  --configuration Release --no-self-contained --verbosity quiet -p:DebugType=none
dotnet publish SimulationEngine --configuration Release --no-self-contained --verbosity quiet -p:DebugType=none --runtime win-x64
dotnet publish simengine2       --configuration Release --self-contained    --verbosity quiet -p:DebugType=none --runtime win-x64
dotnet publish simengine2       --configuration Release --self-contained    --verbosity quiet -p:DebugType=none --runtime linux-x64
pause

REM run release checks and collect release files
dotnet build ReleaseMaker --configuration Debug -t:rebuild  -v:m
cd /D %srcdirectory%\ReleaseMaker\bin\Debug\%dotnetversion%-windows
releasemaker
pause


REM create new python bindings for the pylpg
set "releasedirectory=%srcdirectory%\LPGRelease\release_directories"
set "pylpgdirectory=%srcdirectory%\LPGRelease\pylpg\"

cd /D %releasedirectory%\windows
simulationengine CreatePythonBindings
xcopy lpgdata.py %pylpgdirectory%
xcopy lpgpythonbindings.py %pylpgdirectory%
pause
