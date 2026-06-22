@echo off
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo .NET 10 Runtime not found. Installing now...
    dotnet-sdk-10.0.301-win-x64.exe /quiet /norestart
    echo Installation complete. Launching Parks Route Planner...
    timeout /t 3 >nul
)
start "" "Parks Route Planner.exe"