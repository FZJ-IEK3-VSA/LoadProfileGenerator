REM Builds a new release version of the LoadProfileGenerator for all target platforms, and creates new python bindings

REM get the path to the current directory
set "srcdirectory=%~dp0"

cd /D %srcdirectory%
%srcdirectory%\VersionIncreaser\bin\Debug\versionincreaser.exe

cd /D %srcdirectory%\SimulationEngine
rmdir /S /Q %srcdirectory%\SimulationEngine\bin
dotnet build --configuration Release SimulationEngine.csproj -t:rebuild -v:m

cd /D %srcdirectory%\WpfApplication1
rmdir /S/Q %srcdirectory%\WpfApplication1\bin
dotnet build --configuration Release LoadProfileGenerator.csproj -t:rebuild  -v:m

cd /D %srcdirectory%\SimEngine2
rmdir /S /Q %srcdirectory%\SimEngine2\bin
dotnet publish simengine2.csproj --configuration Release --self-contained true --runtime win-x64 --verbosity quiet -f net10.0
dotnet publish simengine2.csproj --configuration Release --self-contained true --runtime linux-x64 --verbosity quiet -f net10.0
pause

cd /D %srcdirectory%\ReleaseMaker
dotnet build ReleaseMaker.csproj -t:rebuild  -v:m

cd /D %srcdirectory%\ReleaseMaker\bin\Debug\net10.0-windows
releasemaker
pause


REM create new python bindings for the pylpg
set "releasedirectory=C:\LPGReleaseMakerResults\LPGReleases\releases10.10"
set "pylpgdirectory=C:\LPGPythonBindings\pylpg\"

cd /D %releasedirectory%\windows
simulationengine cpy
xcopy lpgdata.py %pylpgdirectory%
xcopy lpgpythonbindings.py %pylpgdirectory%
pause