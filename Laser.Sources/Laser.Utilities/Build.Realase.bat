MSBuild C:\Sviluppo\Laser.Platform.Orchard\Orchard.Sources\src\Orchard.sln /t:Rebuild /p:Configuration="Release" /p:Platform="Any CPU"
PAUSE
MSBuild C:\Sviluppo\Laser.Platform.Orchard\Laser.Sources\Laser.Orchard\Laser.Orchard.sln /t:Rebuild /p:Configuration="Release" /p:Platform="Any CPU"
PAUSE
DEL C:\Sviluppo\Laser.Platform.Orchard\Orchard.Sources\src\Orchard.Web\App_Data\Dependencies\*.*
