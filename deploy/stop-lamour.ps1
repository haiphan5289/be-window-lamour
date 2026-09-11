# Stop Lamour API + Desktop app before copying new build files — run on the TARGET Windows
# machine (D:\app-lamour\), PowerShell. Matches /DEPLOY.md "Buoc 3 - Dung app tren may dich".
#
# Usage: .\stop-lamour.ps1
# Then copy new files (see publish-be-mac.sh / publish-wpf-utm.ps1 output), fix
# appsettings.Production.json password, and run start-lamour.bat again.

Write-Host "Stopping Lamour.Api..."
Stop-Process -Name "Lamour.Api" -Force -ErrorAction SilentlyContinue

Write-Host "Stopping DesktopLamour..."
Stop-Process -Name "DesktopLamour" -Force -ErrorAction SilentlyContinue

Write-Host "Done. Safe to copy new build files now."
