REM Builds a new release version of the LoadProfileGenerator for all target platforms, and creates new python bindings

REM get the path to the current directory
set "srcdirectory=%~dp0"

REM increment the LPG build number
dotnet build VersionIncreaser
VersionIncreaser\bin\Debug\net10.0\versionincreaser.exe

REM clear bin directories
rmdir /S /Q SimulationEngine\bin
rmdir /S /Q WpfApplication1\bin
rmdir /S /Q SimEngine2\bin

REM build simengine2 for Linux and Windows, and LoadProfileGenerator for Windows
dotnet publish WpfApplication1\LoadProfileGenerator.csproj --configuration Release
dotnet publish simengine2 --configuration Release --self-contained true --runtime win-x64 --verbosity quiet
dotnet publish simengine2 --configuration Release --self-contained true --runtime linux-x64 --verbosity quiet
pause

REM run release checks
dotnet build ReleaseMaker -t:rebuild  -v:m
ReleaseMaker\bin\Debug\net10.0-windows\releasemaker
pause


REM create new python bindings for the pylpg
set "releasedirectory=C:\LPGReleaseMakerResults\LPGReleases\releases10.10"
set "pylpgdirectory=C:\LPGPythonBindings\pylpg\"

cd /D %releasedirectory%\windows
simulationengine cpy
xcopy lpgdata.py %pylpgdirectory%
xcopy lpgpythonbindings.py %pylpgdirectory%
pause