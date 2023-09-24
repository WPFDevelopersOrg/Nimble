using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Nimble.Helpers.MouseHelper
{
    /// <summary>
    ///     Captures global mouse events
    /// </summary>
    public class MouseHook : GlobalHook
    {
        #region Constructor

        public MouseHook()
        {
            _hookType = WH_MOUSE_LL;
        }

        #endregion

        #region MouseEventType Enum

        private enum MouseEventType
        {
            None,
            MouseDown,
            MouseUp,
            DoubleClick,
            MouseWheel,
            MouseMove
        }

        #endregion

        #region Events

        public event MouseEventHandler MouseDown;
        public event MouseEventHandler MouseUp;
        public event MouseEventHandler MouseMove;
        public event MouseEventHandler MouseWheel;

        public event EventHandler Click;
        public event EventHandler DoubleClick;

        #endregion

        #region Methods

        protected override int HookCallbackProcedure(int nCode, int wParam, IntPtr lParam)
        {
            if (nCode > -1 && (MouseDown != null || MouseUp != null || MouseMove != null))
            {
                var mouseHookStruct =
                    (MouseHookStruct)Marshal.PtrToStructure(lParam, typeof(MouseHookStruct));

                var button = GetButton(wParam);
                var eventType = GetEventType(wParam);

                var e = new MouseEventArgs(
                    button,
                    eventType == MouseEventType.DoubleClick ? 2 : 1,
                    mouseHookStruct.pt.X,
                    mouseHookStruct.pt.Y,
                    0);

                // Prevent multiple Right Click events (this probably happens for popup menus)
                if (button == MouseButtons.Right) eventType = MouseEventType.None;

                switch (eventType)
                {
                    case MouseEventType.MouseDown:
                        if (MouseDown != null) MouseDown(this, e);
                        break;
                    case MouseEventType.MouseUp:
                        if (Click != null) Click(this, new EventArgs());
                        if (MouseUp != null) MouseUp(this, e);
                        break;
                    case MouseEventType.DoubleClick:
                        if (DoubleClick != null) DoubleClick(this, new EventArgs());
                        break;
                    case MouseEventType.MouseWheel:
                        if (MouseWheel != null) MouseWheel(this, e);
                        break;
                    case MouseEventType.MouseMove:
                        if (MouseMove != null) MouseMove(this, e);
                        break;
                }
            }

            return CallNextHookEx(_handleToHook, nCode, wParam, lParam);
        }

        private MouseButtons GetButton(int wParam)
        {
            switch (wParam)
            {
                case WM_LBUTTONDOWN:
                case WM_LBUTTONUP:
                case WM_LBUTTONDBLCLK:
                    return MouseButtons.Left;
                case WM_RBUTTONDOWN:
                case WM_RBUTTONUP:
                case WM_RBUTTONDBLCLK:
                    return MouseButtons.Right;
                case WM_MBUTTONDOWN:
                case WM_MBUTTONUP:
                case WM_MBUTTONDBLCLK:
                    return MouseButtons.Middle;
                default:
                    return MouseButtons.None;
            }
        }

        private MouseEventType GetEventType(int wParam)
        {
            switch (wParam)
            {
                case WM_LBUTTONDOWN:
                case WM_RBUTTONDOWN:
                case WM_MBUTTONDOWN:
                    return MouseEventType.MouseDown;
                case WM_LBUTTONUP:
                case WM_RBUTTONUP:
                case WM_MBUTTONUP:
                    return MouseEventType.MouseUp;
                case WM_LBUTTONDBLCLK:
                case WM_RBUTTONDBLCLK:
                case WM_MBUTTONDBLCLK:
                    return MouseEventType.DoubleClick;
                case WM_MOUSEWHEEL:
                    return MouseEventType.MouseWheel;
                case WM_MOUSEMOVE:
                    return MouseEventType.MouseMove;
                default:
                    return MouseEventType.None;
            }
        }

        #endregion
    }
}