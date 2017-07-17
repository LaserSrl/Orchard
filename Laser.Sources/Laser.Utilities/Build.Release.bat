echo off
echo [96m Cancellazione bin e obj..[0m
set start=%time%
SET mypath=%~dp0
for %%i in (%mypath%..) do set "LaserSourcesFolder=%%~fi"
for %%i in (%LaserSourcesFolder%\..) do set "LaserPlatformOrchardFolder=%%~fi"
cd /D %LaserPlatformOrchardFolder%
if %errorlevel% neq 0 exit /b %errorlevel%
FOR /d /r . %%d IN (bin) DO @IF EXIST "%%d" rd /s /q "%%d"
FOR /d /r . %%d IN (obj) DO @IF EXIST "%%d" rd /s /q "%%d"
call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" x86

%LaserPlatformOrchardFolder%\Orchard.Sources\lib\nuget\nuget.exe restore C:\Sviluppo\Laser.Platform.Orchard\Orchard.Sources\src\Orchard.sln
if %errorlevel% neq 0 ( 
	echo Errore Nuget
	pause 
	exit /b %errorlevel%
)
echo [96m Compilando Orchard ...[0m
MSBuild /m /nologo /v:q /l:FileLogger,Microsoft.Build.Engine;logfile=%LaserPlatformOrchardFolder%\Laser.Sources\Laser.Utilities\Orchard.log %LaserPlatformOrchardFolder%\Orchard.Sources\src\Orchard.sln /t:Rebuild /p:Configuration="Release" /p:Platform="Any CPU"
if %errorlevel% neq 0 ( 
	echo [91m Errore Compilando Orchard [0m
	pause 
	exit /b %errorlevel%
)
echo [96m Orchard ok[0m
%LaserPlatformOrchardFolder%\Orchard.Sources\lib\nuget\nuget.exe restore C:\Sviluppo\Laser.Platform.Orchard\Laser.Sources\Laser.Orchard\Laser.Orchard.sln
echo [96m Compilando Laser ...[0m
MSBuild /m /nologo /v:q /l:FileLogger,Microsoft.Build.Engine;logfile=%LaserPlatformOrchardFolder%\Laser.Sources\Laser.Utilities\Laser.log  %LaserPlatformOrchardFolder%\Laser.Sources\Laser.Orchard\Laser.Orchard.sln /t:Rebuild /p:Configuration="Release" /p:Platform="Any CPU"
echo [96m Laser ok[0m
if %errorlevel% neq 0 ( 
	echo [91m Errore Compilando moduli Laser [0m
	pause 
	exit /b %errorlevel%
)
DEL %LaserPlatformOrchardFolder%\Orchard.Sources\src\Orchard.Web\App_Data\Dependencies\*.* /q
if %errorlevel% neq 0 ( 
	echo [91m Errore cancellando folder Dependencies [0m
	pause 
	exit /b %errorlevel%
)
echo [96m Done [0m
set end=%time%
set end=%time%
set options="tokens=1-4 delims=:.,"
for /f %options% %%a in ("%start%") do set start_h=%%a&set /a start_m=100%%b %% 100&set /a start_s=100%%c %% 100&set /a start_ms=100%%d %% 100
for /f %options% %%a in ("%end%") do set end_h=%%a&set /a end_m=100%%b %% 100&set /a end_s=100%%c %% 100&set /a end_ms=100%%d %% 100

set /a hours=%end_h%-%start_h%
set /a mins=%end_m%-%start_m%
set /a secs=%end_s%-%start_s%
set /a ms=%end_ms%-%start_ms%
if %ms% lss 0 set /a secs = %secs% - 1 & set /a ms = 100%ms%
if %secs% lss 0 set /a mins = %mins% - 1 & set /a secs = 60%secs%
if %mins% lss 0 set /a hours = %hours% - 1 & set /a mins = 60%mins%
if %hours% lss 0 set /a hours = 24%hours%
if 1%ms% lss 100 set ms=0%ms%

:: mission accomplished
set /a totalsecs = %hours%*3600 + %mins%*60 + %secs% 
echo command took %hours%:%mins%:%secs%.%ms% (%totalsecs%.%ms%s total)
Pause