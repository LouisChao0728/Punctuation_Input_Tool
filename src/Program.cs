// =====================================================================
// PunctInput：PC 用標點符號輸入工具
// UI 比照 Windows 小算盤；點擊符號按鈕即依類別三路路由（DD-4／DD-9）送字
// 至前景應用程式的焦點控制項：類別名含 EDIT 者走 WM_CHAR（PostMessageW）
// 直遞、主控台走 SendInput（KEYEVENTF_UNICODE）、其餘目標走剪貼簿中轉
// 自動貼上；WM_CHAR 投遞失敗或剪貼簿設定失敗時後備 SendInput。
// 工具視窗採 WS_EX_NOACTIVATE，點擊不搶焦點。
// 快捷鍵：Ctrl + Alt + / 呼叫（顯示／隱藏切換；主鍵盤與數字鍵盤之 / 皆可）；
// Esc 關閉視窗（隱藏至系統匣）。
// 建置：Windows 內建 .NET Framework csc.exe，語言層級 C# 5。
// 規格基準：DOC\02_PunctInput_SPEC_v1.0.md（送字策略見第七章）。
// =====================================================================
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace PunctInput
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            bool createdNew;
            using (Mutex mutex = new Mutex(true, "PunctInput_SingleInstance_Mutex", out createdNew))
            {
                if (!createdNew)
                {
                    MessageBox.Show(
                        "標點符號輸入工具已在執行中，請以 Ctrl + Alt + / 呼叫。",
                        PunctPadForm.AppTitle,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new PunctPadForm());
            }
        }
    }

    internal sealed class PunctPadForm : Form
    {
        // 視窗標題：標點符號輸入工具
        public const string AppTitle = "標點符號輸入工具";

        // ---- Win32 常數 ----
        private const int WM_HOTKEY = 0x0312;
        private const int WM_MOUSEACTIVATE = 0x0021;
        private const int WM_CHAR = 0x0102;
        private const int MA_NOACTIVATE = 3;

        private const uint MOD_ALT = 0x0001;
        private const uint MOD_CONTROL = 0x0002;
        private const uint MOD_NOREPEAT = 0x4000;
        private const uint VK_OEM_2 = 0xBF;      // 主鍵盤「/ ?」鍵
        private const uint VK_DIVIDE = 0x6F;     // 數字鍵盤「/」鍵
        private const uint VK_ESCAPE = 0x1B;

        private const int HOTKEY_ID_TOGGLE = 1;      // Ctrl + Alt + /（主鍵盤）：顯示／隱藏切換
        private const int HOTKEY_ID_ESC = 2;         // Esc：僅於視窗顯示期間註冊
        private const int HOTKEY_ID_TOGGLE_NUM = 3;  // Ctrl + Alt + /（數字鍵盤）：同切換

        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        private const uint INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
        private const uint KEYEVENTF_UNICODE = 0x0004;

        private const ushort VK_CONTROL = 0x11;
        private const ushort VK_SHIFT = 0x10;
        private const ushort VK_MENU = 0x12;   // Alt
        private const ushort VK_LWIN = 0x5B;
        private const ushort VK_RWIN = 0x5C;
        private const ushort VK_V = 0x56;

        // ---- 符號清單（Boss_Prompt 指定順序；v1.2 起括號成組一鍵成對輸入）----
        // 「」 『』 《》 【】 ： ● █（4 組括號 + 3 個單符號，共 7 鍵）
        private static readonly string[] Symbols = new string[]
        {
            "「」", "『』", "《》", "【】",
            "：", "●", "█"
        };

        private NotifyIcon _trayIcon;
        private Label _display;
        private bool _exiting;
        private bool _toggleHotkeyOk;
        private bool _toggleNumHotkeyOk;
        private bool _escHotkeyRegistered;

        // 剪貼簿中轉狀態（DD-9）：備份使用者剪貼簿，貼上後延遲還原
        private IDataObject _clipboardBackup;
        private bool _restorePending;
        private System.Windows.Forms.Timer _restoreTimer;

        public PunctPadForm()
        {
            float scale = GetDpiScale();

            Text = AppTitle;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            TopMost = true;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(230, 230, 230);
            Font = new Font("Segoe UI", 9F);
            ClientSize = new Size(Scale(4 * 78 + 16, scale), Scale(48 + 2 * 62 + 20, scale));

            BuildDisplay(scale);
            BuildButtonGrid(scale);
            BuildTrayIcon();

            _restoreTimer = new System.Windows.Forms.Timer();
            _restoreTimer.Interval = 500;
            _restoreTimer.Tick += OnRestoreTimerTick;
        }

        // ---- UI 建置（比照小算盤：上方顯示區 + 下方按鍵格）----

        private void BuildDisplay(float scale)
        {
            _display = new Label();
            _display.Text = "";
            _display.TextAlign = ContentAlignment.MiddleRight;
            _display.Font = new Font("Segoe UI", 20F, FontStyle.Regular);
            _display.BackColor = BackColor;
            _display.ForeColor = Color.FromArgb(32, 32, 32);
            _display.Location = new Point(Scale(8, scale), Scale(6, scale));
            _display.Size = new Size(ClientSize.Width - Scale(16, scale), Scale(42, scale));
            Controls.Add(_display);
        }

        private void BuildButtonGrid(float scale)
        {
            int cols = 4;
            int btnW = Scale(74, scale);
            int btnH = Scale(56, scale);
            int gap = Scale(4, scale);
            int left = Scale(8, scale);
            int top = Scale(52, scale);

            for (int i = 0; i < Symbols.Length; i++)
            {
                Button b = new Button();
                b.Text = Symbols[i];
                b.Tag = Symbols[i];
                b.Font = new Font("Segoe UI", 16F, FontStyle.Regular);
                b.Size = new Size(btnW, btnH);
                b.Location = new Point(left + (i % cols) * (btnW + gap), top + (i / cols) * (btnH + gap));
                b.FlatStyle = FlatStyle.Flat;
                b.BackColor = Color.FromArgb(250, 250, 250);
                b.ForeColor = Color.FromArgb(32, 32, 32);
                b.FlatAppearance.BorderColor = Color.FromArgb(230, 230, 230);
                b.FlatAppearance.BorderSize = 1;
                b.FlatAppearance.MouseOverBackColor = Color.FromArgb(218, 218, 218);
                b.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, 200, 200);
                b.TabStop = false;
                b.Click += OnSymbolClick;
                Controls.Add(b);
            }
        }

        private void BuildTrayIcon()
        {
            ContextMenuStrip menu = new ContextMenuStrip();

            ToolStripMenuItem miToggle = new ToolStripMenuItem(
                "顯示／隱藏（Ctrl + Alt + /）");
            miToggle.Click += delegate(object s, EventArgs e) { TogglePad(); };
            menu.Items.Add(miToggle);

            menu.Items.Add(new ToolStripSeparator());

            // 結束
            ToolStripMenuItem miExit = new ToolStripMenuItem("結束");
            miExit.Click += delegate(object s, EventArgs e) { ExitApp(); };
            menu.Items.Add(miExit);

            _trayIcon = new NotifyIcon();
            _trayIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            _trayIcon.Text = AppTitle + "（Ctrl + Alt + / 呼叫）";
            _trayIcon.ContextMenuStrip = menu;
            _trayIcon.Visible = true;
            _trayIcon.DoubleClick += delegate(object s, EventArgs e) { TogglePad(); };
        }

        private float GetDpiScale()
        {
            using (Graphics g = CreateGraphics())
            {
                return g.DpiX / 96f;
            }
        }

        private static int Scale(int value, float scale)
        {
            return (int)Math.Round(value * scale);
        }

        // ---- 不搶焦點機制 ----

        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOPMOST;
                return cp;
            }
        }

        // ---- 熱鍵註冊 ----

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _toggleHotkeyOk = RegisterHotKey(Handle, HOTKEY_ID_TOGGLE, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, VK_OEM_2);
            _toggleNumHotkeyOk = RegisterHotKey(Handle, HOTKEY_ID_TOGGLE_NUM, MOD_CONTROL | MOD_ALT | MOD_NOREPEAT, VK_DIVIDE);
            if (!_toggleHotkeyOk || !_toggleNumHotkeyOk)
            {
                string failedKeys;
                if (!_toggleHotkeyOk && !_toggleNumHotkeyOk)
                {
                    failedKeys = "主鍵盤與數字鍵盤";
                }
                else if (!_toggleHotkeyOk)
                {
                    failedKeys = "主鍵盤";
                }
                else
                {
                    failedKeys = "數字鍵盤";
                }
                MessageBox.Show(
                    "全域快捷鍵 Ctrl + Alt + /（" + failedKeys + " / 鍵）註冊失敗（被其他程式佔用）。仍可由系統匣圖示操作。",
                    AppTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_toggleHotkeyOk)
            {
                UnregisterHotKey(Handle, HOTKEY_ID_TOGGLE);
                _toggleHotkeyOk = false;
            }
            if (_toggleNumHotkeyOk)
            {
                UnregisterHotKey(Handle, HOTKEY_ID_TOGGLE_NUM);
                _toggleNumHotkeyOk = false;
            }
            UnregisterEscHotkey();
            base.OnHandleDestroyed(e);
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (IsHandleCreated)
            {
                if (Visible)
                {
                    RegisterEscHotkey();
                }
                else
                {
                    UnregisterEscHotkey();
                }
            }
        }

        private void RegisterEscHotkey()
        {
            if (!_escHotkeyRegistered)
            {
                _escHotkeyRegistered = RegisterHotKey(Handle, HOTKEY_ID_ESC, MOD_NOREPEAT, VK_ESCAPE);
            }
        }

        private void UnregisterEscHotkey()
        {
            if (_escHotkeyRegistered)
            {
                UnregisterHotKey(Handle, HOTKEY_ID_ESC);
                _escHotkeyRegistered = false;
            }
        }

        // ---- 視窗訊息處理 ----

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();
                if (id == HOTKEY_ID_TOGGLE || id == HOTKEY_ID_TOGGLE_NUM)
                {
                    TogglePad();
                    return;
                }
                if (id == HOTKEY_ID_ESC)
                {
                    HidePad();
                    return;
                }
            }
            if (m.Msg == WM_MOUSEACTIVATE)
            {
                m.Result = (IntPtr)MA_NOACTIVATE;
                return;
            }
            base.WndProc(ref m);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // 使用者按視窗關閉鈕視同 Esc：隱藏至系統匣，不結束程序。
            if (!_exiting && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                HidePad();
                return;
            }
            if (_restorePending)
            {
                _restoreTimer.Stop();
                _restorePending = false;
                RestoreClipboardBackup();
            }
            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }
            base.OnFormClosing(e);
        }

        // ---- 顯示／隱藏／結束 ----

        private void TogglePad()
        {
            if (Visible)
            {
                HidePad();
            }
            else
            {
                ShowPad();
            }
        }

        private void ShowPad()
        {
            Show();
        }

        private void HidePad()
        {
            Hide();
        }

        private void ExitApp()
        {
            _exiting = true;
            Close();
        }

        // ---- 符號送出 ----

        private void OnSymbolClick(object sender, EventArgs e)
        {
            Button b = sender as Button;
            if (b == null)
            {
                return;
            }
            string symbol = (string)b.Tag;
            _display.Text = symbol;
            SendSymbolToTarget(symbol);
        }

        // 送字策略（類別路由；v1.3 依 DD-9 改版）：
        // 1. 傳統 IMM 控制項（類別名含 EDIT，涵蓋 Edit、RICHEDIT50W、
        //    WindowsForms10.EDIT 等）走 WM_CHAR 直遞：WM_CHAR 攜帶字元字面值
        //    不經 IME，實測無組字問題；投遞失敗時後備 SendInput。
        // 2. 主控台（ConsoleWindowClass）走 SendInput：legacy conhost 之
        //    Ctrl + V 貼上不可靠，維持鍵盤注入。
        // 3. 其餘目標（Chromium / Electron / UWP / Word 等 TSF 應用）走
        //    剪貼簿中轉自動貼上（SendViaClipboardPaste）：SendInput 的
        //    VK_PACKET 對 CJK 區段字元會被注音 IME 攔入組字區（2026-07-11
        //    老闆實機回報「預編譯狀態」，鍵序 1 至 5 中招、非 CJK 之 ●█
        //    直接通過），剪貼簿貼上完全繞過 IME。
        private void SendSymbolToTarget(string text)
        {
            IntPtr fgWin = GetForegroundWindow();
            if (fgWin == IntPtr.Zero || fgWin == Handle)
            {
                DebugLog("SendSymbolToTarget skip: no usable foreground window");
                return;
            }

            uint pid;
            uint tid = GetWindowThreadProcessId(fgWin, out pid);
            GUITHREADINFO gti = new GUITHREADINFO();
            gti.cbSize = Marshal.SizeOf(typeof(GUITHREADINFO));
            IntPtr focus = IntPtr.Zero;
            if (GetGUIThreadInfo(tid, ref gti))
            {
                focus = gti.hwndFocus;
            }
            if (focus == IntPtr.Zero)
            {
                focus = fgWin;
            }

            string cls = GetClassNameOf(focus);
            if (IsClassicEditClass(cls))
            {
                bool allPosted = true;
                for (int i = 0; i < text.Length; i++)
                {
                    if (!PostMessage(focus, WM_CHAR, (IntPtr)text[i], (IntPtr)1))
                    {
                        allPosted = false;
                        break;
                    }
                }
                DebugLog(string.Format(
                    "SendSymbolToTarget route=WM_CHAR text=U+{0:X4} focus=0x{1:X} class={2} posted={3}",
                    (int)text[0], focus.ToInt64(), cls, allPosted));
                if (allPosted)
                {
                    return;
                }
                DebugLog("SendSymbolToTarget route=SendInput (WM_CHAR fallback)");
                SendUnicodeString(text);
                return;
            }
            if (cls == "ConsoleWindowClass")
            {
                DebugLog(string.Format(
                    "SendSymbolToTarget route=SendInput (console) text=U+{0:X4} focus=0x{1:X}",
                    (int)text[0], focus.ToInt64()));
                SendUnicodeString(text);
                return;
            }
            DebugLog(string.Format(
                "SendSymbolToTarget route=ClipboardPaste text=U+{0:X4} focus=0x{1:X} class={2}",
                (int)text[0], focus.ToInt64(), cls));
            SendViaClipboardPaste(text);
        }

        // ---- 剪貼簿中轉（DD-9）----

        private void SendViaClipboardPaste(string text)
        {
            // 還原尚未執行時不重拍快照，保住使用者原始剪貼簿內容
            if (!_restorePending)
            {
                _clipboardBackup = SnapshotClipboard();
            }
            _restoreTimer.Stop();
            try
            {
                Clipboard.SetDataObject(text, true);
            }
            catch (Exception ex)
            {
                DebugLog("clipboard set failed: " + ex.Message + ", fallback SendInput");
                if (!_restorePending)
                {
                    _clipboardBackup = null;
                }
                SendUnicodeString(text);
                return;
            }
            _restorePending = true;
            SendCtrlV();
            _restoreTimer.Start();
        }

        private void OnRestoreTimerTick(object sender, EventArgs e)
        {
            _restoreTimer.Stop();
            _restorePending = false;
            RestoreClipboardBackup();
        }

        private void RestoreClipboardBackup()
        {
            try
            {
                if (_clipboardBackup != null)
                {
                    Clipboard.SetDataObject(_clipboardBackup, true);
                }
                else
                {
                    Clipboard.Clear();
                }
                DebugLog("clipboard restored");
            }
            catch (Exception ex)
            {
                DebugLog("clipboard restore failed: " + ex.Message);
            }
            _clipboardBackup = null;
        }

        // 盡力快照剪貼簿全部格式；個別格式取出失敗時跳過該格式
        private static IDataObject SnapshotClipboard()
        {
            try
            {
                IDataObject src = Clipboard.GetDataObject();
                if (src == null)
                {
                    return null;
                }
                string[] formats = src.GetFormats(false);
                DataObject copy = new DataObject();
                int copied = 0;
                for (int i = 0; i < formats.Length; i++)
                {
                    try
                    {
                        object data = src.GetData(formats[i], false);
                        if (data != null)
                        {
                            copy.SetData(formats[i], false, data);
                            copied++;
                        }
                    }
                    catch (Exception)
                    {
                        // 該格式無法複製（COM 代理等），跳過
                    }
                }
                return copied > 0 ? (IDataObject)copy : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        // 送出 Ctrl + V；先釋放使用者可能按住的 Shift / Alt / Win，
        // 避免組合成 Ctrl + Shift + V 等變體
        private static void SendCtrlV()
        {
            ReleaseModifierIfDown(VK_SHIFT);
            ReleaseModifierIfDown(VK_MENU);
            ReleaseModifierIfDown(VK_LWIN);
            ReleaseModifierIfDown(VK_RWIN);

            INPUT[] inputs = new INPUT[4];
            inputs[0].type = INPUT_KEYBOARD;
            inputs[0].U.ki.wVk = VK_CONTROL;
            inputs[1].type = INPUT_KEYBOARD;
            inputs[1].U.ki.wVk = VK_V;
            inputs[2].type = INPUT_KEYBOARD;
            inputs[2].U.ki.wVk = VK_V;
            inputs[2].U.ki.dwFlags = KEYEVENTF_KEYUP;
            inputs[3].type = INPUT_KEYBOARD;
            inputs[3].U.ki.wVk = VK_CONTROL;
            inputs[3].U.ki.dwFlags = KEYEVENTF_KEYUP;
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private static void ReleaseModifierIfDown(ushort vk)
        {
            if ((GetAsyncKeyState(vk) & 0x8000) == 0)
            {
                return;
            }
            INPUT[] up = new INPUT[1];
            up[0].type = INPUT_KEYBOARD;
            up[0].U.ki.wVk = vk;
            up[0].U.ki.dwFlags = KEYEVENTF_KEYUP;
            SendInput(1, up, Marshal.SizeOf(typeof(INPUT)));
        }

        private static bool IsClassicEditClass(string cls)
        {
            return cls.IndexOf("EDIT", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetClassNameOf(IntPtr hwnd)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
            GetClassName(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        private static void SendUnicodeString(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            INPUT[] inputs = new INPUT[text.Length * 2];
            int n = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];

                inputs[n].type = INPUT_KEYBOARD;
                inputs[n].U.ki.wVk = 0;
                inputs[n].U.ki.wScan = ch;
                inputs[n].U.ki.dwFlags = KEYEVENTF_UNICODE;
                inputs[n].U.ki.time = 0;
                inputs[n].U.ki.dwExtraInfo = IntPtr.Zero;
                n++;

                inputs[n].type = INPUT_KEYBOARD;
                inputs[n].U.ki.wVk = 0;
                inputs[n].U.ki.wScan = ch;
                inputs[n].U.ki.dwFlags = KEYEVENTF_UNICODE | KEYEVENTF_KEYUP;
                inputs[n].U.ki.time = 0;
                inputs[n].U.ki.dwExtraInfo = IntPtr.Zero;
                n++;
            }
            uint injected = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            int err = Marshal.GetLastWin32Error();
            DebugLog(string.Format(
                "SendUnicodeString text=U+{0:X4} requested={1} injected={2} lastError={3} foreground=0x{4:X}",
                (int)text[0], inputs.Length, injected, err, GetForegroundWindow().ToInt64()));
        }

        // 除錯日誌：僅於環境變數 PUNCTINPUT_DEBUG=1 時寫入 %TEMP%\PunctInput_debug.log
        private static void DebugLog(string line)
        {
            try
            {
                if (Environment.GetEnvironmentVariable("PUNCTINPUT_DEBUG") != "1")
                {
                    return;
                }
                string path = Path.Combine(Path.GetTempPath(), "PunctInput_debug.log");
                File.AppendAllText(path, DateTime.Now.ToString("HH:mm:ss.fff") + " " + line + Environment.NewLine);
            }
            catch (Exception)
            {
                // 日誌失敗不影響主功能
            }
        }

        // ---- P/Invoke ----

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        // 明確綁定 W 版：預設 ANSI 綁定會把 WM_CHAR 的 CJK 字元經代碼頁轉換為問號
        [DllImport("user32.dll", SetLastError = true, EntryPoint = "PostMessageW")]
        private static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint pid);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO info);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, System.Text.StringBuilder buffer, int maxCount);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [StructLayout(LayoutKind.Sequential)]
        private struct GUITHREADINFO
        {
            public int cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }
    }
}
