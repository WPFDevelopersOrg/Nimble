using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace Nimble.Helpers
{
    public partial class KeyboardHook
    {
        private DispatcherTimer _dispatcherTimer;

        #region pinvoke details

        private enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        public struct KBDLLHOOKSTRUCT
        {
            public UInt32 vkCode;
            public UInt32 scanCode;
            public UInt32 flags;
            public UInt32 time;
            public IntPtr extraInfo;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(
            HookType code, HookProc func, IntPtr instance, int threadID);

        [DllImport("user32.dll")]
        private static extern int UnhookWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(
            IntPtr hook, int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

        #endregion

        HookType _hookType = HookType.WH_KEYBOARD_LL;
        IntPtr _hookHandle = IntPtr.Zero;
        HookProc _hookFunction = null;

        private delegate int HookProc(int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam);

        public delegate void HookEventHandler(object sender, HookEventArgs e);
        public event HookEventHandler KeyDown;
        public event HookEventHandler KeyUp;

        public KeyboardHook()
        {
            _hookFunction = new HookProc(HookCallback);
            Install();
            var osVersion = Environment.OSVersion.Version;
            if (osVersion.Major > 6 || (osVersion.Major == 6 && osVersion.Minor >= 2))
            {
                _dispatcherTimer = new DispatcherTimer();
                _dispatcherTimer.Tick += _dispatcherTimer_Tick;
                _dispatcherTimer.Interval = new TimeSpan(3, 0, 0);
                _dispatcherTimer.Start();
            }
        }

        void CreateKeyHook()
        {
            Uninstall();
            Install();
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            _dispatcherTimer.Stop();
            CreateKeyHook();
            _dispatcherTimer.Start();
        }

        ~KeyboardHook()
        {
            Uninstall();
        }

        private int HookCallback(int code, IntPtr wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            if (code < 0)
                return CallNextHookEx(_hookHandle, code, wParam, ref lParam);

            if ((lParam.flags & 0x80) != 0 && KeyUp != null)
                KeyUp(this, new HookEventArgs(lParam.vkCode));

            if ((lParam.flags & 0x80) == 0 && KeyDown != null)
                KeyDown(this, new HookEventArgs(lParam.vkCode));

            return CallNextHookEx(_hookHandle, code, wParam, ref lParam);
        }

        private void Install()
        {
            if (_hookHandle != IntPtr.Zero)
                return;

            Module[] list = Assembly.GetExecutingAssembly().GetModules();
            System.Diagnostics.Debug.Assert(list != null && list.Length > 0);

            _hookHandle = SetWindowsHookEx(_hookType,
                _hookFunction, Marshal.GetHINSTANCE(list[0]), 0);
        }

        private void Uninstall()
        {
            if (_hookHandle != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookHandle);
                _hookHandle = IntPtr.Zero;
            }
        }
    }

    public class HookEventArgs : EventArgs
    {
        public Key Key;
        public bool Alt;
       
        public HookEventArgs(UInt32 keyCode)
        {
            Key = KeyInterop.KeyFromVirtualKey((int)keyCode);
        }
    }


}
