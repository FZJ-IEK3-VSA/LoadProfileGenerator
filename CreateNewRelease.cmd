REM Builds a new release version of the LoadProfileGenerator for all target platforms, and creates new python bindings

REM get the path to the current directory
set "srcdirectory=%~dp0"
set "dotnetversion=net10.0"

REM increment the LPG build number
REM dotnet build VersionIncreaser --configuration Debug
REM VersionIncreaser\bin\Debug\%dotnetversion%\versionincreaser.exe

REM clear bin directories
REM rmdir /S /Q SimulationEngine\bin
REM rmdir /S /Q WpfApplication1\bin
REM rmdir /S /Q SimEngine2\bin

REM build simengine2 for Linux and Windows, and LoadProfileGenerator for Windows
REM dotnet publish WpfApplication1\LoadProfileGenerator.csproj --configuration Release --verbosity quiet -p:DebugType=none
REM dotnet publish SimulationEngine --configuration Release --self-contained true --runtime win-x64 --verbosity quiet -p:DebugType=none
REM dotnet publish simengine2 --configuration Release --self-contained true --runtime win-x64 --verbosity quiet -p:DebugType=none
REM dotnet publish simengine2 --configuration Release --self-contained true --runtime linux-x64 --verbosity quiet -p:DebugType=none
REM pause

REM run release checks and collect release files
REM dotnet build ReleaseMaker --configuration Debug -t:rebuild  -v:m
REM cd /D %srcdirectory%\ReleaseMaker\bin\Debug\%dotnetversion%-windows
REM releasemaker
REM pause


REM create new python bindings for the pylpg
set "releasedirectory=%srcdirectory%\LPGReleases\releases10.10"
set "pylpgdirectory=%srcdirectory%\LPGReleases\pylpg\"

cd /D %releasedirectory%\windows
simulationengine CreatePythonBindings
xcopy lpgdata.py %pylpgdirectory%
xcopy lpgpythonbindings.py %pylpgdirectory%
pause
