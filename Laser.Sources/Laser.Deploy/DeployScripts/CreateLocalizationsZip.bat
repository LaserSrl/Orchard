::  Estrae la strutture dei soli files *-po presenti in tutti i moduli Orchard
::  Utilizzo: 
::  1. copiare il file localizations.zip sul server
::  2. Entrare nella cartella Orchard.Web e unzippare il file

pushd %~dp0
del localizations.zip
7z a -tzip localizations.zip -r ..\Orchard.Source\src\Orchard.Web\*.po -mx5 -xr!bin -xr!obj
pause