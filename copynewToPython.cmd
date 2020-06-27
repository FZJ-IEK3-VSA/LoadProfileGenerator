copy V:\Dropbox\LPG\WpfApplication1\profilegenerator-latest.db3 c:\work\LPGdev\WpfApplication1\profilegenerator-latest.db3
cd C:\Work\LPGDev\SimulationEngine
rmdir /S /Q C:\Work\LPGDev\SimulationEngine\bin
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe" SimulationEngine.csproj -t:rebuild -v:m
cd C:\Work\LPGDev\WpfApplication1
rmdir /S/Q C:\Work\LPGDev\WpfApplication1\bin
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe" LoadProfileGenerator.csproj -t:rebuild  -v:m
cd C:\Work\LPGDev\SimEngine2
rmdir /S /Q C:\Work\LPGDev\SimEngine2\bin
dotnet publish simengine2.csproj --configuration Release --self-contained true --runtime win10-x64 --verbosity quiet
dotnet publish simengine2.csproj --configuration Release --self-contained true --runtime linux-x64 --verbosity quiet

cd C:\Work\LPGDev\ReleaseMaker
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe" ReleaseMaker.csproj -t:rebuild  -v:m

cd C:\Work\LPGDev\ReleaseMaker\bin\Debug
releasemaker

v:
cd V:\Dropbox\LPGReleases\releases9.9.0\net48
simulationengine cpy
copy lpgdata.py c:\work\fzj\pylpg\
copy lpgpythonbindings.py c:\work\fzj\pylpg\
c:
cd \work\lpgdev
robocopy V:\Dropbox\LPGReleases\releases9.9.0\net48 C:\Work\fzj\pylpg\LPG_9.9.0_win /E /R:0 /W:0 /MIR
robocopy V:\Dropbox\LPGReleases\releases9.9.0\linux C:\Work\fzj\pylpg\LPG_9.9.0_linux /E /R:0 /W:0 /MIR

pause