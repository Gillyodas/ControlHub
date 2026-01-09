@echo off
echo Starting ControlHub Frontend Development Server...
echo.
echo Make sure your backend API is running at http://localhost:5000
echo.
cd /d "%~dp0"
npm run dev
pause
