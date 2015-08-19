::  Estrae la strutture dei soli files *-po presenti in tutti i moduli Orchard
::  Utilizzo: 
::  1. copiare il file localizations.zip sul server
::  2. Entrare nella cartella Orchard.Web e unzippare il file

pushd %~dp0
rmdir  /s /q Deploy
mkdir Deploy
XCOPY ..\Orchard.Source\src\Orchard.Web\Modules\*.* Deploy\Modules\ /S /Y /EXCLUDE:deploy.excludelist.txt
XCOPY ..\Orchard.Source\src\Orchard.Web\Themes\*.* Deploy\Themes\ /S /Y /EXCLUDE:deploy.excludelist.txt

pause