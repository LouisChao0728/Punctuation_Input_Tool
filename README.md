# PunctInput 標點符號輸入工具

PC 用標點符號快速輸入面板（C# WinForms）。以全域快捷鍵呼叫，滑鼠點擊即把常用全形標點直接輸入至任何前景應用程式，不需切換輸入法、不打斷打字節奏。UI 比照 Windows 10 小算盤。

## 功能特色

1. **一鍵成對輸入**：「」『』《》【】四組括號各為一鍵，點一下直接輸入成對符號。
2. **全域快捷鍵**：Ctrl + Alt + / 顯示／隱藏面板，主鍵盤與數字鍵盤的 `/` 皆可。
3. **不搶焦點**：面板以 `WS_EX_NOACTIVATE` 呈現，點擊符號時游標仍留在原本編輯位置。
4. **繞過輸入法組字區**：依目標控制項類別自動選擇送字路徑（`WM_CHAR` 直遞／`SendInput`／剪貼簿中轉自動貼上），中文輸入法開啟時符號也直接定稿，不會卡在組字（預編譯）狀態。
5. **系統匣常駐**：Esc 或關閉鈕僅隱藏視窗；雙擊系統匣圖示切換顯示，右鍵選單結束程序；單一實例防重複啟動。
6. **零依賴**：單一 exe，僅需 Windows 內建 .NET Framework 4.x runtime；建置使用 Windows 內建 `csc.exe`，不需安裝任何 SDK。

## 符號清單（4 欄 2 列，共 7 鍵）

| 「」 | 『』 | 《》 | 【】 |
|------|------|------|------|
| **：** | **●** | **█** | |

## 快速開始

### 安裝（建議）

```powershell
powershell -ExecutionPolicy Bypass -File scripts\install.ps1
```

部署至 `%LOCALAPPDATA%\Programs\PunctInput\`，建立開始功能表捷徑與開機自啟捷徑並啟動。選項：`-NoStartup` 不設開機自啟、`-NoLaunch` 安裝後不啟動。

解除安裝：

```powershell
powershell -ExecutionPolicy Bypass -File scripts\uninstall.ps1
```

### 免安裝直接執行

```powershell
powershell -ExecutionPolicy Bypass -File scripts\build.ps1
.\dist\PunctInput.exe
```

## 操作方式

| 操作 | 行為 |
|------|------|
| Ctrl + Alt + /（主鍵盤或數字鍵盤） | 顯示／隱藏面板 |
| 點擊符號按鍵 | 直接輸入至前景應用程式 |
| Esc（面板顯示期間）／視窗關閉鈕 | 隱藏面板（程序常駐系統匣） |
| 系統匣圖示雙擊 | 顯示／隱藏切換 |
| 系統匣右鍵「結束」 | 結束程序（唯一結束路徑） |

## 環境需求

1. Windows 10 / 11。
2. .NET Framework 4.x runtime（Windows 內建，免安裝）。
3. 建置僅需內建編譯器 `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`（原始碼為 C# 5 相容）。

## 專案文件

| 文件 | 說明 |
|------|------|
| `DOC/00_PunctInput_INDEX.md` | 專案索引（結構、操作對照、文件地圖） |
| `DOC/01_PunctInput_PRD.md` | 產品需求文件（設計決策紀錄、風險） |
| `DOC/02_PunctInput_SPEC_v1.0.md` | 專案規格書（實作基準，含送字路由決策樹） |
| `DOC/03_PunctInput_SRS_v1.0.md` | 軟體需求規格書（FR／NFR 可測試需求） |
| `DOC/04_PunctInput_MODIFY_LOG.md` | 異動摘要紀錄（最新版在上） |
| `CLAUDE.md` | 專案維護規則（文件基準、版本作業檢核清單） |

## 已知限制

1. 面板顯示期間，Esc 為全域熱鍵：前景應用程式的第一個 Esc 會被面板吃掉（隱藏面板），面板隱藏後即恢復。
2. 無法輸入至以系統管理員權限執行的視窗（Windows UIPI 限制）。
3. 剪貼簿中轉路徑送字時，剪貼簿內容會被替換約 0.5 秒後自動還原；常見格式（文字、RTF、HTML、檔案清單、圖片）可還原，特殊格式可能遺失。
4. 非標準輸入管線的應用程式（如以 raw input 讀取鍵盤的遊戲）可能無法接收符號。

疑難排解：設定環境變數 `PUNCTINPUT_DEBUG=1` 後，送字路由與結果會記錄於 `%TEMP%\PunctInput_debug.log`。
