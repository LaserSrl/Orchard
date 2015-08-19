:: Estrae la strutture dei soli files *-po presenti in tutti i moduli Orchard
:: Utilizzo: 
:: 1. copiare il file localizations.zip sul server
:: 2. Entrare nella cartella Orchard.Web e unzippare il file

pushd %~dp0
@echo off
set /p removefolder="Eliminare la precedente cartella (S/N)?"
if %removefolder% == S (
	goto removefolderaction
) else (
	if %removefolder% == s (
		goto removefolderaction
	) else (
		if %removefolder% == Y (
			goto removefolderaction
		) else (
			if %removefolder% == y (
				goto :removefolderaction
			) else (
				goto :createcopyaction
			)
		)
	)
)

:removefolderaction
rmdir /s /q DeploySingleFile
goto :createcopy

:createcopyaction
mkdir DeploySingleFile

@echo off
set /p nomefile="Inserire Nome File es. (Orchard.Taxonomies.dll): " %=%
XCOPY ..\Orchard.Source\src\Orchard.Web\Modules\*%nomefile% DeploySingleFile\Modules\ /S /Y 
XCOPY ..\Orchard.Source\src\Orchard.Web\Themes\*%nomefile% DeploySingleFile\Themes\ /S /Y 
XCOPY ..\Orchard.Source\src\Orchard.Web\bin\*%nomefile% DeploySingleFile\bin\ /S /Y 

pause
