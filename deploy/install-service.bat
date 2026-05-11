@echo off
REM Register Lamour BE API as a Windows Service
REM Run as Administrator on the Windows Server
REM BE must already be published to C:\lamour\be\

SET SERVICE_NAME=LamourApi
SET EXE_PATH=C:\lamour\be\Lamour.Api.exe
SET DISPLAY_NAME=Lamour API Service
SET DESCRIPTION=Lamour cosmetics management REST API

echo Registering Windows Service: %SERVICE_NAME%

sc create %SERVICE_NAME% binPath= "%EXE_PATH%" start= auto DisplayName= "%DISPLAY_NAME%"
sc description %SERVICE_NAME% "%DESCRIPTION%"
sc start %SERVICE_NAME%

echo.
echo Service registered and started.
echo To check status: sc query %SERVICE_NAME%
echo To stop:         sc stop %SERVICE_NAME%
echo To remove:       sc delete %SERVICE_NAME%
pause
