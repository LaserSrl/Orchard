MSBuild C:\Sviluppo\Laser.Platform.Orchard\Orchard.Sources\src\Orchard.sln /t:Rebuild /p:Configuration=Debug
PAUSE
MSBuild C:\Sviluppo\Laser.Platform.Orchard\Laser.Sources\Laser.Orchard\Laser.Orchard.sln /t:Rebuild /p:Configuration=Debug
PAUSE
DELETE C:\Sviluppo\Laser.Platform.Orchard\Orchard.Sources\src\Orchard.Web\App_Data\Dependencies\*.*
