# PunctInput 異動摘要紀錄（MODIFY_LOG）

**規則**：最新版在上；每版記錄變更項目與影響範圍。

---

## v1.4.2（2026-07-11，Aphy 分支）——列結構改版：1 列 11 項與獨立列

### 觸發

1. 老闆 Boss_Prompt（[look rs]）指示三項：A. 改成 1 個 row 有 11 個 item；B. ✿ ❀ 排序改接在 ⛤ 後面；C. 圈號 ⓪ 至 ⑩ 與點號 ⒈ 至 ⒑ 各自獨立 row。

### 程式

1. `src\Program.cs`：符號定義由流水陣列 `Symbols` 改為列結構陣列 `SymbolRows`（獨立列語意無法以流水折行表達）——5 列固定結構：第 1 列基礎 7 鍵＋〔〕﹝﹞←→（11 鍵）、第 2 列 ➤ 至 ⛤（11 鍵）、第 3 列 ✿ ❀（2 鍵）、第 4 列 ⓪ 至 ⑩（11 鍵，獨立列）、第 5 列 ⒈ 至 ⒑（10 鍵，獨立列）；`GRID_COLS` 10 → 11（列容量上限）；`BuildButtonGrid` 改巢狀迴圈按列建鍵，不足之欄留空。
2. `src\app.manifest`：assemblyIdentity version 1.4.1.0 → 1.4.2.0。

### 驗證

1. V13：45 鍵逐列座標比對——5 列組成（11／11／2／11／10）與各列鍵序全數 PASS；視窗實測 880 × 407 px（96 DPI）；Ctrl + Alt + / 切換隱藏／重現皆 PASS。
2. 建置：csc 0 錯誤，`dist\PunctInput_Aphy.exe` 17,920 bytes；測後以新版接續執行。
3. 排錯紀錄：首次建置因 v1.4.1 實例持有檔案鎖失敗（測試腳本停程序步驟晚於建置，順序修正後通過）；列驗證初版以 hashtable 供 `Group-Object` 分組失敗（PowerShell 5.1 不解析 hashtable 鍵為屬性），改 `PSCustomObject` 後正確分列。
4. 老闆實機驗證通過（2026-07-11 回報原文「實機驗證通過」）；老闆同時裁決本機預設維持 master 版（開機自啟不改）。

### 版號

1. Aphy 分支 v1.4.2；manifest assembly version 1.4.2.0。

---

## v1.4.1（2026-07-11，Aphy 分支）——1 列 10 項

### 觸發

1. 老闆 Boss_Prompt（[look rs]）指示：Aphy 版改成 1 個 row 有 10 個 item（同輪另項「安裝前先解除舊版」見下方 cherry-pick 註記區塊）。

### 程式

1. `src\Program.cs`：`GRID_COLS` 4 → 10（45 鍵 = 10 欄 5 列，末列 5 鍵：⒏ ⒐ ⒑ ✿ ❀）；列數與視窗尺寸沿用符號數推導，無其他邏輯異動。
2. `src\app.manifest`：assemblyIdentity version 1.4.0.0 → 1.4.1.0。

### 驗證

1. V12：按鍵枚舉 45 鍵碼位全對（MISSING 無、EXTRA 無）；視窗實測 802 × 407 px（96 DPI，10 欄 5 列，較 v1.4 之 334 × 841 更合螢幕比例，v1.4 之 125% 縮放高度疑慮解除）；Ctrl + Alt + / 切換隱藏／重現皆 PASS。
2. 建置：csc 0 錯誤，`dist\PunctInput_Aphy.exe` 17,920 bytes；測後以新版接續執行。
3. 處置紀錄：本輪建置時發現老闆正試用中的 v1.4 實例（`PunctInput_Aphy`，19:32 啟動）持有檔案鎖與 Mutex，且稍早測試腳本誤啟之 master 安裝版卡在單一實例對話框；兩者清理後重建，以 v1.4.1 接續老闆試用狀態。

### 版號

1. Aphy 分支 v1.4.1；manifest assembly version 1.4.1.0。

---

## 安裝流程改版註記（2026-07-11，自 master `2d422c2` cherry-pick）——程式本體無異動

### 觸發

1. 老闆 Boss_Prompt（[look rs]）指示：Release 安裝時先判斷是否已有安裝舊版檔案，有則先解除安裝；master 與 Branch 都要。

### 變更

1. `scripts\install.ps1`：部署前新增偵測步驟——安裝目錄或任一捷徑存在即先呼叫 `uninstall.ps1` 完整解除舊版，再進行部署；本分支之來源檔名 `dist\PunctInput_Aphy.exe` 於合併時保留。

### 驗證

1. 腳本邏輯與 master 完全相同；master 端已實測 PASS（偵測既有安裝 → 三項舊件全移除 → 重新部署 → 啟動）。本分支安裝與 master 共用目的地與 Mutex，於老闆本機重複實測會取代預設之 master 安裝，故不重測。

---

## v1.4（2026-07-11，Aphy 分支）——符號集擴充

> 本區塊僅存在於 `Aphy` 分支；master 維持 v1.3。

### 觸發

1. 老闆 Boss_Prompt（[look rs]）指示：開新分支「Aphy」，新增「new item」區塊符號——成對 2 組（〔〕 U+3014 U+3015、﹝﹞ U+FE5D U+FE5E）、單一 36 個（含箭頭、愛心、勾選、圈號 ⓪ 至 ⑩、點號 ⒈ 至 ⒑、花卉；清單第 5 項之 Facebook 表情圖檔實體為 ♥ U+2665，圖檔 URL 內含碼位 2665 佐證）。

### 程式

1. `src\Program.cs`：`Symbols` 陣列由 7 鍵擴為 45 鍵（基礎 7 鍵保持原位，新符號依 Boss_Prompt 列出順序附加：先成對 2 鍵、後單一 36 鍵）；新增 `GRID_COLS` 常數（維持 v1.2「1 列 4 項」裁決），按鍵格列數與視窗高度改由符號數推導（45 鍵 = 4 欄 12 列，末列 1 鍵）。
2. `src\app.manifest`：assemblyIdentity version 1.3.0.0 → 1.4.0.0（Aphy 分支版號線）。
3. 送字層無異動：全部新符號皆為 BMP 單碼元（成對鍵 2 碼元），沿用既有三路路由。

### 驗證

1. 按鍵枚舉：45 鍵組成與預期碼位清單完全吻合（MISSING 無、EXTRA 無）；Ctrl + Alt + / 切換 PASS。
2. 視窗尺寸實測 334 × 841 px（96 DPI）。註記：於 125% 顯示縮放約 1,051 px 高，接近 1080p 工作區極限；若過高，後續可裁決改多欄或縮小鍵高。
3. 建置：csc 0 錯誤，`dist\PunctInput.exe` 17,920 bytes；測試後已恢復老闆已安裝之 master v1.3 執行。
4. 觀察：exe 之 FileVersionInfo 顯示 0.0.0.0（manifest 版號不落入 Win32 版本資源；master 亦同），如需檔案總管可見版號，後續可於原始碼補 assembly 屬性。

### 版號

1. Aphy 分支 v1.4；manifest assembly version 1.4.0.0。

### 建置產出分流（2026-07-11 老闆指示補充）

1. 老闆裁決：本機預設 master 版；`dist\` 分兩檔——`PunctInput.exe`＝master 版、`PunctInput_Aphy.exe`＝Aphy 版。
2. 變更：Aphy 分支之 `build.ps1` 輸出改為 `dist\PunctInput_Aphy.exe`（master 分支維持 `dist\PunctInput.exe`，兩分支建置互不覆蓋）；`install.ps1` 來源同步改名，並註記安裝目的地與 Mutex 與 master 共用、安裝 Aphy 版會取代既有安裝。
3. `dist\` 於 `.gitignore` 排除，雙檔僅存於本機工作目錄。

### 發布（2026-07-11 老闆指示）

1. GitHub Release「PunctInput v1.4（Aphy 版）」：tag `v1.4-Aphy` 指向 Aphy 分支，附 `PunctInput_Aphy.exe`（17,920 bytes，assembly 1.4.0.0）為 release asset；以 `--latest=false` 發布，repo「Latest」徽章維持 master 之 v1.3（老闆本機預設 master 版）。
2. 附帶修正：本分支 `Program.cs` 檔頭註解補 DD-9 三路路由敘述（分支點早於 master 之同項修正 `79fa4e8`，故需各自修正）。

---

## 安裝工具與 GitHub 發布註記（2026-07-11）——程式本體無異動，版號維持 v1.3

### 觸發

1. 老闆指示：create install tool；完成之後 push to github。

### 變更

1. 新增 `scripts\install.ps1`（FR-014）：部署 exe 至 `%LOCALAPPDATA%\Programs\PunctInput\`、建立開始功能表捷徑與開機自啟捷徑（Startup 資料夾）並啟動；選項 `-NoStartup`（略過自啟）、`-NoLaunch`（不啟動）；`dist\` 缺檔時自動先建置。UTF-8 BOM。
2. 新增 `scripts\uninstall.ps1`（FR-014）：停止程序、移除兩處捷徑與安裝目錄；不動原始碼與 `dist\`。UTF-8 BOM。
3. 界線註記：PRD 5.2 原將「開機自啟」列為範圍外（v1.0 當時事實清單未列）；本次老闆指示安裝工具，開機自啟為其核心價值，隨 FR-014 納入（以 `-NoStartup` 保留退出口）。
4. GitHub 發布：以 gh CLI（帳號 LouisChao0728）建立 repo 並推送完整 commit 歷史；細節見本節「發布」。

### 驗證

1. 實跑 `install.ps1`：部署 17,408 bytes、兩捷徑建立且目標均指向安裝位置、程序自安裝位置啟動（PASS）。
2. 實跑 `uninstall.ps1`：安裝目錄與兩捷徑全數移除，Test-Path 驗證皆 False（PASS）。
3. 重跑 `install.ps1`：重裝成功，最終狀態為已安裝並執行中（PASS）。

### 發布

1. Repo：`LouisChao0728/Punctuation_Input_Tool`；推送分支 master，含 v1.0 至 v1.3 全部 commit 歷史。初始建為 private，同日老闆指示改 public（gh CLI 實查 `visibility: PUBLIC`）。
2. README.md 補檔（同日老闆指示，repo 首頁原為空）：功能特色、符號清單、安裝與免安裝執行、操作對照、環境需求、文件地圖、已知限制與疑難排解；INDEX 檔案職責同步登記。
3. GitHub Release v1.3（同日老闆指示）：tag `v1.3`，附 `PunctInput.exe`（17,408 bytes，assembly 1.3.0.0）為 release asset，供免建置直接下載；release notes 彙整 v1.0 至 v1.3 沿革。`dist\` 於 `.gitignore` 排除，release asset 為執行檔的正式散布管道。

---

## v1.3（2026-07-11）——剪貼簿中轉繞過輸入法組字區

### 觸發

1. 老闆實機回報：鍵序 1 至 5（「」『』《》【】：，皆 CJK 標點）點擊輸入時呈現「預編譯狀態」（輸入法組字區），鍵序 6、7（●█，非 CJK）正常直接輸入；指示比照鍵序 6、7。
2. 根因：非 EDIT 目標走 SendInput 路徑時，VK_PACKET 之 CJK 區段字元被注音 IME 攔入組字區；非 CJK 字元不受攔截。與 v1.0 開發期記事本觀察到的攔截行為同根因（當時僅對 EDIT 類以 WM_CHAR 繞過，未覆蓋 TSF 應用）。

### 裁決

1. DD-9 非 EDIT／非主控台目標之送字改「剪貼簿中轉自動貼上」（老闆三選項裁決選 1；候選「WM_CHAR 直遞擴大至全部目標」「暫切英文鍵盤佈局」落選）。
2. 老闆前置條件：執行前確認 git 已保存現版——已確認工作樹乾淨、v1.2 完整入庫（commit `1661d74`），回退指令 `git reset --hard 1661d74`。
3. 界線註記：DD-2 否決的是「手動剪貼簿」使用者體驗；DD-9 為內部傳輸機制，點擊行為不變（自動貼上）。

### 程式

1. `src\Program.cs`：
   - `SendSymbolToTarget` 改三路路由：EDIT 類 WM_CHAR（不變）→ 主控台 SendInput（不變）→ 其餘改 `SendViaClipboardPaste`。
   - 新增剪貼簿中轉機制：`SnapshotClipboard`（逐格式盡力快照）、`Clipboard.SetDataObject` 置入符號、`SendCtrlV`（送出前以 `GetAsyncKeyState` 檢測並釋放按住的 Shift／Alt／Win）、500 ms 計時器 `RestoreClipboardBackup` 延遲還原；連續點擊期間不重拍快照；程序結束時補執行未完成之還原。
   - 後備：剪貼簿設定失敗（被鎖定等）退回 `SendUnicodeString`；除錯日誌新增 `route=ClipboardPaste` 與還原紀錄。
2. `src\app.manifest`：assemblyIdentity version 1.2.0.0 → 1.3.0.0。

### 文件

1. 活文件同步：INDEX（版本、路由敘述、行數 741、exe 17,408 bytes）、PRD（目標 3、範圍 5.1.3、DD-9 補登、R4 改寫、R6 新增）、SPEC（§3.1、§7 三路決策樹與新 §7.6、§7.5 日誌欄位、§9、R4／R6、§12 DD-9、V10、沿革）、SRS（FR-004、FR-005、沿革）、專案 CLAUDE.md Rule 5 路由規則改寫。

### 驗證

1. V10：剪貼簿路徑端對端實測——WPF TextBox（HwndWrapper 類，非 EDIT）為目標，點擊 「」／：／● 三鍵，文字完整定稿到達 `「」：●`（無組字狀態），剪貼簿於還原計時器後回復原始標記內容，兩項皆 PASS。
2. 建置：csc 0 錯誤，`dist\PunctInput.exe` 17,408 bytes（2026-07-11）；測後保留執行實例供老闆直接使用。
3. 老闆實機複測項：於原回報發生預編譯狀態之應用程式點擊鍵序 1 至 5，確認送出即定稿。

### 版號

1. v1.3；manifest assembly version 1.3.0.0。

---

## v1.2（2026-07-11）——括號成組、4 欄配置、數字鍵盤鍵位

### 觸發

1. 老闆 Boss_Prompt（[LOOK RS]）指示三項調整：A. 括號改成組（「」『』《》【】點一下直接成對輸入，不再個別點選）；B. 1 個 ROW 改成 4 個項目；C. 快捷鍵 Ctrl + Alt + / 之 / 要包含數字鍵盤區域的 /。

### 程式

1. `src\Program.cs`：
   - `Symbols` 陣列 11 個單符號改為 7 鍵（「」『』《》【】4 組 + ：●█ 3 單符號）；送字路徑（`SendSymbolToTarget`／`SendUnicodeString`）原生支援多碼元字串，逐碼元送出，無需改動。
   - 按鍵格 `cols` 3 改 4，`ClientSize` 改寬 `Scale(4*78+16)`、高 `Scale(48+2*62+20)`（4 欄 2 列）。
   - 新增 `VK_DIVIDE`(0x6F) 與 `HOTKEY_ID_TOGGLE_NUM`＝3：數字鍵盤 / 與主鍵盤 / 雙註冊，`WndProc` 對 ID 1 或 3 一致執行 `TogglePad()`；任一鍵位註冊失敗時警告訊息列出失敗鍵位。
2. `src\app.manifest`：assemblyIdentity version 1.1.0.0 → 1.2.0.0。

### 文件

1. 活文件同步：INDEX（對應版本、快捷鍵對照、按鍵表、Program.cs 行數 577、文件地圖）、PRD（目標判準、範圍 5.1）、SPEC（概觀、§3.1、§4.1、§4.3、§5 符號契約重寫為 7 鍵表、§6.1 熱鍵表增數字鍵盤列、§6.2、§6.3、§9、§13、V9 新增、沿革）、SRS（FR-001、FR-003、FR-012、沿革）。

### 驗證

1. V9：按鍵枚舉 7 鍵組成與文字全對（「」『』《》【】：●█）；`keybd_event` 主鍵盤 VK_OEM_2 與數字鍵盤 VK_DIVIDE 切換隱藏／重現皆 PASS；Esc 隱藏 PASS。成組送字沿用已驗證之逐碼元送出迴圈（V3 曾以 11 碼元連續投遞驗證），實際點擊由老闆使用時確認。
2. 建置：csc 0 錯誤，`dist\PunctInput.exe` 14,848 bytes（2026-07-11）；測後保留執行實例供老闆直接使用。

### 版號

1. v1.2；manifest assembly version 1.2.0.0。

---

## v1.1（2026-07-11）——呼叫快捷鍵改版

### 觸發

1. 老闆回饋（2026-07-11）：R2 可接受；V7 實機驗證通過；R1 之 Ctrl + / 熱鍵實務上有使用困難，指示搭配可以使用的快捷鍵。

### 裁決

1. DD-8 呼叫快捷鍵改為 Ctrl + Alt + /（老闆委任選鍵）：保留原 / 鍵位記憶（同鍵加按 Alt）；主流應用（VS Code、Word、Chrome、Edge、Obsidian、LINE）無此預設綁定、Windows 系統未保留此組合，衝突面實質歸零。原 Ctrl + / 不再註冊，鍵位歸還給各應用程式。

### 程式

1. `src\Program.cs`：新增 `MOD_ALT`(0x0001) 常數；`HOTKEY_ID_TOGGLE` 註冊參數改為 `MOD_CONTROL | MOD_ALT | MOD_NOREPEAT` + `VK_OEM_2`；同步更新五處文案（檔頭註解、單一實例提示、系統匣提示文字、右鍵選單、註冊失敗警告）與常數註解。
2. `src\app.manifest`：assemblyIdentity version 1.0.0.0 → 1.1.0.0。

### 文件

1. 活文件同步：INDEX（概觀版本、快捷鍵對照、文件地圖）、PRD（目標判準、使用者故事、範圍、DD-8 補登、DD-3 措辭、R1 緩解、R2 採認、R4 解除、交接考量結案）、SPEC（概觀、§4.5、§6.1、§6.2、§9、R1／R2／R4、§12 DD-8、§13、V7 結案、V8 新增、沿革）、SRS（FR-001、FR-009、FR-011、FR-012、沿革）。
2. 本 v1.0 區塊之 Ctrl + / 敘述屬歷史紀錄（point-in-time），不回改。

### 驗證

1. V7 結案：老闆 2026-07-11 實機驗證通過（回報原文「實機驗證通過」）。
2. V8 新增：v1.1 建置後 `keybd_event` 實測——Ctrl + Alt + / 切換隱藏（PASS）、切換重現（PASS）、Esc 隱藏（PASS）；測後保留執行實例供老闆直接使用。
3. 建置：csc 0 錯誤，`dist\PunctInput.exe` 14,848 bytes（2026-07-11）。

### 版號

1. v1.1；manifest assembly version 1.1.0.0。

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
