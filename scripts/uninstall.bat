@echo off
rem PunctInput uninstaller wrapper. Double-click to uninstall.
rem Uses %~dp0 to locate uninstall.ps1, so it works from any start directory.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0uninstall.ps1"
pause
