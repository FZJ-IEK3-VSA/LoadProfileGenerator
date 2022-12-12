REM get the path to the current directory
set "srcdirectory=%~dp0"
set "vsdirectory=D:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin"

REM copy V:\Dropbox\LPG\WpfApplication1\profilegenerator-latest.db3 c:\Main\Git-Repositories\LoadProfileGenerator\WpfApplication1\profilegenerator-latest.db3

cd /D %srcdirectory%
%srcdirectory%\VersionIncreaser\bin\Debug\versionincreaser.exe

cd /D %srcdirectory%\SimulationEngine
rmdir /S /Q %srcdirectory%\SimulationEngine\bin
"%vsdirectory%\msbuild.exe" SimulationEngine.csproj -t:rebuild -v:m

cd /D %srcdirectory%\WpfApplication1
rmdir /S/Q %srcdirectory%\WpfApplication1\bin
"%vsdirectory%\msbuild.exe" LoadProfileGenerator.csproj -t:rebuild  -v:m

cd /D %srcdirectory%\SimEngine2
rmdir /S /Q %srcdirectory%\SimEngine2\bin
dotnet publish simengine2.csproj --configuration Release --self-contained true --runtime win10-x64 --verbosity quiet -f net6.0-windows
dotnet publish simengine2.csproj --configuration Release --self-contained true --runtime linux-x64 --verbosity quiet -f net6.0

cd /D %srcdirectory%\ReleaseMaker
"%vsdirectory%\msbuild.exe" ReleaseMaker.csproj -t:rebuild  -v:m

cd /D %srcdirectory%\ReleaseMaker\bin\Debug\net6.0-windows
releasemaker
pause


REM create pylpg
set "releasedirectory=C:\LPGReleaseMakerResults\LPGReleases\releases10.9.0"
set "pylpgdirectory=C:\LPGPythonBindings\pylpg\"

cd /D %releasedirectory%\net48
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