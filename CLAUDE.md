# PunctInput 專案規則（Project Rule）

**進行任何異動之前，請先閱讀本文件。**

---

## 適用對象

本規則適用於所有參與 PunctInput（標點符號輸入工具）專案開發、維護或修改的人員與 AI 助理。

---

## Rule 1：文件體系與異動基準

**本專案文件位於 `DOC/`，異動時以 SPEC 為基準，再同步至 SRS。**

| 文件 | 說明 |
|------|------|
| `DOC/00_PunctInput_INDEX.md` | 專案索引（結構、檔案職責、文件地圖之單一入口） |
| `DOC/01_PunctInput_PRD.md` | 產品需求文件（產品定位、範圍、設計決策紀錄） |
| `DOC/02_PunctInput_SPEC_v1.0.md` | 專案規格書（**實作與改動的基準**；版號依實際最新版） |
| `DOC/03_PunctInput_SRS_v1.0.md` | 軟體需求規格書（FR-xxx / NFR-xx 可測試需求與驗收條件） |
| `DOC/04_PunctInput_MODIFY_LOG.md` | 異動摘要紀錄（最新版在上） |

- 兩份規格內容不一致時，**以 SPEC（目前最新版）為準**。
- 每次程式碼異動前，必須先判斷本次異動是否影響功能、介面、送字路由或行為；若影響，同一次作業中同步更新 SPEC 與 SRS。

## Rule 2：異動摘要紀錄

任何程式或文件異動完成後，將異動摘要寫入 `DOC/04_PunctInput_MODIFY_LOG.md` 對應版本區段（最新版在上）。

## Rule 3：異動範圍與權限

本專案由老闆 Boss_Prompt 於 2026-07-11 指示建立。`C:\Users\user\Claude_WorkSpace\Punctuation_Input_Tool` 並非 Global Rules 第十節 1.1 條所列之「完全權限路徑」，本專案內之任何異動須依老闆當次指示執行，**禁止聲稱或比照全權開發授權**。涉及第二節第 7 條「歧義處理」情境者，依該條列選項向老闆裁決。

## Rule 4：版本作業檢核清單

每次版本作業（需求開發、Bug Fix）須依序完成：

1. 程式碼實作（`src\Program.cs`；C# 5 語言層級相容，見 Rule 6）。
2. 建置驗證：實跑 `powershell -ExecutionPolicy Bypass -File scripts\build.ps1`，確認 csc 0 錯誤、`dist\PunctInput.exe` 產出。
3. SPEC / SRS 同步（依 Rule 1 判斷）。
4. 更新 `04_PunctInput_MODIFY_LOG.md`。
5. 版號遞增：`src\app.manifest` 內 `assemblyIdentity version`（現行 `1.0.0.0`）。
6. 視情況更新 `00_PunctInput_INDEX.md`（涉及 src 檔案增刪、DOC 清單異動時）。
7. Git commit（含上述所有變更）。

## Rule 5：送字路由特別義務

本工具送字邏輯（`Program.cs` 內 `SendSymbolToTarget` 方法）依 DD-4 裁決，依焦點控制項類別名分流：類別名含 `EDIT`（不分大小寫）者走 `WM_CHAR` 直遞（`PostMessageW`），其餘目標走 `SendInput`（`KEYEVENTF_UNICODE`），API 失敗時後備 `SendInput`（對應 FR-004、FR-005）。

**修改 `SendSymbolToTarget` 或其路由判準時，必須：**

1. 同步更新 SPEC 文件之「送字策略」章節，反映最新路由邏輯與判準。
2. 設定環境變數 `PUNCTINPUT_DEBUG=1` 後實機測試，以 `%TEMP%\PunctInput_debug.log`（FR-013）記錄之路由結果（route、focus handle、控制項類別、投遞結果）作為修改生效之實測證據，不得僅以程式碼審閱替代實測。

## Rule 6：建置環境

1. 編譯器路徑固定：`C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe`（Windows 內建 .NET Framework 4.x runtime 隨附，非 .NET SDK）；本機無 .NET SDK 與 AutoHotkey，不得改用需額外安裝之工具鏈。
2. C# 語言層級上限為 C# 5（csc 4.0.30319 限制）：**禁止**使用字串插值（`$"..."`）、null 條件運算子（`?.`）、`nameof` 等 C# 6 以上語法。
3. `scripts\*.ps1` 檔須維持 UTF-8 BOM 編碼。
4. `src\Program.cs` 原始碼為 UTF-8，編譯時須帶 `/codepage:65001` 參數（已寫入 `build.ps1`），確保中文字串常值正確編譯。
5. 建置指令：`/nologo /codepage:65001 /target:winexe /platform:anycpu /optimize+ /win32manifest:"src\app.manifest" /r:System.dll /r:System.Drawing.dll /r:System.Windows.Forms.dll /out:"dist\PunctInput.exe" "src\Program.cs"`（NFR-02）。乾淨檢出時 `dist\` 資料夾不存在，`build.ps1` 會自動建立。

---

*本文件為專案共識，修改時請一併更新 MODIFY_LOG。*
