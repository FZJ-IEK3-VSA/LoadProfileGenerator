cd C:\Work\LPGDev\SimulationEngine
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe" SimulationEngine.csproj -t:rebuild
cd C:\Work\LPGDev\WpfApplication1
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe" LoadProfileGenerator.csproj -t:rebuild
cd C:\Work\LPGDev\ReleaseMaker
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe" ReleaseMaker.csproj -t:rebuild
copy V:\Dropbox\LPG\WpfApplication1\profilegenerator-latest.db3 c:\work\LPGdev\WpfApplication1\profilegenerator-latest.db3
cd C:\Work\LPGDev\ReleaseMaker\bin\Debug
rmdir /S /Q V:\Dropbox\LPGReleases\releases9.6.0
releasemaker
robocopy V:\Dropbox\LPGReleases\releases9.6.0 C:\Work\utsp\LPG /E /R:0 /W:0 /MIR
cd ..
cd ..
cd ..

pause