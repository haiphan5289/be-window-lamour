#!/bin/bash
# Publish BE API as self-contained win-x64 executable — run on Mac.
# Matches the exact command in /DEPLOY.md ("Publish từ Mac (khi có code mới)" / "Bước 1").
# Output: publish/api-win/ — copy this folder to D:\app-lamour\LamourApi\api-win\ on the
# Windows target machine (TeamViewer file transfer), then re-check appsettings.Production.json
# (publish always resets Password to the CHANGE_ME placeholder — see DEPLOY.md warning).
set -euo pipefail
cd "$(dirname "$0")/.."

echo "[1/1] Publishing Lamour.Api (win-x64, self-contained)..."
dotnet publish src/Lamour.Api \
  -r win-x64 \
  --self-contained true \
  -c Release \
  -o publish/api-win

echo
echo "Done. Output: publish/api-win/"
echo "Next: copy this folder to D:\\app-lamour\\LamourApi\\api-win\\ on the target machine,"
echo "then fix appsettings.Production.json (Password=CHANGE_ME -> Password=lamour123)."
