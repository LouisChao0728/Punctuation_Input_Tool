@echo off
rem PunctInput installer wrapper. Double-click to install.
rem Uses %~dp0 to locate install.ps1, so it works from any start directory.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0install.ps1"
pause
