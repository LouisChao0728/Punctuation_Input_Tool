# PunctInput 專案規格書（SPEC）v1.0

**文件版本**：v1.1（2026-07-11；檔名依 AssetM 慣例維持 v1.0，就地更新並於文末註記沿革）
**定位**：本文件為實作與改動的唯一基準；規格與程式碼逐一對應，兩者不一致時以本文件與現行程式碼複核後修正。
**對照文件**：`01_PunctInput_PRD.md`（產品定位與設計決策）、`03_PunctInput_SRS_v1.0.md`（FR / NFR 可測試需求與驗收條件）
**實作對象**：`src\Program.cs`（單檔全部邏輯）、`src\app.manifest`、`scripts\build.ps1`、`dist\PunctInput.exe`

---

## 一、 產品概觀

1. 產品：標點符號輸入工具（PunctInput），Windows 桌面用之常駐小工具。UI 比照 Windows 10 小算盤：上方顯示區 + 下方按鍵格。
2. 核心行為：點擊符號按鈕，即以送字策略（第七章）將該符號直接輸入至前景應用程式的焦點控制項（DD-2 送出行為）。工具視窗不搶焦點，點擊後前景應用程式仍保有輸入游標。
3. 呼叫方式：全域快捷鍵 Ctrl + Alt + / 切換視窗顯示／隱藏（FR-001，v1.1 依 DD-8 改鍵）；程序常駐系統匣，關閉視窗僅隱藏，結束程序須由系統匣選單「結束」（DD-3、FR-010）。
4. 版本：v1.1（`app.manifest` assemblyIdentity version `1.1.0.0`），日期 2026-07-11。
5. 來源與授權界線：老闆 Boss_Prompt 2026-07-11 指示建立，文件體系比照 `Asset_Management` 專案。`Claude_WorkSpace` 非 Global Rules 完全權限路徑，本專案異動依老闆當次指示執行，不主張全權開發授權。

## 二、 技術棧與依賴

| 類別 | 選用 | 版本／路徑 | 用途 |
|------|------|-----------|------|
| 語言 | C# | 語言層級 C# 5 | 單檔 WinForms 應用程式邏輯 |
| UI 框架 | WinForms | .NET Framework 4.x | 視窗、按鈕、`NotifyIcon` 系統匣 |
| 執行環境 | .NET Framework | 4.x runtime（Windows 內建） | 免安裝執行（NFR-01） |
| 編譯器 | `csc.exe` | `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe` | 建置（NFR-02），無 .NET SDK |
| 建置腳本 | PowerShell | `scripts\build.ps1`（UTF-8 BOM） | 一鍵建置 |
| 系統 API | user32.dll | P/Invoke | 熱鍵、送字、視窗與焦點查詢 |

### 2.1 技術棧裁決（DD-1）

1. 選定 C# WinForms + Windows 內建 `csc.exe`（候選 Python 3.11 + tkinter、Electron 落選）。
2. 理由：零新增安裝、單一原生 exe、工具規模匹配。本機無 .NET SDK 與 AutoHotkey，僅有 .NET Framework 4.8 runtime 與內建編譯器。

### 2.2 C# 5 語言層級限制（NFR-05）

1. `csc.exe` 4.0.30319 之語言層級上限為 C# 5，撰寫時禁止使用下列後續版本語法：
   - 字串插值（`$"..."`）。
   - null 條件運算子（`?.`）。
   - `nameof` 運算子。
2. UI 文案一律繁體中文；原始碼字串常值中文以 `/codepage:65001` 確保正確編譯（第八章）。
3. 現行 `Program.cs` 委派以 `delegate(object s, EventArgs e) { ... }` 匿名方法撰寫（相容 C# 5），未使用 lambda 表達式以外之新語法。

## 三、 專案結構與檔案職責

```
Punctuation_Input_Tool/
├── DOC/                       文件（本檔等六件套）
│   └── 02_PunctInput_SPEC_v1.0.md   本規格書（實作基準）
├── src/
│   ├── Program.cs             全部邏輯：Main（單一實例）+ PunctPadForm（UI／熱鍵／送字／系統匣／P/Invoke）
│   └── app.manifest           DPI 感知（dpiAware=true）+ Common Controls v6 + supportedOS Win10/11
├── scripts/
│   └── build.ps1              建置腳本（csc.exe 一鍵編譯，UTF-8 BOM）
└── dist/
    └── PunctInput.exe         建置產出（14,848 bytes，2026-07-11）
```

### 3.1 `src\Program.cs` 內部結構

| 型別／區塊 | 職責 |
|-----------|------|
| `Program.Main`（`[STAThread]`） | 單一實例 `Mutex`（名稱 `PunctInput_SingleInstance_Mutex`，FR-011）、`EnableVisualStyles`、`Application.Run(new PunctPadForm())` |
| `PunctPadForm` 建構式 | DPI 縮放取得、視窗屬性設定、`BuildDisplay` / `BuildButtonGrid` / `BuildTrayIcon` |
| `BuildDisplay` | 上方顯示區 `Label`（FR-008） |
| `BuildButtonGrid` | 3×4 符號按鍵格（FR-003） |
| `BuildTrayIcon` | 系統匣 `NotifyIcon` 與右鍵選單（FR-009） |
| `CreateParams` / `ShowWithoutActivation` / `WndProc`(`WM_MOUSEACTIVATE`) | 不搶焦點機制（FR-006） |
| `OnHandleCreated` / `OnHandleDestroyed` / `OnVisibleChanged` | 熱鍵生命週期（第六章） |
| `WndProc`(`WM_HOTKEY`) | 熱鍵訊息分派（FR-001、FR-002） |
| `OnFormClosing` | 關閉鈕視同隱藏（FR-010） |
| `SendSymbolToTarget` / `SendUnicodeString` | 送字策略雙路徑（第七章） |
| `DebugLog` | 除錯日誌（FR-013） |
| P/Invoke 宣告與結構 | user32.dll 匯入、`GUITHREADINFO`／`INPUT`／`KEYBDINPUT` 等 |

## 四、 UI 規格

### 4.1 主視窗屬性

| 屬性 | 值 | 來源 |
|------|-----|------|
| `Text`（標題） | `標點符號輸入工具`（`AppTitle` 常數） | V2 驗證標題 |
| `FormBorderStyle` | `FixedSingle` | 不可縮放 |
| `MaximizeBox` / `MinimizeBox` | `false` / `false` | 固定尺寸小工具 |
| `TopMost` | `true`（+ `WS_EX_TOPMOST`） | 置頂（FR-007、NFR-04） |
| `ShowInTaskbar` | `false` | 常駐工具不占工作列 |
| `StartPosition` | `CenterScreen` | 置中 |
| `BackColor` | RGB(230,230,230) | 小算盤風底色（NFR-04） |
| `Font` | Segoe UI 9pt | 全域字型（NFR-04） |
| `ClientSize` | 寬 `Scale(3*78+16)`＝250、高 `Scale(48+4*62+20)`＝316（縮放前，96 DPI） | 版面計算 |

### 4.2 顯示區（FR-008）

1. 元件：`Label`（`_display`），比照小算盤顯示區。
2. 位置與尺寸（縮放前）：Location (8, 6)、Size (ClientSize.Width − 16, 42)。
3. 對齊：`ContentAlignment.MiddleRight`（右對齊）。
4. 字型／顏色：Segoe UI 20pt、前景 RGB(32,32,32)、背景同視窗底色 RGB(230,230,230)。
5. 內容：初值空字串；每次點擊符號後更新為最後點擊之符號（回饋用途，不影響送字）。

### 4.3 按鍵格（FR-003）

1. 佈局：3 欄 × 4 列（11 個按鈕，末列 2 個）。
2. 尺寸（縮放前）：按鈕寬 74、高 56、間距 4；起點 left 8、top 52。
3. 座標公式：`left + (i % 3) * (74 + 4)`、`top + (i / 3) * (56 + 4)`（i 為符號索引）。
4. 樣式：`FlatStyle.Flat`；背景 RGB(250,250,250)、前景 RGB(32,32,32)、字型 Segoe UI 16pt。
5. 框線與互動色：`BorderColor` RGB(230,230,230)、`BorderSize` 1；`MouseOverBackColor` RGB(218,218,218)、`MouseDownBackColor` RGB(200,200,200)。
6. `TabStop = false`：按鈕不進入 Tab 焦點鏈（配合不搶焦點）。
7. 事件：`Click += OnSymbolClick`。

### 4.4 DPI 縮放（NFR-03）

1. Manifest 宣告 `dpiAware=true`（`app.manifest`）。
2. 執行期 `GetDpiScale()`：以 `Graphics.DpiX / 96f` 取縮放係數。
3. `Scale(int value, float scale)`：`(int)Math.Round(value * scale)`，套用於所有版面尺寸與座標，於 4.1 至 4.3 全部以 `Scale(...)` 包裹。

### 4.5 系統匣（FR-009、NFR-04）

1. 元件：`NotifyIcon`（`_trayIcon`），圖示取自 `Icon.ExtractAssociatedIcon(Application.ExecutablePath)`。
2. 提示文字：`標點符號輸入工具（Ctrl + Alt + / 呼叫）`。
3. 雙擊（`DoubleClick`）：`TogglePad()` 切換顯示。
4. 右鍵選單（`ContextMenuStrip`）：
   - `顯示／隱藏（Ctrl + Alt + /）` → `TogglePad()`。
   - 分隔線。
   - `結束` → `ExitApp()`（唯一結束程序入口）。

## 五、 符號清單資料契約（FR-003）

### 5.1 符號集與順序

1. 符號來源：Boss_Prompt 指定順序，共 11 個，硬編於 `Symbols` 字串陣列，禁止改序。
2. 送出內容即字串本身（單一 UTF-16 碼元，皆位於 BMP）。

| 序 | 索引 i | 符號 | 碼位 | 名稱 |
|----|-------|------|------|------|
| 1 | 0 | 「 | U+300C | 左角括號 |
| 2 | 1 | 」 | U+300D | 右角括號 |
| 3 | 2 | 『 | U+300E | 左白角括號 |
| 4 | 3 | 』 | U+300F | 右白角括號 |
| 5 | 4 | 《 | U+300A | 左書名號 |
| 6 | 5 | 》 | U+300B | 右書名號 |
| 7 | 6 | 【 | U+3010 | 左黑透鏡括號 |
| 8 | 7 | 】 | U+3011 | 右黑透鏡括號 |
| 9 | 8 | ： | U+FF1A | 全形冒號 |
| 10 | 9 | ● | U+25CF | 實心圓 |
| 11 | 10 | █ | U+2588 | 整格方塊 |

### 5.2 按鈕配置對照（3 欄 × 4 列）

| 列＼欄 | 第 0 欄 | 第 1 欄 | 第 2 欄 |
|--------|--------|--------|--------|
| 第 0 列 | 「（i=0） | 」（i=1） | 『（i=2） |
| 第 1 列 | 』（i=3） | 《（i=4） | 》（i=5） |
| 第 2 列 | 【（i=6） | 】（i=7） | ：（i=8） |
| 第 3 列 | ●（i=9） | █（i=10） | （空） |

（配置由 `i % 3`（欄）與 `i / 3`（列）決定；第 3 列第 2 欄無按鈕。）

## 六、 熱鍵規格

### 6.1 註冊參數

| 熱鍵 | ID 常數 | 修飾鍵 | 虛擬鍵 | 功能 | FR |
|------|--------|--------|--------|------|----|
| Ctrl + Alt + / | `HOTKEY_ID_TOGGLE`＝1 | `MOD_CONTROL`(0x0002) \| `MOD_ALT`(0x0001) \| `MOD_NOREPEAT`(0x4000) | `VK_OEM_2`(0xBF) | 顯示／隱藏切換（v1.1 依 DD-8 改鍵） | FR-001 |
| Esc | `HOTKEY_ID_ESC`＝2 | `MOD_NOREPEAT`(0x4000) | `VK_ESCAPE`(0x1B) | 隱藏視窗 | FR-002 |

### 6.2 生命週期

1. Ctrl + Alt + / 於 `OnHandleCreated` 註冊、`OnHandleDestroyed` 反註冊（整個程序存活期間有效）。
2. Esc 為裸熱鍵（無修飾鍵），採 DD-5 副作用最小化：僅於視窗顯示期間註冊。
   - `OnVisibleChanged`：`Visible` 為真時 `RegisterEscHotkey()`；為假時 `UnregisterEscHotkey()`。
   - `RegisterEscHotkey` / `UnregisterEscHotkey` 以 `_escHotkeyRegistered` 布林旗標防重複註冊／反註冊。
   - `OnHandleDestroyed` 額外呼叫 `UnregisterEscHotkey()` 作為兜底。

### 6.3 WndProc 訊息處理

1. `WM_HOTKEY`(0x0312)：讀 `WParam` 為熱鍵 ID。
   - ID＝1（`HOTKEY_ID_TOGGLE`）→ `TogglePad()`。
   - ID＝2（`HOTKEY_ID_ESC`）→ `HidePad()`。
2. `WM_MOUSEACTIVATE`(0x0021)：回傳 `MA_NOACTIVATE`(3)，配合不搶焦點（FR-006）。
3. 其餘訊息交回 `base.WndProc`。

### 6.4 顯示／隱藏／結束語意（DD-3、FR-010）

1. `TogglePad`：依 `Visible` 切換 `ShowPad`（`Show()`）／`HidePad`（`Hide()`）。
2. `OnFormClosing`：`CloseReason.UserClosing` 且非結束旗標時，`e.Cancel = true` 並 `HidePad()`（關閉鈕視同隱藏）。
3. `ExitApp`：設 `_exiting = true` 後 `Close()`；此時 `OnFormClosing` 不攔截，並釋放 `NotifyIcon`（`Visible = false` + `Dispose`）。

## 七、 送字策略（DD-4 路由）

### 7.1 焦點解析（FR-004）

1. `GetForegroundWindow()` 取前景視窗 `fgWin`；若為 `Zero` 或等於本工具 `Handle`，記錄除錯日誌「no usable foreground window」後直接返回（不送字）。
2. `GetWindowThreadProcessId(fgWin, out pid)` 取前景執行緒 `tid`。
3. `GetGUIThreadInfo(tid, ref gti)` 取該執行緒焦點控制項 `gti.hwndFocus` 作為送字目標 `focus`；若為 `Zero`，退回以 `fgWin` 為目標。
4. `GetClassNameOf(focus)` 取目標控制項類別名 `cls`。

### 7.2 路由決策樹（DD-4）

```
取得焦點目標 focus 與類別名 cls
        │
   cls 含 "EDIT"（不分大小寫，IsClassicEditClass）？
        │
  ┌─────┴─────┐
  是            否
  │             │
WM_CHAR 直遞     SendInput（KEYEVENTF_UNICODE）
（PostMessageW）        │
  │                  送出完成、記錄日誌
  逐字元 PostMessageW
  │
 allPosted？
  ┌───┴───┐
  是        否（任一失敗）
  │         │
 完成返回   後備：SendInput（FR-005）
```

1. 傳統 IMM 控制項（`IsClassicEditClass`：`cls.IndexOf("EDIT", OrdinalIgnoreCase) >= 0`，涵蓋 `Edit`、`RICHEDIT50W`、`WindowsForms10.EDIT` 等）走 WM_CHAR 直遞：逐 UTF-16 碼元 `PostMessage(focus, WM_CHAR, (IntPtr)text[i], (IntPtr)1)`。
   - 依據：2026-07-11 本機實測 SendInput（`KEYEVENTF_UNICODE`）在注音 IME 開啟時被組字層攔截延後提交；WM_CHAR 攜帶字元字面值不經 IME。
2. 其餘目標（Chromium、Electron、UWP、Word 等 TSF 應用、主控台、Windows Terminal）走 SendInput：此類視窗可能將佇列中的 WM_CHAR 靜默忽略（對抗審查 3/3 確認 `PostMessage` 回傳 TRUE 僅代表入佇列，不代表已處理）。
3. WM_CHAR 綁定 `PostMessageW`（DD-6）：`DllImport` 明確 `EntryPoint = "PostMessageW"`。實測修正——預設 ANSI 綁定會把 CJK WM_CHAR 字元經代碼頁轉為問號。

### 7.3 後備條件（FR-005）

1. WM_CHAR 路徑中，任一碼元 `PostMessage` 回傳 `false` 即中止該路徑（`allPosted = false`），落入 `SendUnicodeString`（SendInput）後備。
2. 非 EDIT 類目標直接走 `SendUnicodeString`。

### 7.4 SendInput 實作（`SendUnicodeString`）

1. 每個 UTF-16 碼元產生 2 筆 `INPUT`（`INPUT_KEYBOARD`）：keydown（`KEYEVENTF_UNICODE`）+ keyup（`KEYEVENTF_UNICODE | KEYEVENTF_KEYUP`）。
2. `wVk = 0`、`wScan = ch`（字元碼元）。
3. `SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)))`；記錄 `injected` 與 `Marshal.GetLastWin32Error()`。

### 7.5 除錯日誌欄位（FR-013）

1. 觸發條件：環境變數 `PUNCTINPUT_DEBUG` 值為 `1`；append 寫入 `%TEMP%\PunctInput_debug.log`，行首時間戳 `HH:mm:ss.fff`。日誌失敗以 `catch` 吞除，不影響主功能。
2. WM_CHAR 路徑欄位：`route=WM_CHAR`、`text=U+XXXX`、`focus=0x{handle}`、`class={cls}`、`posted={allPosted}`。
3. SendInput 路徑欄位：`route=SendInput`、`text=U+XXXX`、`focus`、`class`；`SendUnicodeString` 另記 `requested`、`injected`、`lastError`、`foreground=0x{handle}`。
4. 略過送字時記：`SendSymbolToTarget skip: no usable foreground window`。

## 八、 建置與環境

### 8.1 `scripts\build.ps1` 全參數（NFR-02）

1. `$ErrorActionPreference = "Stop"`。
2. 路徑：`$root` 由 `Split-Path -Parent $PSScriptRoot`；`$csc` 固定 `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`；`$src` = `src\Program.cs`；`$manifest` = `src\app.manifest`；`$out` = `dist\PunctInput.exe`。
3. 前置檢查：`csc.exe` 不存在則 `Write-Error` 中止。
4. 輸出目錄：`New-Item -ItemType Directory -Force`（乾淨檢出時 `dist\` 不存在，csc 不自建目錄會失敗）。
5. 編譯命令參數：

| 參數 | 作用 |
|------|------|
| `/nologo` | 不印 logo |
| `/codepage:65001` | 原始碼 UTF-8，確保中文字串常值正確編譯 |
| `/target:winexe` | Windows GUI 應用（無主控台視窗） |
| `/platform:anycpu` | 平台中立 |
| `/optimize+` | 最佳化 |
| `/win32manifest:"$manifest"` | 內嵌 DPI／Common Controls manifest |
| `/r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll` | WinForms 參考組件 |
| `/out:"$out" "$src"` | 輸出 exe 與輸入原始碼 |

6. 後置檢查：`$LASTEXITCODE -ne 0` 則 `Write-Error`；成功印出 `建置完成：{路徑}（{bytes} bytes，{時間}）`。

### 8.2 BOM 注意事項

1. `build.ps1` 以 UTF-8 BOM 儲存（PowerShell 5.1 對含中文之腳本以 BOM 確保正確解碼）。
2. `Program.cs` 為 UTF-8，經 `/codepage:65001` 交由 csc 正確解讀中文字串常值。

### 8.3 Home PC 實況

| 項目 | 值 |
|------|-----|
| 編譯器 | `.NET Framework` 內建 `csc.exe`（v4.0.30319，Framework64） |
| .NET SDK | 無（本機僅 .NET Framework 4.8 runtime + 內建編譯器） |
| AutoHotkey | 無 |
| 建置指令 | `powershell -ExecutionPolicy Bypass -File scripts\build.ps1` |
| 產出 | `dist\PunctInput.exe`（14,848 bytes，2026-07-11） |

## 九、 錯誤處理與降級

| 情境 | 行為 | 對應 |
|------|------|------|
| Ctrl + Alt + / 註冊失敗（被佔用） | `OnHandleCreated` 顯示警告訊息框，程序續行，仍可由系統匣操作 | FR-012 |
| 重複啟動（已有實例） | `Mutex` `createdNew=false`，彈出「標點符號輸入工具已在執行中，請以 Ctrl + Alt + / 呼叫。」提示後結束 | FR-011 |
| 前景無可用視窗（Zero 或自身） | 記日誌後直接返回，不送字 | 7.1 |
| WM_CHAR 投遞失敗 | 落入 SendInput 後備 | FR-005 |
| UIPI（目標以系統管理員權限執行） | WM_CHAR 與 SendInput 皆被阻擋，靜默失敗（除錯日誌可查 `lastError`） | R3 |
| 除錯日誌寫入失敗 | `catch` 吞除，不影響主功能 | FR-013 |

## 十、 已知限制（R1 至 R5）

- **R1（全域熱鍵衝突，v1.1 已緩解）**：全域熱鍵仍優先於應用程式內同鍵位快捷鍵；v1.1 依 DD-8 由 Ctrl + / 改為 Ctrl + Alt + /（老闆 2026-07-11 回饋原鍵位實務使用困難），主流應用（VS Code、Word、Chrome、Edge、Obsidian、LINE）無此預設綁定，衝突面實質歸零。
- **R2（裸 Esc 攔截代價，老闆已採認）**：視窗顯示期間裸 Esc 被全域攔截，前景應用程式的 Esc（IME 組字取消、關閉選單等）會被吃掉一次並隱藏面板，面板隱藏後即恢復。對抗審查 3/3 確認之設計代價，已裁決保留（Boss_Prompt 明定 Esc 關閉、不搶焦點約束下無 `KeyPreview` 替代路徑）；老闆 2026-07-11 採認「R2 可接受」。
- **R3（UIPI）**：無法輸入至以系統管理員權限執行的視窗（WM_CHAR 與 SendInput 皆被阻擋，靜默失敗，除錯日誌可查）。
- **R4（TSF 相容性假設，已解除）**：SendInput 路徑對 TSF 應用（Chromium、Word）在 IME 開啟時的相容性原為「假設」；老闆 2026-07-11 實機驗證通過（V7 結案），後續異常時以 `PUNCTINPUT_DEBUG=1` 查路由。
- **R5（非標準輸入管線）**：非標準輸入管線應用（遊戲 raw input 等）可能忽略兩種注入方式。

## 十一、 驗證紀錄（2026-07-11 實測）

| 編號 | 項目 | 結果 |
|------|------|------|
| V1 | 編譯（csc） | 0 錯誤 |
| V2 | 啟動煙霧測試 | 程序回應、主視窗存在且標題「標點符號輸入工具」正確、無熱鍵註冊失敗警告視窗 |
| V3 | WM_CHAR 投遞 | 背景記事本 Edit 控制項全 11 符號依序完整到達（PASS） |
| V4 | Esc 隱藏 | `keybd_event` 注入 Esc，視窗實測隱藏（PASS） |
| V5 | Ctrl + / 呼叫（v1.0 當時鍵位） | `keybd_event` 注入組合鍵，視窗實測重現（PASS） |
| V6 | 對抗審查 | 3 視角（interop／spec／robust）Opus 審查共 16 項發現，3 人反駁表決確認 5 項（已修正或裁決文件化）、否決 11 項 |
| V7 | 老闆實機驗證 | 2026-07-11 老闆回報通過：實際滑鼠點擊送字至常用應用程式之路由正確性（結案） |
| V8 | v1.1 熱鍵改版實測 | `keybd_event` 注入 Ctrl + Alt + /：切換隱藏（PASS）、切換重現（PASS）；Esc 隱藏（PASS）（2026-07-11） |

## 十二、 設計決策索引（DD-1 至 DD-7）

| 編號 | 決策 | 類別 |
|------|------|------|
| DD-1 | 技術棧 C# WinForms + 內建 csc.exe | 老闆裁決 |
| DD-2 | 送出行為＝直接輸入至前景應用程式 | 老闆裁決 |
| DD-3 | Esc／關閉鈕語意＝隱藏視窗、程序常駐系統匣，結束僅由系統匣「結束」 | 自主設計 |
| DD-4 | 送字類別路由（EDIT 類走 WM_CHAR，其餘走 SendInput，失敗後備） | 自主設計 |
| DD-5 | Esc 全域熱鍵僅於視窗顯示期間註冊、隱藏即反註冊 | 自主設計 |
| DD-6 | `PostMessage` 明確綁定 `PostMessageW` | 自主設計 |
| DD-7 | 命名比照 AssetM：資料夾 `Punctuation_Input_Tool`、文件與 exe 前綴 `PunctInput` | 自主設計 |
| DD-8 | 呼叫快捷鍵改為 Ctrl + Alt + /（v1.1；原 Ctrl + / 因 R1 實務使用困難汰換，主流應用無預設綁定） | 老闆裁決（2026-07-11 回饋後委任選鍵） |

## 十三、 操作方式

1. 啟動：執行 `dist\PunctInput.exe`（顯示視窗與系統匣圖示）。
2. 呼叫：Ctrl + Alt + / 顯示／隱藏；Esc 或視窗關閉鈕隱藏；系統匣右鍵「結束」退出程序。
3. 重建：`powershell -ExecutionPolicy Bypass -File scripts\build.ps1`。

---

*本文件為 PunctInput 實作與改動基準，與現行 `src\Program.cs`、`scripts\build.ps1`、`src\app.manifest` 逐一對應；後續異動須同步更新本文件相關章節與版本沿革註記。沿革：v1.0（2026-07-11）首版建立；v1.1（2026-07-11）呼叫快捷鍵依 DD-8 改為 Ctrl + Alt + /，V7 結案、R1 緩解、R2 老闆採認、新增 V8。*
