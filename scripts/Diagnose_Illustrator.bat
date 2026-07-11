@echo off
setlocal
REM ============================================================
REM  PunctInput (Aphy) - Illustrator send-failure diagnostic
REM  Purpose : capture environment info + PunctInput debug log
REM            into log.txt (same folder as this bat) so the
REM            failure route can be identified remotely.
REM  Usage   : double-click this file, then follow the prompts.
REM  Notes   : pure ASCII on purpose (avoids codepage garbling);
REM            no installation and no permanent system change,
REM            PUNCTINPUT_DEBUG=1 lives only in this session.
REM ============================================================

set "LOG=%~dp0log.txt"
set "DBGLOG=%TEMP%\PunctInput_debug.log"

echo ================================================== > "%LOG%"
echo  PunctInput Aphy - Illustrator diagnostic capture >> "%LOG%"
echo ================================================== >> "%LOG%"

REM ---------- STEP-1 session info ----------
echo [STEP-1] Session info >> "%LOG%"
echo DateTime: %DATE% %TIME% >> "%LOG%"
echo Computer: %COMPUTERNAME% >> "%LOG%"
echo User: %USERNAME% >> "%LOG%"
ver >> "%LOG%"
reg query "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion" /v DisplayVersion >> "%LOG%" 2>&1
chcp >> "%LOG%"
net session >nul 2>&1
if %errorlevel%==0 (echo BatchElevation: ELEVATED >> "%LOG%") else (echo BatchElevation: normal user >> "%LOG%")
echo. >> "%LOG%"

REM ---------- STEP-2 preserve any previous debug log ----------
echo [STEP-2] Previous PunctInput debug log >> "%LOG%"
if exist "%DBGLOG%" (
    echo --- old debug log content preserved below, original removed --- >> "%LOG%"
    type "%DBGLOG%" >> "%LOG%"
    del /Q "%DBGLOG%"
    echo --- end of old debug log --- >> "%LOG%"
) else (
    echo no previous debug log found >> "%LOG%"
)
echo. >> "%LOG%"

REM ---------- STEP-3 stop running PunctInput instances ----------
echo [STEP-3] Stopping running PunctInput instances >> "%LOG%"
taskkill /IM PunctInput_Aphy.exe /F >> "%LOG%" 2>&1
taskkill /IM PunctInput.exe /F >> "%LOG%" 2>&1
echo. >> "%LOG%"

REM ---------- STEP-4 locate the exe to diagnose ----------
set "EXE="
if exist "%~dp0PunctInput_Aphy.exe" set "EXE=%~dp0PunctInput_Aphy.exe"
if not defined EXE if exist "%~dp0..\dist\PunctInput_Aphy.exe" set "EXE=%~dp0..\dist\PunctInput_Aphy.exe"
if not defined EXE if exist "%LOCALAPPDATA%\Programs\PunctInput\PunctInput.exe" set "EXE=%LOCALAPPDATA%\Programs\PunctInput\PunctInput.exe"
if not defined EXE if exist "%~dp0PunctInput.exe" set "EXE=%~dp0PunctInput.exe"
if not defined EXE goto :noexe

echo [STEP-4] Exe under diagnosis >> "%LOG%"
echo ExePath: %EXE% >> "%LOG%"
for %%A in ("%EXE%") do echo ExeSize: %%~zA bytes >> "%LOG%"
echo Reference sizes: master v1.3 = 17408 bytes, Aphy v1.4.2 = 17920 bytes >> "%LOG%"
certutil -hashfile "%EXE%" MD5 >> "%LOG%" 2>&1
echo. >> "%LOG%"

REM ---------- STEP-5 environment probes (before repro) ----------
echo [STEP-5] Illustrator process state - BEFORE repro >> "%LOG%"
powershell -NoProfile -Command "$p=Get-Process -Name Illustrator -ErrorAction SilentlyContinue; if(-not $p){'Illustrator: NOT RUNNING'} else { $p | ForEach-Object { $s='Illustrator PID='+$_.Id+' Path='+$_.Path; try { $null=$_.Handle; $s+' HandleAccess=OK - likely NOT elevated' } catch { $s+' HandleAccess=DENIED - likely ELEVATED, UIPI suspect' } } }" >> "%LOG%"
powershell -NoProfile -Command "$p=Get-Process -Name Illustrator -ErrorAction SilentlyContinue | Select-Object -First 1; if($p -and $p.Path){ 'IllustratorFileVersion: '+(Get-Item $p.Path).VersionInfo.FileVersion } else { 'IllustratorFileVersion: n/a' }" >> "%LOG%"
echo. >> "%LOG%"
echo [STEP-5] Installed input languages / IMEs >> "%LOG%"
powershell -NoProfile -Command "foreach($l in (Get-WinUserLanguageList)){ $l.LanguageTag + ' InputMethods: ' + ($l.InputMethodTips -join ' ; ') }" >> "%LOG%"
echo. >> "%LOG%"
echo [STEP-5] Full process list - for clipboard managers and hook tools >> "%LOG%"
tasklist >> "%LOG%" 2>&1
echo. >> "%LOG%"

REM ---------- STEP-6 start exe with debug logging on ----------
echo [STEP-6] Starting exe with PUNCTINPUT_DEBUG=1 >> "%LOG%"
set "PUNCTINPUT_DEBUG=1"
start "" "%EXE%"
"%SystemRoot%\System32\ping.exe" -n 3 127.0.0.1 >nul
tasklist /FI "IMAGENAME eq PunctInput.exe" >> "%LOG%" 2>&1
tasklist /FI "IMAGENAME eq PunctInput_Aphy.exe" >> "%LOG%" 2>&1
echo StartTime: %TIME% >> "%LOG%"
echo. >> "%LOG%"

REM ---------- STEP-7 user reproduces the problem ----------
echo ==================================================
echo  REPRODUCE THE PROBLEM NOW:
echo.
echo   1. Switch to Adobe Illustrator.
echo   2. Select the Type tool and click inside a text
echo      object, so a text cursor is blinking.
echo   3. Open the symbol panel with Ctrl+Alt+/ or the
echo      NumPad / key, click the symbols that fail.
echo      Remember what you see on screen.
echo   4. CONTROL TEST: open Notepad and click one
echo      symbol there as well.
echo   5. Come back to this window and press any key.
echo ==================================================
pause >nul

REM ---------- STEP-8 collect results ----------
echo [STEP-8] Repro finished >> "%LOG%"
echo EndTime: %DATE% %TIME% >> "%LOG%"
echo --- PunctInput debug log captured during repro --- >> "%LOG%"
if exist "%DBGLOG%" (
    type "%DBGLOG%" >> "%LOG%"
) else (
    echo NO DEBUG LOG PRODUCED - exe did not inherit PUNCTINPUT_DEBUG or no symbol was clicked >> "%LOG%"
)
echo --- end of debug log --- >> "%LOG%"
echo. >> "%LOG%"
echo [STEP-8] Illustrator process state - AFTER repro >> "%LOG%"
powershell -NoProfile -Command "$p=Get-Process -Name Illustrator -ErrorAction SilentlyContinue; if(-not $p){'Illustrator: NOT RUNNING'} else { $p | ForEach-Object { $s='Illustrator PID='+$_.Id+' Path='+$_.Path; try { $null=$_.Handle; $s+' HandleAccess=OK - likely NOT elevated' } catch { $s+' HandleAccess=DENIED - likely ELEVATED, UIPI suspect' } } }" >> "%LOG%"
echo. >> "%LOG%"
echo [DONE] capture complete >> "%LOG%"

echo.
echo Done. Diagnostic saved to:
echo   %LOG%
echo.
echo Please send log.txt back for analysis.
echo The symbol tool keeps running in debug mode until you
echo close it; restarting it later returns to normal mode.
pause
exit /b 0

:noexe
echo [STEP-4] FAIL: no PunctInput exe found >> "%LOG%"
echo Searched: %~dp0PunctInput_Aphy.exe >> "%LOG%"
echo Searched: %~dp0..\dist\PunctInput_Aphy.exe >> "%LOG%"
echo Searched: %LOCALAPPDATA%\Programs\PunctInput\PunctInput.exe >> "%LOG%"
echo Searched: %~dp0PunctInput.exe >> "%LOG%"
echo.
echo ERROR: PunctInput_Aphy.exe was not found.
echo Put this bat file in the same folder as PunctInput_Aphy.exe
echo and run it again.
pause
exit /b 1
