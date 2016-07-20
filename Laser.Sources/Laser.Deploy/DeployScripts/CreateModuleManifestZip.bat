::  Estrae la strutture dei soli files *-txt presenti in tutti i moduli Orchard Laser
::  Utilizzo: 
::  1. copiare il file modulesmanifest.zip sul server
::  2. Entrare nella cartella Orchard.Web e unzippare il file

pushd %~dp0
del modulesmanifest.zip 
7z a -tzip modulesmanifest.zip -r ..\..\..\Laser.Sources\Laser.Orchard\Module.txt -mx5 -xr!bin -xr!obj
7z a -tzip modulesmanifest.zip -r ..\..\..\Laser.Sources\Laser.Orchard\Theme.txt -mx5 -xr!bin -xr!obj
pause