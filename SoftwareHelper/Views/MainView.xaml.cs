using SoftwareHelper.Helpers;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace SoftwareHelper.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        #region 静态属性
        private Rect desktopWorkingArea;
        private System.Windows.Point anchorPoint;
        private bool inDrag;
        #endregion


        public bool IsEdgeHide
        {
            get { return (bool)GetValue(IsEdgeHideProperty); }
            set { SetValue(IsEdgeHideProperty, value); }
        }

        public static readonly DependencyProperty IsEdgeHideProperty =
            DependencyProperty.Register("IsEdgeHide", typeof(bool), typeof(MainView), new PropertyMetadata(false));


        public MainView()
        {
            InitializeComponent();
            desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            Height = desktopWorkingArea.Height / 2;
            Left = desktopWorkingArea.Width - Width;
            Top = desktopWorkingArea.Height / 2 - (Height / 2);
            Loaded += Window_Loaded;

            #region 边缘隐藏
            //MouseLeave += MainView_MouseLeave;
            //MouseEnter += MainView_MouseEnter;
            #endregion

            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        Topmost = false;
                        Topmost = true;
                        
                    }));
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
       

        #region Deactivated,PreviewLostKeyboardFocus
        private void MainView_Deactivated(object sender, EventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }
        private void MainView_PreviewLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Window window = (Window)sender;
            window.Topmost = true;
        }
        #endregion

        #region Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region 注释
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            int exStyle = (int)Win32Api.GetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)Win32Api.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            Win32Api.SetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
            #endregion
        }
        #endregion

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
                if (inDrag)
                {
                    System.Windows.Point currentPoint = e.GetPosition(this);
                    var y = Top + currentPoint.Y - anchorPoint.Y;
                    Win32Api.RECT rect;
                    Win32Api.GetWindowRect(new WindowInteropHelper(this).Handle, out rect);
                    var w = rect.right - rect.left;
                    var h = rect.bottom - rect.top;
                    int x = Convert.ToInt32(PrimaryScreen.DESKTOP.Width - w);

                    Win32Api.MoveWindow(new WindowInteropHelper(this).Handle, x, (int)y, w, h, 1);
                }
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
                //CollectRise();
                #region 注释
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
                widthAnimation.Completed += (s, e1) =>
                {
                    Left = desktopWorkingArea.Width - Width;
                };
                //heightAnimation.Completed += Animation_Completed;
                //widthAnimation.Completed += Animation_Completed;
                BeginAnimation(Window.HeightProperty, heightAnimation);
                BeginAnimation(Window.WidthProperty, widthAnimation);
                #endregion
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
                //Unfold();
                #region 注释
                EasingFunctionBase easeFunction = new CubicEase()
                {
                    EasingMode = EasingMode.EaseIn,
                };
                var widthAnimation = new DoubleAnimation
                {
                    To = 120,
                    Duration = new Duration(TimeSpan.FromSeconds(0.01)),
                    EasingFunction = easeFunction
                };
                widthAnimation.Completed += (s, e1) =>
                {
                    Left = desktopWorkingArea.Width - Width;
                };

                var heightAnimation = new DoubleAnimation
                {
                    To = desktopWorkingArea.Height / 2,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    EasingFunction = easeFunction
                };
                BeginAnimation(Window.WidthProperty, widthAnimation);
                BeginAnimation(Window.HeightProperty, heightAnimation);
                #endregion
            }
            catch (Exception ex)
            {
                Log.Error($"MainView.UnToggleButtonMini_Checked{ex.Message}");
            }

        }


        #endregion

        private void myNotifyIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            Show();
            Activate();
            Focus();
        }
    }
}
