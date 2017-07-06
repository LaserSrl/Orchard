call "C:\Program Files (x86)\Microsoft Visual Studio 14.0\VC\vcvarsall.bat" x86
nuget.exe restore C:\Sviluppo\Laser.Platform.Orchard\Orchard.Sources\src\Orchard.sln
MSBuild C:\Sviluppo\Laser.Platform.Orchard\Orchard.Sources\src\Orchard.sln /t:Rebuild /p:Configuration="Release" /p:Platform="Any CPU"
PAUSE
nuget.exe restore C:\Sviluppo\Laser.Platform.Orchard\Laser.Sources\Laser.Orchard\Laser.Orchard.sln
MSBuild C:\Sviluppo\Laser.Platform.Orchard\Laser.Sources\Laser.Orchard\Laser.Orchard.sln /t:Rebuild /p:Configuration="Release" /p:Platform="Any CPU"
PAUSE
DEL C:\Sviluppo\Laser.Platform.Orchard\Orchard.Sources\src\Orchard.Web\App_Data\Dependencies\*.*
