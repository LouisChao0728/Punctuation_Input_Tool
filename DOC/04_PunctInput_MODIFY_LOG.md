# PunctInput 異動摘要紀錄（MODIFY_LOG）

**規則**：最新版在上；每版記錄變更項目與影響範圍。

---

## v1.0（2026-07-11）——專案建立

### 觸發

1. 老闆 Boss_Prompt 2026-07-11 指示建立標點符號輸入工具（PunctInput），文件體系比照 Asset_Management 專案；Claude_WorkSpace 非 Global Rules 完全權限路徑，本專案異動依老闆指示執行（不得聲稱全權開發授權）。
2. 老闆裁決（AskUserQuestion 選項裁決）：
   - DD-1 技術棧：C# WinForms + Windows 內建 csc.exe（候選 Python 3.11 + tkinter、Electron 落選；理由：零新增安裝、單一原生 exe、工具規模匹配。本機無 .NET SDK 與 AutoHotkey，僅有 .NET Framework 4.8 runtime 與內建編譯器）。
   - DD-2 送出行為：點擊符號直接輸入至前景應用程式（候選「複製到剪貼簿」「兩者並行」落選）。

### 程式

初版功能清單（`src\Program.cs` 單檔全部邏輯）：

1. FR-001 全域快捷鍵 Ctrl + /（MOD_CONTROL + VK_OEM_2 + MOD_NOREPEAT）切換視窗顯示／隱藏。
2. FR-002 Esc 於視窗顯示期間全域隱藏視窗（顯示期間才註冊裸 Esc 熱鍵）。
3. FR-003 11 個符號按鈕，依序：「 」 『 』 《 》 【 】 ： ● █，3 欄 4 列。
4. FR-004 點擊符號送往前景執行緒焦點控制項（GetGUIThreadInfo），依 DD-4 類別路由送出。
5. FR-005 WM_CHAR 投遞失敗（PostMessageW 回傳 false）時後備 SendInput。
6. FR-006 不搶焦點：WS_EX_NOACTIVATE + WM_MOUSEACTIVATE 回 MA_NOACTIVATE + ShowWithoutActivation。
7. FR-007 視窗置頂（TopMost + WS_EX_TOPMOST）。
8. FR-008 顯示區回饋最後點擊符號（比照小算盤顯示區，右對齊）。
9. FR-009 系統匣常駐（NotifyIcon）：雙擊切換顯示；右鍵選單「顯示／隱藏（Ctrl + /）」「結束」。
10. FR-010 視窗關閉鈕視同隱藏（FormClosing 取消 + Hide），程序結束僅由系統匣「結束」。
11. FR-011 單一實例（Mutex：PunctInput_SingleInstance_Mutex），重複啟動彈出提示。
12. FR-012 Ctrl + / 註冊失敗顯示警告訊息，仍可由系統匣操作。
13. FR-013 除錯日誌：環境變數 PUNCTINPUT_DEBUG=1 時 append 寫入 %TEMP%\PunctInput_debug.log（記錄送字路由、類別、注入結果、前景 handle）；日誌失敗不影響主功能。

自主設計決策（DD-3 至 DD-7，供事後審計）：

1. DD-3 Esc 語意 = 隱藏視窗、程序常駐系統匣（Boss_Prompt「Esc 關閉此工具」解讀為關閉視窗；全域熱鍵 Ctrl + / 需常駐程序才能運作）。視窗關閉鈕同語意；程序結束僅由系統匣選單「結束」。
2. DD-4 送字類別路由（對抗審查修正）：焦點控制項類別名含 EDIT（不分大小寫，涵蓋 Edit、RICHEDIT50W、WindowsForms10.EDIT 等傳統 IMM 控制項）走 WM_CHAR 直遞（PostMessageW）；其餘目標（Chromium、Electron、UWP、Word 等 TSF 應用、主控台、Windows Terminal）走 SendInput（KEYEVENTF_UNICODE）。WM_CHAR 投遞 API 失敗時後備 SendInput。
3. DD-5 Esc 全域熱鍵僅於視窗顯示期間註冊、隱藏即反註冊（副作用最小化）。
4. DD-6 PostMessage 明確綁定 PostMessageW（`src\Program.cs` 第 487 至 489 行）。
5. DD-7 命名比照 AssetM 慣例：資料夾全名 Punctuation_Input_Tool、文件與 exe 前綴 PunctInput。

### 排錯

實測修正三項：

1. **build.ps1 UTF-8 BOM**：原始碼含中文字元常值，`csc /codepage:65001` 需搭配 UTF-8（含 BOM）原始檔方能正確編譯，否則中文字串常值編碼誤判；`scripts\build.ps1` 落檔採 UTF-8 BOM，編譯參數明確加 `/codepage:65001`（NFR-02）。
2. **PostMessageW ANSI 綁定問號化**：`PostMessage` 之 `DllImport` 預設 ANSI 綁定，會把 CJK `WM_CHAR` 字元經代碼頁轉為問號；修正為明確綁定 `EntryPoint = "PostMessageW"`（DD-6）。
3. **SendInput 遭注音 IME 組字攔截改類別路由**：本機實測 `SendInput`（KEYEVENTF_UNICODE）在注音 IME 開啟時被組字層攔截延後提交（記事本 Edit 控制項驗證）；修正為 DD-4 類別路由，傳統 IMM 控制項改走 WM_CHAR 直遞，繞過 IME 組字層。

### 對抗審查

1. 對抗審查 3 視角（interop／spec／robust，Opus 審查）共提出 16 項發現，經 3 人反駁表決確認 5 項，組成與處置如下：
   - WM_CHAR 靜默忽略問題（1 項，表決 3/3）：Chromium 等 TSF 應用之 `PostMessage` 回傳 TRUE 僅代表訊息入佇列、不代表已處理，可能被靜默忽略且不觸發後備；修正為 DD-4 類別路由，僅傳統 IMM 控制項走 WM_CHAR，其餘走 SendInput。
   - Esc 吞鍵風險（3 項，三視角各自提出、同一根因，表決 3/3、2/3、2/3）：視窗顯示期間裸 Esc 全域熱鍵會攔截前景應用程式之 Esc（IME 組字取消、關閉選單等），該按鍵會被吃掉一次並隱藏面板。裁決保留（R2，未修正）：Boss_Prompt 明定 Esc 關閉、不搶焦點約束（FR-006）下無 KeyPreview 替代路徑，已於文件揭露供老闆知悉。
   - dist 目錄建立（1 項，表決 3/3）：`csc.exe` 不會自行建立輸出目錄，乾淨檢出時 `dist\` 不存在會導致編譯失敗；`build.ps1` 加入 `New-Item -ItemType Directory -Force` 前置建立（NFR-02）。
   - 餘 11 項發現經表決否決，不列入本版變更。
2. 註：「排錯」第 2 項（PostMessageW ANSI 綁定）與第 3 項（SendInput 遭 IME 攔截）為審查前之實測修正，非審查確認項；審查確認之 WM_CHAR 靜默忽略問題與排錯第 3 項共同構成 DD-4 類別路由之完整依據。

### 文件

1. 六件文件（INDEX、PRD、SPEC、SRS、MODIFY_LOG、專案 CLAUDE.md）由平行撰寫產出，另經逐檔 fresh-context 驗收與跨文件一致性審查。
2. 驗收修正（同日完成）：INDEX 之 CLAUDE.md 與 DOC 四檔「待建立」陳舊標示改為實況、建置參數補全；PRD 對照文件時效句更新；SPEC 補 Mutex 名稱字面值、單一實例提示訊息改逐字引用；SRS 檔首 SPEC 對照聲明更新、FR 表新增「SPEC 對照」欄並將欄名對齊範本「驗收條件」；`Program.cs` 檔頭註解由單一 SendInput 敘述更新為 DD-4 類別路由敘述。

### 驗證

1. V1 編譯：csc 0 錯誤。
2. V2 啟動煙霧測試：程序回應、主視窗存在且標題「標點符號輸入工具」正確、無熱鍵註冊失敗警告視窗。
3. V3 WM_CHAR 投遞：背景記事本 Edit 控制項全 11 符號依序完整到達（PASS）。
4. V4 Esc 隱藏：keybd_event 注入 Esc，視窗實測隱藏（PASS）。
5. V5 Ctrl + / 呼叫：keybd_event 注入組合鍵，視窗實測重現（PASS）。
6. V6 對抗審查：3 視角（interop／spec／robust）Opus 審查共 16 項發現，3 人反駁表決確認 5 項（已修正或裁決文件化）、否決 11 項。
7. V7 待驗證（老闆實機）：實際滑鼠點擊送字至常用應用程式（Obsidian、瀏覽器、Word、LINE）之路由正確性。

### 版號

1. 版本 v1.0；manifest assembly version 1.0.0.0（`src\app.manifest` 第 3 行）。
2. 建置產出 `dist\PunctInput.exe`，14,848 bytes（2026-07-11 實查確認）。

---

*本文件為專案異動摘要紀錄，修改時請一併核對 SPEC／SRS 是否同步。*
