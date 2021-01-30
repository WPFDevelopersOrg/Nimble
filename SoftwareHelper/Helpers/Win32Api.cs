using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SoftwareHelper.Helpers
{
    public class Win32Api
    {
        public struct  WINDOWPOS
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public int cx;
            public int cy;
            public UInt32 flags;
        };
        public static void HwndSourceAdd(Window win)
        {
            HwndSource source = HwndSource.FromHwnd(new WindowInteropHelper(win).Handle);
            source.AddHook(new HwndSourceHook(Win32Api.WndProc));
        }
        private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case 0x46:
                    if (Mouse.LeftButton != MouseButtonState.Pressed)
                    {
                        WINDOWPOS wp = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
                        wp.flags = wp.flags | 2;
                        Marshal.StructureToPtr(wp, lParam, false);
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        /// <summary>
        /// 移动
        /// </summary>
        /// <param name="hWnd"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="nWidth"></param>
        /// <param name="nHeight"></param>
        /// <param name="bRepaint"></param>
        /// <returns></returns>
        [DllImportAttribute("user32.dll", EntryPoint = "MoveWindow")]
        public static extern int MoveWindow(IntPtr hwnd, int x, int y, int w, int h, int bRepaint);


        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static ImageSource ChangeBitmapToImageSource(Bitmap bitmap)
        {
            IntPtr hBitmap = bitmap.GetHbitmap();
            ImageSource wpfBitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new System.ComponentModel.Win32Exception();
            }
            return wpfBitmap;
        }


        #region Window styles
        [Flags]
        public enum ExtendedWindowStyles
        {
            // ...
            WS_EX_TOOLWINDOW = 0x00000080,
            // ...
        }

        public enum GetWindowLongFields
        {
            // ...
            GWL_EXSTYLE = (-20),
            // ...
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

        public static IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            int error = 0;
            IntPtr result = IntPtr.Zero;
            // Win32 SetWindowLong doesn't clear error on success
            SetLastError(0);

            if (IntPtr.Size == 4)
            {
                // use SetWindowLong
                Int32 tempResult = IntSetWindowLong(hWnd, nIndex, IntPtrToInt32(dwNewLong));
                error = Marshal.GetLastWin32Error();
                result = new IntPtr(tempResult);
            }
            else
            {
                // use SetWindowLongPtr
                result = IntSetWindowLongPtr(hWnd, nIndex, dwNewLong);
                error = Marshal.GetLastWin32Error();
            }

            if ((result == IntPtr.Zero) && (error != 0))
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            return result;
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
        private static extern IntPtr IntSetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
        private static extern Int32 IntSetWindowLong(IntPtr hWnd, int nIndex, Int32 dwNewLong);

        private static int IntPtrToInt32(IntPtr intPtr)
        {
            return unchecked((int)intPtr.ToInt64());
        }

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        public static extern void SetLastError(int dwErrorCode);
        #endregion


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32")]
        public static extern int GetSystemMetrics(int nIndex);

        #region 设置DPI 
        [STAThread]
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool SetProcessDPIAware(); 
        #endregion


        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        internal struct RECT
        {
            internal readonly int left;
            internal readonly int top;
            internal readonly int right;
            internal readonly int bottom;
        }
        [DllImport("user32.dll")]
        internal static extern int GetWindowRect(IntPtr hWnd, out RECT rect);

        #region 置顶(待定)
        [DllImport("user32.dll")]
        public static extern int SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

        public const int SWP_SHOWWINDOW = 0x0040;

        public const int SWP_NOMOVE = 0x2;

        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        #endregion



        [DllImport("shell32.dll")]
        public static extern UInt32 SHAppBarMessage(UInt32 dwMessage, ref APPBARDATA pData);
        public enum AppBarMessages
        {
            New = 0x00,
            Remove = 0x01,
            QueryPos = 0x02,
            SetPos = 0x03,
            GetState = 0x04,
            GetTaskBarPos = 0x05,
            Activate = 0x06,
            GetAutoHideBar = 0x07,
            SetAutoHideBar = 0x08,
            WindowPosChanged = 0x09,
            SetState = 0x0a
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public UInt32 cbSize;
            public IntPtr hWnd;
            public UInt32 uCallbackMessage;
            public UInt32 uEdge;//后期扩展
            public RECT1 rc;
            public Int32 lParam;
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT1
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="window"></param>
        public static void RegisterDesktop(Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            HwndSource mainWindowSrc = (HwndSource)HwndSource.FromHwnd(helper.Handle);

            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = (uint)Marshal.SizeOf(abd);
            abd.hWnd = mainWindowSrc.Handle;
            abd.uEdge = 2;
            abd.rc.top = 0;
            abd.rc.bottom = (int)SystemParameters.PrimaryScreenHeight;
            abd.rc.right = (int)SystemParameters.PrimaryScreenWidth;
            abd.rc.left = abd.rc.right - (int)window.ActualWidth;

            //注册新的应用栏，并指定系统应用于向应用栏发送通知消息的消息标识符。
            SHAppBarMessage((UInt32)AppBarMessages.New, ref abd);
            //请求应用栏的大小和屏幕位置。
            SHAppBarMessage((UInt32)AppBarMessages.QueryPos, ref abd);
            //设置应用栏的大小和屏幕位置。
            SHAppBarMessage((UInt32)AppBarMessages.SetPos, ref abd);
            //设置应用所在平面位置。
            MoveWindow(abd.hWnd, abd.rc.left, abd.rc.top, abd.rc.right - abd.rc.left, abd.rc.bottom - abd.rc.top, 1);
        }
        /// <summary>
        /// 卸载
        /// </summary>
        /// <param name="window"></param>
        public static void UnRegisterDesktop(Window window)
        {
            WindowInteropHelper helper = new WindowInteropHelper(window);
            HwndSource mainWindowSrc = (HwndSource)HwndSource.FromHwnd(helper.Handle);

            APPBARDATA abd = new APPBARDATA();
            abd.cbSize = (uint)Marshal.SizeOf(abd);
            abd.hWnd = mainWindowSrc.Handle;

            SHAppBarMessage((UInt32)AppBarMessages.Remove, ref abd);
        }

        #region 键盘转译
        public enum MapType : uint
        {
            MAPVK_VK_TO_VSC = 0x0,
            MAPVK_VSC_TO_VK = 0x1,
            MAPVK_VK_TO_CHAR = 0x2,
            MAPVK_VSC_TO_VK_EX = 0x3,
        }

        [DllImport("user32.dll")]
        public static extern int ToUnicode(
            uint wVirtKey,
            uint wScanCode,
            byte[] lpKeyState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeParamIndex = 4)]
            StringBuilder pwszBuff,
            int cchBuff,
            uint wFlags);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern uint MapVirtualKey(uint uCode, MapType uMapType);

        public static char GetCharFromKey(Key key)
        {
            char ch = ' ';

            int virtualKey = KeyInterop.VirtualKeyFromKey(key);
            byte[] keyboardState = new byte[256];
            GetKeyboardState(keyboardState);

            uint scanCode = MapVirtualKey((uint)virtualKey, MapType.MAPVK_VK_TO_VSC);
            StringBuilder stringBuilder = new StringBuilder(2);

            int result = ToUnicode((uint)virtualKey, scanCode, keyboardState, stringBuilder, stringBuilder.Capacity, 0);
            switch (result)
            {
                case -1:
                    break;
                case 0:
                    break;
                case 1:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
                default:
                    {
                        ch = stringBuilder[0];
                        break;
                    }
            }
            return ch;
        }
        #endregion

        #region IME
        [DllImport("user32.dll")]
        public static extern long GetKeyboardLayoutName(
         System.Text.StringBuilder pwszKLID);
        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hhwnd, uint msg, IntPtr wparam, IntPtr lparam);
        [DllImport("user32.dll")]
        public static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        public const uint WM_INPUTLANGCHANGEREQUEST = 0x0050;
        public const uint KLF_ACTIVATE = 1;
        #endregion

        [DllImport("user32.dll")]
        public static extern IntPtr SetFocus(IntPtr hWnd);//设定焦点

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        //This simulates a left mouse click
        public static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetActiveWindow();
        [DllImport("user32.dll")]
        public static extern int GetFocus();
    }
}
