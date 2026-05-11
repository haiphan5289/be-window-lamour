@echo off
REM Publish BE API as self-contained Windows x64 executable
REM Run this script from the be-window-lamour project root on a Windows/Mac machine
REM Then copy the output folder to the Windows Server

SET OUTPUT=.\publish\be

echo [1/3] Restoring packages...
dotnet restore src\Lamour.Api

echo [2/3] Publishing self-contained win-x64...
dotnet publish src\Lamour.Api ^
  -c Release ^
  -r win-x64 ^
  --self-contained true ^
  -p:PublishSingleFile=false ^
  -o %OUTPUT%

echo [3/3] Done. Output: %OUTPUT%
echo.
echo NEXT STEPS on Windows Server:
echo   1. Copy %OUTPUT% to C:\lamour\be\
echo   2. Edit C:\lamour\be\appsettings.Production.json (set Password)
echo   3. Run: C:\lamour\be\Lamour.Api.exe
echo      Or register as Windows Service (see deploy\install-service.bat)
pause
