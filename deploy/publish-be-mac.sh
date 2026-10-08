#!/bin/bash
# Publish BE API as self-contained win-x64 executable — run on Mac.
# Matches the exact command in /DEPLOY.md ("Publish từ Mac (khi có code mới)" / "Bước 1").
# Output: publish/api-win/ — copy this folder to D:\app-lamour\LamourApi\api-win\ on the
# Windows target machine (TeamViewer file transfer), then re-check appsettings.Production.json
# (publish always resets Password to the CHANGE_ME placeholder — see DEPLOY.md warning).
set -euo pipefail
cd "$(dirname "$0")/.."

# License: every build expires LICENSE_DAYS days after the build date (default 5), at
# 23:59:59 Vietnam time (= 16:59:59 UTC same day). Override: LICENSE_DAYS=30 bash deploy/publish-be-mac.sh
LICENSE_DAYS="${LICENSE_DAYS:-5}"
LICENSE_FILE="src/Lamour.Infrastructure/Licensing/LicenseService.cs"
EXPIRY_DATE="$(TZ=Asia/Ho_Chi_Minh date -v+"${LICENSE_DAYS}"d +%Y-%m-%d)"
IFS=- read -r EXP_Y EXP_M EXP_D <<< "$EXPIRY_DATE"
EXPIRY_CTOR="new($EXP_Y, $((10#$EXP_M)), $((10#$EXP_D)), 16, 59, 59, DateTimeKind.Utc);"

echo "[1/2] Setting license expiry to $EXPIRY_DATE 23:59:59 VN (build date + $LICENSE_DAYS days)..."
EXPIRY_DATE="$EXPIRY_DATE" EXPIRY_CTOR="$EXPIRY_CTOR" perl -pi -e '
  s|^(\s*// )\d{4}-\d{2}-\d{2} 23:59:59 giờ Việt Nam \(UTC\+7\) = \d{4}-\d{2}-\d{2} 16:59:59 UTC\.|$1$ENV{EXPIRY_DATE} 23:59:59 giờ Việt Nam (UTC+7) = $ENV{EXPIRY_DATE} 16:59:59 UTC.|;
  s|(ExpiresAtUtc = )new\(.*$|$1$ENV{EXPIRY_CTOR}|;
' "$LICENSE_FILE"
grep -qF "ExpiresAtUtc = $EXPIRY_CTOR" "$LICENSE_FILE" \
  || { echo "ERROR: could not update ExpiresAtUtc in $LICENSE_FILE" >&2; exit 1; }

echo "[2/2] Publishing Lamour.Api (win-x64, self-contained)..."
dotnet publish src/Lamour.Api \
  -r win-x64 \
  --self-contained true \
  -c Release \
  -o publish/api-win

echo
echo "Done. Output: publish/api-win/ (license expires $EXPIRY_DATE 23:59:59 VN)"
echo "Next: copy this folder to D:\\app-lamour\\LamourApi\\api-win\\ on the target machine,"
echo "then fix appsettings.Production.json (Password=CHANGE_ME -> Password=lamour123)."
