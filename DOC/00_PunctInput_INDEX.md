# PunctInput 專案索引（INDEX）

**用途**：專案結構之單一入口索引，供人與 AI 助理快速定位檔案職責、快捷鍵操作與文件。
**體例依據**：文件體系比照 Asset_Management（AssetM）專案慣例（老闆 2026-07-11 指示）。
**對應版本**：v1.4.3（2026-07-12，Aphy 分支——剪貼簿快照依 DD-10 限縮文字類白名單，修正延遲渲染擁有者致 UI 凍結；master 對應 v1.3.1）

---

## 一、 專案概觀

| 項目 | 內容 |
|------|------|
| 產品 | 標點符號輸入工具（英文識別 PunctInput）：PC 端全域標點符號快速輸入面板 |
| 專案根 | `C:\Users\user\Claude_WorkSpace\Punctuation_Input_Tool` |
| 技術棧 | C# WinForms + Windows 內建 `csc.exe`（DD-1，候選 Python 3.11 + tkinter、Electron 落選） |
| 送出行為 | 點擊符號直接輸入至前景應用程式（DD-2，候選「複製到剪貼簿」「兩者並行」落選） |
| 常駐機制 | 系統匣常駐（NotifyIcon）；Esc 與視窗關閉鈕僅隱藏視窗，程序結束僅由系統匣選單「結束」（DD-3） |
| 版本 | v1.4.3（Aphy 分支，manifest assembly version 1.4.3.0），2026-07-12 |
| 來源 | 老闆 Boss_Prompt 2026-07-11 指示建立；文件體系比照 Asset_Management 專案 |
| 權限範圍 | `Claude_WorkSpace` 非 Global Rules 完全權限路徑，本專案異動依老闆指示執行，不適用全權開發授權 |

## 二、 快捷鍵與操作對照

| 操作 | 觸發方式 | 行為 | 對應需求編號 |
|------|---------|------|--------------|
| Ctrl + Alt + / | 全域熱鍵（MOD_CONTROL + MOD_ALT + MOD_NOREPEAT；主鍵盤 VK_OEM_2 與數字鍵盤 VK_DIVIDE 雙註冊，v1.2 起） | 視窗顯示／隱藏切換 | FR-001 |
| Esc | 全域熱鍵，僅於視窗顯示期間註冊 | 隱藏視窗 | FR-002 |
| 點擊符號按鈕（Aphy 版 45 鍵） | 滑鼠點擊 | 送往前景執行緒焦點控制項，依類別三路路由（DD-4／DD-9）：EDIT 類走 WM_CHAR、主控台走 SendInput、其餘走剪貼簿中轉自動貼上；失敗後備 SendInput；成對鍵一鍵成對輸入兩字元 | FR-003、FR-004、FR-005 |
| 視窗關閉鈕 | 滑鼠點擊 | 視同隱藏（FormClosing 取消 + Hide），不結束程序 | FR-010 |
| 系統匣圖示雙擊 | 滑鼠雙擊 | 顯示／隱藏切換 | FR-009 |
| 系統匣右鍵選單「顯示／隱藏（Ctrl + Alt + /）」 | 滑鼠右鍵 | 顯示／隱藏切換 | FR-009 |
| 系統匣右鍵選單「結束」 | 滑鼠右鍵 | 結束程序（唯一結束路徑） | FR-009、FR-010 |

符號按鍵（FR-003；Aphy 版 v1.4.2 老闆裁決「1 列 11 項」＋獨立列，採列結構定義；成對鍵一鍵成對輸入）。Aphy 分支共 45 鍵、5 列：

| 列 | 按鍵 |
|----|------|
| 第 1 列（11 鍵） | 「」 『』 《》 【】 ： ● █ 〔〕 ﹝﹞ ← → |
| 第 2 列（11 鍵） | ➤ ❥ ♥ ♡ ► ◄ ⇒ ✔ ✓ ☑ ⛤ |
| 第 3 列（2 鍵） | ✿ ❀ |
| 第 4 列（11 鍵，獨立列） | ⓪ ① ② ③ ④ ⑤ ⑥ ⑦ ⑧ ⑨ ⑩ |
| 第 5 列（10 鍵，獨立列） | ⒈ ⒉ ⒊ ⒋ ⒌ ⒍ ⒎ ⒏ ⒐ ⒑ |

完整碼位對照見 `02_PunctInput_SPEC_v1.0.md` §5.1。

## 三、 檔案職責

| 路徑 | 職責 | 備考 |
|------|------|------|
| `src\Program.cs` | C# WinForms 單檔全部邏輯：視窗建置（`SymbolRows` 列結構）、DPI 縮放、全域熱鍵、符號送出三路路由（DD-4／DD-9，含剪貼簿快照——文字類白名單 DD-10——與還原）、系統匣、除錯日誌（FR-013） | 795 行（2026-07-12 v1.4.3 實查）；語言層級 C# 5（NFR-05） |
| `src\app.manifest` | DPI 感知宣告（`dpiAware=true`，NFR-03）+ Common Controls v6 相依宣告 | assembly version 1.4.3.0（Aphy 分支） |
| `scripts\build.ps1` | 建置腳本：呼叫 `csc.exe` 編譯 `Program.cs`，產出 `dist\PunctInput_Aphy.exe`（master 分支產出 `dist\PunctInput.exe`） | UTF-8 BOM；乾淨檢出時自動建立 `dist\`（NFR-02） |
| `scripts\install.ps1` | 安裝腳本（FR-014）：偵測既有安裝先解除舊版，再部署 exe 至 `%LOCALAPPDATA%\Programs\PunctInput\`、建立開始功能表與開機自啟捷徑、啟動 | UTF-8 BOM；`-NoStartup` 略過自啟、`-NoLaunch` 不啟動；dist 缺檔時自動先建置 |
| `scripts\uninstall.ps1` | 解除安裝腳本（FR-014）：停止程序、移除捷徑與安裝目錄 | UTF-8 BOM；不動原始碼與 `dist\` |
| `scripts\install.bat` / `scripts\uninstall.bat` | 雙擊執行包裝：以 `%~dp0` 自我定位呼叫對應 `.ps1`，與開啟位置無關（2026-07-11 老闆實測回饋新增） | 純 ASCII；結尾 `pause` 保留輸出視窗 |
| `scripts\Diagnose_Illustrator.bat` | 送字失效遠端取證工具（2026-07-12，Illustrator 事件；本分支即取證對象）：環境探測（Illustrator 程序與提權 heuristic、IME 清單、全程序清單）→ `PUNCTINPUT_DEBUG=1` 重啟工具 → 引導重現（含記事本對照組）→ debug log 收割，全數寫入同層 `log.txt` | 純 ASCII；結尾 `pause`；exe 四序位自動定位（同層 Aphy exe → `..\dist` → 安裝版 → 同層 PunctInput.exe）；不安裝、不改系統設定 |
| `dist\PunctInput_Aphy.exe` | Aphy 分支建置產出（可執行檔）；master 分支產出 `dist\PunctInput.exe`，dist 雙檔並存 | 18,432 bytes（2026-07-12 v1.4.3 實查值） |
| `DOC\` | 本專案文件目錄 | 詳見第四節 |
| `CLAUDE.md` | 專案規則（比照 AssetM 慣例）：Rule 1 文件基準、Rule 2 異動紀錄、Rule 3 權限、Rule 4 檢核清單、Rule 5 送字路由義務、Rule 6 建置環境 | 已建立（2026-07-11） |
| `README.md` | GitHub repo 首頁說明：功能特色、安裝與操作、環境需求、文件地圖、已知限制 | 2026-07-11 補檔（repo 公開後） |

## 四、 DOC 文件地圖

| 文件 | 用途 | 版本 |
|------|------|------|
| `00_PunctInput_INDEX.md` | 本檔，專案索引 | v1.4.3（2026-07-12） |
| `01_PunctInput_PRD.md` | 產品需求文件（含設計決策紀錄 DD-1 至 DD-10、風險 R1 至 R6） | v1.4.3（2026-07-12） |
| `02_PunctInput_SPEC_v1.0.md` | 專案規格書（實作基準；檔名依慣例維持 v1.0，內容就地更新） | v1.4.3（2026-07-12） |
| `03_PunctInput_SRS_v1.0.md` | 軟體需求規格書（FR-001 至 FR-014、NFR-01 至 NFR-05；檔名依慣例維持 v1.0，內容就地更新） | v1.4.3（2026-07-12） |
| `04_PunctInput_MODIFY_LOG.md` | 異動摘要紀錄（最新版在上） | v1.4.3（2026-07-12） |

## 五、 環境速查

| 項目 | 值 |
|------|-----|
| 編譯器 | `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe` |
| Runtime | Windows 內建 .NET Framework 4.x（NFR-01，免安裝） |
| 建置指令 | `powershell -ExecutionPolicy Bypass -File scripts\build.ps1` |
| 安裝指令 | 雙擊 `scripts\install.bat`；或於專案根目錄執行 `powershell -ExecutionPolicy Bypass -File scripts\install.ps1`（FR-014，選項 `-NoStartup` / `-NoLaunch`；相對路徑須先 `cd` 至專案根） |
| 解除安裝指令 | 雙擊 `scripts\uninstall.bat`；或於專案根目錄執行 `powershell -ExecutionPolicy Bypass -File scripts\uninstall.ps1` |
| 安裝位置 | `%LOCALAPPDATA%\Programs\PunctInput\PunctInput.exe`；捷徑於開始功能表與 Startup 資料夾 |
| 建置參數 | `/nologo /codepage:65001 /target:winexe /platform:anycpu /optimize+ /win32manifest:"src\app.manifest" /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /out:"dist\PunctInput_Aphy.exe"`（NFR-02，全參數見 `scripts\build.ps1`；master 之 `/out` 為 `dist\PunctInput.exe`） |
| 除錯環境變數 | `PUNCTINPUT_DEBUG=1`（啟用時 append 寫入 `%TEMP%\PunctInput_debug.log`，FR-013） |
| 單一實例識別 | Mutex 名稱 `PunctInput_SingleInstance_Mutex`（FR-011） |
| 建置產出 | `dist\PunctInput_Aphy.exe`（18,432 bytes，v1.4.3）；master 為 `dist\PunctInput.exe`（17,408 bytes，v1.3.1） |

---

*本檔為專案索引；內容與實際結構不符時，以程式碼與最新版 SPEC 為準並回頭修正本檔。*
