::Batch file to deploy Orchard.Core.dll

@echo off

pushd ".."

	pushd "..\Orchard.Sources\src\Orchard.Web\"
	
	SETLOCAL EnableDelayedExpansion
		for /d /r %%f in (*bin) do (
			if exist %%f\Orchard.Core.dll (
				echo File found in %%f >>C:\Sviluppo\Laser.Platform.Orchard\Laser.Sources\Laser.Deploy\folders.txt
				set folder=%%f
				echo File found in !folder:~66! >>C:\Sviluppo\Laser.Platform.Orchard\Laser.Sources\Laser.Deploy\folders.txt
				@echo on
				xcopy %%f\Orchard.Core.dll C:\Sviluppo\Laser.Platform.Orchard\Laser.Sources\Laser.Deploy\DeployScripts\Deploy!folder:~66!\ /S /Y 
				@echo off
			)
		)
	SETLOCAL DisableDelayedExpansion
	
	popd
popd

PAUSE
EXIT /B 0
