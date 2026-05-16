@echo off
echo Dang khoi dong Lamour API...
start "Lamour API" /min /d "D:\app-lamour\LamourApi\api-win" "D:\app-lamour\LamourApi\api-win\Lamour.Api.exe"

echo Cho API khoi dong (20 giay)...
timeout /t 20 /nobreak > nul

echo Mo ung dung Lamour...
start "" "D:\app-lamour\LamourDesktop\desktop-win\DesktopLamour.exe"

exit
