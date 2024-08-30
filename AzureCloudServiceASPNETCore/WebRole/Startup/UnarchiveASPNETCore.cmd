@ECHO OFF
SETLOCAL

set startupLog=WebRoleCoreStartupConfigLog.txt
echo %date% %time:~0,2%:%time:~3,2%:%time:~6,2% Starting web role config ... >> %startupLog%

PowerShell -ExecutionPolicy Unrestricted .\Startup\UnarchiveASPNETCore.ps1 >> %startupLog% 2>&1

EXIT /B 0