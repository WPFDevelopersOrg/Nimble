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

namespace SoftwareHelper.Views
{
    /// <summary>
    ///     EmbedDeasktopView.xaml 的交互逻辑
    /// </summary>
    public partial class EmbedDeasktopView : Window
    {
        private bool inDrag;
        private readonly Rect desktopWorkingArea;
        private Point anchorPoint;
        private readonly KeyboardHook _hook;
        private readonly List<Key> keys = new List<Key>();

        public EmbedDeasktopView()
        {
            InitializeComponent();
            desktopWorkingArea = SystemParameters.WorkArea;
            Loaded += EmbedDeasktopView_Loaded;
            Closing += EmbedDeasktopView_Closing;
            _hook = new KeyboardHook();
            _hook.KeyDown += OnHookKeyDown;
            _hook.KeyUp += OnHookKeyUp;
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
            if (IsKeyDown(Key.PrintScreen))
            {
                var screenCut = new WPFDevelopers.Controls.ScreenCut();
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
                if (!inDrag) return;
                var currentPoint = e.GetPosition(this);
                var y = this.Top + currentPoint.Y - anchorPoint.Y;
                Win32Api.RECT rect;
                Win32Api.GetWindowRect(new WindowInteropHelper(this).Handle, out rect);
                var w = rect.right - rect.left;
                var h = rect.bottom - rect.top;
                int x = Convert.ToInt32(PrimaryScreen.DESKTOP.Width - w);
                Win32Api.MoveWindow(new WindowInteropHelper(this).Handle, x, (int)y, w, h, 1);

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
        public void ExitEmbedded()
        {
            Width = 110;
            Height = desktopWorkingArea.Height / 2;
            Left = desktopWorkingArea.Width - Width;
            Top = desktopWorkingArea.Height / 2 - (Height / 2);
        }
        #region 窗体动画
        private void ToggleButtonMini_Checked(object sender, RoutedEventArgs e)
        {

            try
            {
                EasingFunctionBase easeFunction = new CubicEase()
                {
                    EasingMode = EasingMode.EaseOut,
                };

                var heightAnimation = new DoubleAnimation
                {
                    Name = "heightMini",
                    To = 60,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    EasingFunction = easeFunction
                };
                var widthAnimation = new DoubleAnimation
                {
                    Name = "widthMini",
                    To = 30,
                    Duration = new Duration(TimeSpan.FromSeconds(0.51)),
                    EasingFunction = easeFunction
                };
                widthAnimation.Completed += delegate
                {
                    this.Left = desktopWorkingArea.Width - this.Width;
                };
               
                this.BeginAnimation(HeightProperty, heightAnimation);
                this.BeginAnimation(WidthProperty, widthAnimation);
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
                var easeFunction = new CubicEase()
                {
                    EasingMode = EasingMode.EaseIn,
                };
                var widthAnimation = new DoubleAnimation
                {
                    To = 120,
                    Duration = new Duration(TimeSpan.FromSeconds(0.01)),
                    EasingFunction = easeFunction
                };
                widthAnimation.Completed += delegate
                {
                    this.Left = desktopWorkingArea.Width - this.Width;
                };

                var heightAnimation = new DoubleAnimation
                {
                    To = desktopWorkingArea.Height / 2,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    EasingFunction = easeFunction
                };
                this.BeginAnimation(WidthProperty, widthAnimation);
                this.BeginAnimation(HeightProperty, heightAnimation);
            }
            catch (Exception ex)
            {
                Log.Error($"MainView.UnToggleButtonMini_Checked{ex.Message}");
            }

        }


        #endregion

        private void myNotifyIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as MainVM;
            if (vm == null && vm.IsEmbedded) return;
            Show();
            Activate();
            Focus();
        }
    }
}