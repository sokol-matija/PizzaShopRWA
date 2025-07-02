@echo off
echo === Quick Azure Deployment ===
echo.
echo This will deploy both API and WebApp to Azure...
echo Press Ctrl+C to cancel, or
pause
echo.
echo Starting deployment...
powershell -ExecutionPolicy Bypass -File "deploy-both.ps1"
if %ERRORLEVEL% EQU 0 (
    echo.
    echo === Deployment Complete ===
    echo Check the URLs above to verify deployment
) else (
    echo.
    echo === Deployment Failed ===
    echo Check the error messages above
)
echo.
pause 