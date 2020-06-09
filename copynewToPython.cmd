copy V:\Dropbox\LPG\WpfApplication1\profilegenerator-latest.db3 c:\work\LPGdev\WpfApplication1\profilegenerator-latest.db3
cd C:\Work\LPGDev\ReleaseMaker\bin\Debug
rmdir /S /Q V:\Dropbox\LPGReleases\releases9.6.0
releasemaker
robocopy V:\Dropbox\LPGReleases\releases9.6.0 C:\Work\utsp\LPG /E /R:0 /W:0 /MIR
pause