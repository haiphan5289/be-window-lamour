# Publish DesktopLamour (WPF) as self-contained win-x64 exe — run on UTM Windows VM PowerShell.
# Matches /DEPLOY.md "Publish từ Mac (khi có code mới)" / "Bước 2" — enforces the mandatory
# sync -> publish -> zip order the doc warns about (skipping/reordering silently zips a stale build).
#
# Usage (UTM PowerShell, from anywhere — the script cd's into its own folder):
#   C:\projects\desktop-lamour\publish-wpf-utm.ps1
#
# Result: Z:\publish\desktop-win-new.zip (visible back on Mac at
# desktop-lamour/publish/desktop-win-new.zip via the Z:\ share) — extract into
# D:\app-lamour\LamourDesktop\desktop-win\ on the target machine. Never copy
# publish\desktop-win\ directly — always go through this zip step.

$ErrorActionPreference = "Stop"
Set-Location $PSScriptRoot

Write-Host "[1/3] Syncing source from Z:\ ..."
.\sync.ps1

Write-Host "[2/3] Publishing DesktopLamour (win-x64, self-contained)..."
dotnet publish src\DesktopLamour -r win-x64 --self-contained true -c Release -o publish\desktop-win

Write-Host "[3/3] Zipping to Z:\publish\desktop-win-new.zip ..."
if (-not (Test-Path "Z:\publish")) {
    Write-Host "Z:\publish does not exist yet — creating it."
    New-Item -ItemType Directory -Path "Z:\publish" -Force | Out-Null
}
Compress-Archive -Path "C:\projects\desktop-lamour\publish\desktop-win\*" `
                  -DestinationPath "Z:\publish\desktop-win-new.zip" -Force

Write-Host ""
Write-Host "Done. desktop-win-new.zip should now appear on Mac at desktop-lamour/publish/desktop-win-new.zip"
Write-Host "Next: extract it into D:\app-lamour\LamourDesktop\desktop-win\ on the target machine."
