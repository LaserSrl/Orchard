echo off
set start=%time%
SET mypath=%~dp0
for %%i in (%mypath%..) do set "LaserSourcesFolder=%%~fi"
for %%i in (%LaserSourcesFolder%\..) do set "LaserPlatformOrchardFolder=%%~fi"
call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" x86
echo [96m Compilando Laser ...[0m
MSBuild /m /nologo /v:q /l:FileLogger,Microsoft.Build.Engine;logfile=%LaserPlatformOrchardFolder%\Laser.Sources\Laser.Utilities\Laser.log  %LaserPlatformOrchardFolder%\Laser.Sources\Laser.Orchard\Laser.Orchard.sln /t:Rebuild /p:Configuration="Debug" /p:Platform="Any CPU"
echo [96m Laser ok[0m
Pause