# =====================================================================
# PunctInput 解除安裝腳本
# 用法：powershell -ExecutionPolicy Bypass -File scripts\uninstall.ps1
# 動作：停止程序、移除開始功能表與開機自啟捷徑、移除安裝目錄。
# 不動專案原始碼與 dist\ 建置產出。
# =====================================================================
$ErrorActionPreference = "Stop"

$installDir = Join-Path $env:LOCALAPPDATA "Programs\PunctInput"
$startMenuLnk = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\PunctInput.lnk"
$startupLnk = Join-Path $env:APPDATA "Microsoft\Windows\Start Menu\Programs\Startup\PunctInput.lnk"

$running = Get-Process PunctInput -ErrorAction SilentlyContinue
if ($running) {
    Write-Output ("停止執行中的 PunctInput（PID {0}）" -f $running.Id)
    Stop-Process -Id $running.Id -Force
    Start-Sleep -Milliseconds 800
}

foreach ($item in @($startMenuLnk, $startupLnk, $installDir)) {
    if (Test-Path $item) {
        Remove-Item $item -Recurse -Force
        Write-Output ("已移除：{0}" -f $item)
    } else {
        Write-Output ("不存在（略過）：{0}" -f $item)
    }
}
Write-Output "解除安裝完成"
