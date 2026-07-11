# =====================================================================
# PunctInput 建置腳本
# 使用 Windows 內建 .NET Framework 編譯器（免安裝 SDK）。
# 用法：powershell -ExecutionPolicy Bypass -File scripts\build.ps1
# 產出：dist\PunctInput_Aphy.exe（Aphy 分支；master 分支產出 dist\PunctInput.exe）
# =====================================================================
$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$src = Join-Path $root "src\Program.cs"
$manifest = Join-Path $root "src\app.manifest"
$out = Join-Path $root "dist\PunctInput_Aphy.exe"

if (-not (Test-Path $csc)) {
    Write-Error "找不到 csc.exe：$csc"
}

# csc 不會自行建立輸出目錄，乾淨檢出時 dist\ 不存在會編譯失敗
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $out) | Out-Null

# /codepage:65001：原始碼為 UTF-8，確保中文字串常值正確編譯
& $csc /nologo /codepage:65001 /target:winexe /platform:anycpu /optimize+ `
    /win32manifest:"$manifest" `
    /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll `
    /out:"$out" "$src"

if ($LASTEXITCODE -ne 0) {
    Write-Error "編譯失敗（csc 結束碼 $LASTEXITCODE）"
}

$exe = Get-Item $out
Write-Output ("建置完成：{0}（{1:N0} bytes，{2}）" -f $exe.FullName, $exe.Length, $exe.LastWriteTime)
