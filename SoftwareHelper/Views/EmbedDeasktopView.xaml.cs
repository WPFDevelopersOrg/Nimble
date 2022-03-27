using SoftwareHelper.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;

namespace SoftwareHelper.Views
{
    /// <summary>
    /// EmbedDeasktopView.xaml 的交互逻辑
    /// </summary>
    public partial class EmbedDeasktopView : Window
    {
        private KeyboardHook _hook;
        private List<Key> keys = new List<Key>();
        public EmbedDeasktopView()
        {
            InitializeComponent();
            Loaded += EmbedDeasktopView_Loaded;
            Closing += EmbedDeasktopView_Closing;
            _hook = new KeyboardHook();
            _hook.KeyDown += new KeyboardHook.HookEventHandler(OnHookKeyDown);
            _hook.KeyUp += new KeyboardHook.HookEventHandler(OnHookKeyUp);
            
        }

        private void OnHookKeyUp(object sender, HookEventArgs e)
        {
            SetKeyUp(e.Key);
            Thread.Sleep(300);
            KeyDownPanel.Visibility = Visibility.Collapsed;
        }

        private void OnHookKeyDown(object sender, HookEventArgs e)
        {
            SetKeyDown(e.Key);
            if (keys.Count == 2 && IsKeyDown(Key.LeftAlt))
            {
                var _key = Win32Api.GetCharFromKey(e.Key).ToString().ToUpper();
                if (string.IsNullOrWhiteSpace(_key))
                    return;
                double offset = 0.0;
                ScrollViewer scrollViewer = ControlsHelper.FindChild<ScrollViewer>(AppSwitchList, "PART_ScrollViewer");
                var elementList = ControlsHelper.FindVisualChildren<Border>(AppSwitchList).ToList();
                bool isFind = false;
                for (int i = 0; i < elementList.Count; i++)
                {
                    var element = elementList[i];
                    if (isFind) break;
                    if (element.Tag != null)
                    {
                        offset += element.ActualHeight;
                        if (element.Tag.ToString().Equals(_key))
                        {
                            offset -= element.ActualHeight;
                            scrollViewer.ScrollToVerticalOffset(offset);
                            isFind = true;
                        }
                    }

                }
                KeyDownText.Text = _key;
                KeyDownPanel.Visibility = Visibility.Visible;
            }
            
        }
        private bool IsKeyDown(Key key)
        {
            return keys.Contains(key);
        }

        private void SetKeyDown(Key key)
        {
            if (!keys.Contains(key))
                keys.Add(key);
        }

        private void SetKeyUp(Key key)
        {
            if (keys.Contains(key))
                keys.Remove(key);
        }
        void IMEUS()
        {
            StringBuilder name = new StringBuilder(9);
            Win32Api.GetKeyboardLayoutName(name);
            var keyBoardLayout = name.ToString();
            Win32Api.PostMessage(Win32Api.GetForegroundWindow(), Win32Api.WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, Win32Api.LoadKeyboardLayout("0000409", Win32Api.KLF_ACTIVATE));
        }
        private void EmbedDeasktopView_KeyUp(object sender, KeyEventArgs e)
        {
            Thread.Sleep(300);
            KeyDownPanel.Visibility = Visibility.Collapsed;
        }

        private void EmbedDeasktopView_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void EmbedDeasktopView_Loaded(object sender, RoutedEventArgs e)
        {
            #region 注释
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            int exStyle = (int)Win32Api.GetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)Win32Api.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            Win32Api.SetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
            #endregion
        }

        private void EmbedDeasktopView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Win32Api.UnRegisterDesktop();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Win32Api.RegisterDesktop(this);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
            myNotifyIcon.Dispose();
            Environment.Exit(0);
        }
    }
}
