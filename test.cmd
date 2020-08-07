cd C:\Work\LPGDev\ReleaseMaker
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe" ReleaseMaker.csproj -t:rebuild  -v:m

cd C:\Work\LPGDev\ReleaseMaker\bin\Debug
releasemaker
pause