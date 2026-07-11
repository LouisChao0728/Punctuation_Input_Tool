# =====================================================================
# PunctInput 安裝腳本
# 用法：powershell -ExecutionPolicy Bypass -File scripts\install.ps1 [-NoStartup] [-NoLaunch]
#   -NoStartup：不建立開機自啟捷徑
#   -NoLaunch ：安裝完成後不立即啟動
# 動作：
#   1. dist\PunctInput.exe 不存在時先執行 build.ps1 建置
#   2. 偵測既有安裝（安裝目錄或捷徑任一存在），有則先執行 uninstall.ps1 解除安裝
#   3. 停止執行中的 PunctInput，複製 exe 至 %LOCALAPPDATA%\Programs\PunctInput\
#   4. 建立開始功能表捷徑與開機自啟捷徑（Startup 資料夾）
#   5. 啟動已安裝之 PunctInput
# 解除安裝：powershell -ExecutionPolicy Bypass -File scripts\uninstall.ps1
# =====================================================================
param(
    [switch]$NoStartup,
    [switch]$NoLaunch
)
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$srcExe = Join-Path $root "dist\PunctInput.exe"
$installDir = Join-Path $env:LOCALAPPDATA "Programs\PunctInput"
$installExe = Join-Path $installDir "PunctInput.exe"
$startMenuLnk = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\PunctInput.lnk"
$startupLnk = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\Startup\PunctInput.lnk"

# 1. 確保建置產出存在
if (-not (Test-Path $srcExe)) {
    Write-Output "dist\PunctInput.exe 不存在，先執行建置..."
    & (Join-Path $PSScriptRoot "build.ps1")
    if (-not (Test-Path $srcExe)) {
        Write-Error "建置後仍找不到 $srcExe"
    }
}

# 2. 偵測既有安裝，先解除安裝（老闆 2026-07-11 指示：安裝前先清舊版）
$hasOld = (Test-Path $installDir) -or (Test-Path $startMenuLnk) -or (Test-Path $startupLnk)
if ($hasOld) {
    Write-Output "偵測到既有安裝，先執行解除安裝..."
    & (Join-Path $PSScriptRoot "uninstall.ps1")
}

# 3. 停止執行中實例（解除安裝已停止者此處為空跑保險），複製 exe
$running = Get-Process PunctInput -ErrorAction SilentlyContinue
if ($running) {
    Write-Output ("停止執行中的 PunctInput（PID {0}）" -f $running.Id)
    Stop-Process -Id $running.Id -Force
    Start-Sleep -Milliseconds 800
}
New-Item -ItemType Directory -Force -Path $installDir | Out-Null
Copy-Item -Path $srcExe -Destination $installExe -Force
$copied = Get-Item $installExe
Write-Output ("已部署：{0}（{1:N0} bytes）" -f $copied.FullName, $copied.Length)

# 4. 建立捷徑（WScript.Shell 為 Windows 內建 COM，零外部依賴）
$shell = New-Object -ComObject WScript.Shell
$lnk = $shell.CreateShortcut($startMenuLnk)
$lnk.TargetPath = $installExe
$lnk.WorkingDirectory = $installDir
$lnk.Description = "PunctInput"
$lnk.Save()
Write-Output ("開始功能表捷徑：{0}" -f $startMenuLnk)

if ($NoStartup) {
    if (Test-Path $startupLnk) {
        Remove-Item $startupLnk -Force
        Write-Output "已移除既有開機自啟捷徑（-NoStartup）"
    } else {
        Write-Output "略過開機自啟捷徑（-NoStartup）"
    }
} else {
    $lnk2 = $shell.CreateShortcut($startupLnk)
    $lnk2.TargetPath = $installExe
    $lnk2.WorkingDirectory = $installDir
    $lnk2.Description = "PunctInput"
    $lnk2.Save()
    Write-Output ("開機自啟捷徑：{0}" -f $startupLnk)
}

# 5. 啟動
if ($NoLaunch) {
    Write-Output "安裝完成（未啟動，-NoLaunch）"
} else {
    Start-Process $installExe
    Start-Sleep -Seconds 2
    $p = Get-Process PunctInput -ErrorAction SilentlyContinue
    if ($p) {
        Write-Output ("安裝完成並已啟動（PID {0}）：Ctrl + Alt + / 呼叫" -f $p.Id)
    } else {
        Write-Error "安裝完成但啟動失敗，請手動執行 $installExe"
    }
}
