using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using SoftwareHelper.Helpers;
using SoftwareHelper.ViewModels;
using WPFDevelopers.Controls;

namespace SoftwareHelper.Views
{
    /// <summary>
    ///     EmbedDeasktopView.xaml 的交互逻辑
    /// </summary>
    public partial class EmbedDeasktopView : Window
    {
        private readonly KeyboardHook _hook;
        private readonly Rect desktopWorkingArea;
        private readonly List<Key> keys = new List<Key>();
        private Point anchorPoint;
        private bool inDrag;
        private readonly MainVM mainVM;

        public EmbedDeasktopView()
        {
            InitializeComponent();
            mainVM = DataContext as MainVM;
            desktopWorkingArea = SystemParameters.WorkArea;
            Loaded += EmbedDeasktopView_Loaded;
            Closing += EmbedDeasktopView_Closing;
            _hook = new KeyboardHook();
            _hook.KeyDown += OnHookKeyDown;
            _hook.KeyUp += OnHookKeyUp;
        }

        private void OnHookKeyUp(object sender, HookEventArgs e)
        {
            if (!mainVM.IsEmbedded) return;
            SetKeyUp(e.Key);
            KeyDownPanel.Visibility = Visibility.Collapsed;
        }

        private void OnHookKeyDown(object sender, HookEventArgs e)
        {
            if (!mainVM.IsEmbedded) return;
            SetKeyDown(e.Key);
            if (IsKeyDown(Key.PrintScreen))
            {
                var screenCut = new ScreenCut();
                screenCut.ShowDialog();
            }
            else
            {
                if (keys.Count == 2 && IsKeyDown(Key.LeftAlt))
                {
                    var _key = Win32Api.GetCharFromKey(e.Key).ToString().ToUpper();
                    if (string.IsNullOrWhiteSpace(_key))
                        return;
                    var offset = 0.0;
                    var scrollViewer = ControlsHelper.FindChild<ScrollViewer>(AppSwitchList, "PART_ScrollViewer");
                    var elementList = ControlsHelper.FindVisualChildren<Border>(AppSwitchList).ToList();
                    var isFind = false;
                    for (var i = 0; i < elementList.Count; i++)
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

        private void IMEUS()
        {
            var name = new StringBuilder(9);
            Win32Api.GetKeyboardLayoutName(name);
            var keyBoardLayout = name.ToString();
            Win32Api.PostMessage(Win32Api.GetForegroundWindow(), Win32Api.WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero,
                Win32Api.LoadKeyboardLayout("0000409", Win32Api.KLF_ACTIVATE));
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

            var wndHelper = new WindowInteropHelper(this);
            var exStyle = (int)Win32Api.GetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)Win32Api.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            Win32Api.SetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            #endregion
        }

        private void EmbedDeasktopView_Closing(object sender, CancelEventArgs e)
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

        public void ExitEmbedded()
        {
            if (ToggleButtonMini.IsChecked == true)
                Width = 30;
            else
                Width = 120;
            Height = desktopWorkingArea.Height / 2;
            Left = desktopWorkingArea.Width - Width;
            Top = desktopWorkingArea.Height / 2 - Height / 2;
        }

        public void OepnEmbedded()
        {
            if (mainVM.IsEmbedded) return;
        }

        private void myNotifyIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            if (mainVM.IsEmbedded) return;
            Show();
            Activate();
            Focus();
        }

        #region 移动窗体

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            anchorPoint = e.GetPosition(this);
            inDrag = true;
            CaptureMouse();
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            try
            {
                if (!inDrag && e.LeftButton != MouseButtonState.Pressed) return;
                var currentPoint = e.GetPosition(this);
                Top = Top + (currentPoint.Y - anchorPoint.Y);
            }
            catch (Exception ex)
            {
                Log.Error($"MainView.OnMouseMove{ex.Message}");
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (inDrag)
            {
                ReleaseMouseCapture();
                inDrag = false;
                e.Handled = true;
            }
        }

        #endregion

        #region 窗体动画

        private void ToggleButtonMini_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                EasingFunctionBase easeFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseOut
                };

                var heightAnimation = new DoubleAnimation
                {
                    Name = "heightMini",
                    From = desktopWorkingArea.Height / 2,
                    To = 60,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    EasingFunction = easeFunction
                };
                heightAnimation.Completed += delegate { BeginAnimation(HeightProperty, null); };
                var widthAnimation = new DoubleAnimation
                {
                    Name = "widthMini",
                    From = 120,
                    To = 30,
                    Duration = new Duration(TimeSpan.FromSeconds(0.51)),
                    EasingFunction = easeFunction
                };
                widthAnimation.Completed += delegate
                {
                    Left = desktopWorkingArea.Width - Width;
                    BeginAnimation(WidthProperty, null);
                };
                BeginAnimation(HeightProperty, heightAnimation);
                BeginAnimation(WidthProperty, widthAnimation);
            }
            catch (Exception ex)
            {
                Log.Error($"MainView.ToggleButtonMini_Checked{ex.Message}");
            }
        }


        private void UnToggleButtonMini_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                var easeFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseIn
                };
                var widthAnimation = new DoubleAnimation
                {
                    From = 30,
                    To = 120,
                    Duration = new Duration(TimeSpan.FromSeconds(0.01)),
                    EasingFunction = easeFunction
                };
                widthAnimation.Completed += delegate
                {
                    Left = desktopWorkingArea.Width - Width;
                    BeginAnimation(WidthProperty, null);
                };

                var heightAnimation = new DoubleAnimation
                {
                    From = 60,
                    To = desktopWorkingArea.Height / 2,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    EasingFunction = easeFunction
                };
                heightAnimation.Completed += delegate { BeginAnimation(HeightProperty, null); };
                BeginAnimation(WidthProperty, widthAnimation);
                BeginAnimation(HeightProperty, heightAnimation);
            }
            catch (Exception ex)
            {
                Log.Error($"MainView.UnToggleButtonMini_Checked{ex.Message}");
            }
        }

        #endregion
    }
}