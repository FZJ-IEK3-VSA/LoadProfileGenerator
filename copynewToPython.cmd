set "srcdirectory=C:\Main\Git-Repositories\LoadProfileGenerator"
set "vsdirectory=D:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin"

REM copy V:\Dropbox\LPG\WpfApplication1\profilegenerator-latest.db3 c:\Main\Git-Repositories\LoadProfileGenerator\WpfApplication1\profilegenerator-latest.db3

cd %srcdirectory%
%srcdirectory%\VersionIncreaser\bin\Debug\versionincreaser.exe

cd %srcdirectory%\SimulationEngine
rmdir /S /Q %srcdirectory%\SimulationEngine\bin
"%vsdirectory%\msbuild.exe" SimulationEngine.csproj -t:rebuild -v:m

cd %srcdirectory%\WpfApplication1
rmdir /S/Q %srcdirectory%\WpfApplication1\bin
"%vsdirectory%\msbuild.exe" LoadProfileGenerator.csproj -t:rebuild  -v:m

cd %srcdirectory%\SimEngine2
rmdir /S /Q %srcdirectory%\SimEngine2\bin
dotnet publish simengine2.csproj --configuration Release --self-contained true --runtime win10-x64 --verbosity quiet -f net5.0-windows
dotnet publish simengine2.csproj --configuration Release --self-contained true --runtime linux-x64 --verbosity quiet -f net5.0

cd %srcdirectory%\ReleaseMaker
"%vsdirectory%\msbuild.exe" ReleaseMaker.csproj -t:rebuild  -v:m

cd %srcdirectory%\ReleaseMaker\bin\Debug\net5.0-windows
releasemaker
pause


REM create pylpg
set "releasedirectory=C:\LPGReleaseMakerResults\LPGReleases\releases10.8.0"
set "pylpgdirectory=C:\LPGPythonBindings\pylpg\"

cd %releasedirectory%\net48
simulationengine cpy
xcopy lpgdata.py %pylpgdirectory%
xcopy lpgpythonbindings.py %pylpgdirectory%

robocopy %releasedirectory%\net48 %pylpgdirectory%\LPG_win /E /R:0 /W:0 /MIR
robocopy %releasedirectory%\net48 C:\LPGPythonBindings\pylpg_2\LPG_win_most_recent /E /R:0 /W:0 /MIR
del %pylpgdirectory%\LPG_win\LPG*.zip
del %pylpgdirectory%\LPG_win\setup*.exe
robocopy %releasedirectory%\linux %pylpgdirectory%\LPG_linux /E /R:0 /W:0 /MIR
del %pylpgdirectory%\LPG_linux\LPG*.zip
pause