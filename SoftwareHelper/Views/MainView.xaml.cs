using SoftwareHelper.Helpers;
using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
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
        //private DoubleAnimation animationRight, animationRightBurden;
        /// <summary>
        /// 88 or 32
        /// </summary>
        //private double miniWdith = 32;
        //private bool isLeave, isMove = true;
        #endregion


        public bool IsEdgeHide
        {
            get { return (bool)GetValue(IsEdgeHideProperty); }
            set { SetValue(IsEdgeHideProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsEdgeHide.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEdgeHideProperty =
            DependencyProperty.Register("IsEdgeHide", typeof(bool), typeof(MainView), new PropertyMetadata(false));


        public MainView()
        {
            InitializeComponent();
            //if (!Microsoft.Windows.Shell.SystemParameters2.Current.IsGlassEnabled)
            //{
            //   
            //}
            desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Height = desktopWorkingArea.Height / 2;
            this.Left = desktopWorkingArea.Width - this.Width;
            this.Top = desktopWorkingArea.Height / 2 - (this.Height / 2);
            this.Loaded += Window_Loaded;

            #region 边缘隐藏
            //this.MouseLeave += MainView_MouseLeave;
            //this.MouseEnter += MainView_MouseEnter;
            #endregion

            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    Dispatcher.BeginInvoke(new Action(delegate
                    {
                        //Console.WriteLine(this.IsActive);
                        this.Topmost = false;
                        this.Topmost = true;
                        //if (!this.IsActive)
                        //{
                        //    this.Topmost = false;
                        //    this.Topmost = true;
                        //}
                    }));
                }
            });
            thread.IsBackground = true;
            thread.Start();
            //this.Deactivated += MainView_Deactivated;
            //this.PreviewLostKeyboardFocus += MainView_PreviewLostKeyboardFocus;
        }







        #region 鼠标移入、鼠标移出
        //private void MainView_MouseEnter(object sender, MouseEventArgs e)
        //{
        //    if (!IsEdgeHide)
        //        return;
        //    Unfold();
        //    ToggleButtonMini.IsChecked = false;
        //}
        //private void MainView_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    if (!IsEdgeHide)
        //        return;
        //    CollectRise();
        //    ToggleButtonMini.IsChecked = true;
        //}
        #endregion

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
            if (Common.IsWin10)
            {
                //WindowBlur.SetIsEnabled(this, true);
                //DataContext = new WindowBlureffect(this, AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND) { BlurOpacity = 100 };
            }
            //if (Environment.OSVersion.Version.Major >= 6)
            //    Win32Api.SetProcessDPIAware();

            #region 注释
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            int exStyle = (int)Win32Api.GetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)Win32Api.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            Win32Api.SetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
            #endregion

            //IsEdgeHide = ConfigHelper.EdgeHide;
            //this.MenuItemEdgeHide.IsChecked = IsEdgeHide;
            //SetLightDark(SetConfig());
            //Win32Api.HwndSourceAdd(this);
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
                    var y = this.Top + currentPoint.Y - anchorPoint.Y;
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
                    this.Left = desktopWorkingArea.Width - this.Width;
                };
                //heightAnimation.Completed += Animation_Completed;
                //widthAnimation.Completed += Animation_Completed;
                this.BeginAnimation(Window.HeightProperty, heightAnimation);
                this.BeginAnimation(Window.WidthProperty, widthAnimation);
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
                    this.Left = desktopWorkingArea.Width - this.Width;
                };

                var heightAnimation = new DoubleAnimation
                {
                    To = desktopWorkingArea.Height / 2,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    EasingFunction = easeFunction
                };
                this.BeginAnimation(Window.WidthProperty, widthAnimation);
                this.BeginAnimation(Window.HeightProperty, heightAnimation);
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
            //if (!IsEdgeHide)
            //    Unfold();
            //Unfold();
            //ToggleButtonMini.IsChecked = false;

        }

        //private void MenuItemEdgeHide_Checked(object sender, RoutedEventArgs e)
        //{
        //    IsEdgeHide = true;
        //    SaveEdgeHide();
        //}

        //private void MenuItemEdgeHide_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    IsEdgeHide = false;
        //    SaveEdgeHide();
        //}

        #region 方法

        //void SaveEdgeHide()
        //{
        //    if (IsEdgeHide != ConfigHelper.EdgeHide)
        //    {
        //        ConfigHelper.SaveEdgeHide(IsEdgeHide);
        //    }
        //}

        #region 收起
        //void CollectRise()
        //{
        //    //Console.WriteLine($"鼠标移出{this.ActualWidth}");
        //    if (!isMove || animationRight != null)
        //        return;
        //    var xProp = this.ActualWidth - miniWdith;
        //    animationRight = new DoubleAnimation
        //    {
        //        To = xProp,
        //        Duration = new Duration(TimeSpan.FromMilliseconds(500)),
        //        EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn },
        //    };
        //    animationRight.Completed += (s1, e1) =>
        //    {
        //        Console.WriteLine($"收起translateForm.X：{translateForm.X}");
        //        if (animationRightBurden != null)
        //        {
        //            animationRightBurden = null;
        //        }
        //        isLeave = true;
        //        isMove = false;
        //    };
        //    translateForm.BeginAnimation(TranslateTransform.XProperty, animationRight);


        //    #region 注释
        //    //if (!IsEdgeHide || animationRight!=null)
        //    //    return;
        //    //if (!isMove)
        //    //    return;
        //    //EasingFunctionBase easeFunction = new SineEase()
        //    //{
        //    //    EasingMode = EasingMode.EaseIn,
        //    //};
        //    //animationRight = new DoubleAnimation
        //    //{
        //    //    To = desktopWorkingArea.Width - miniWdith,
        //    //    Duration = new Duration(TimeSpan.FromMilliseconds(500)),
        //    //    EasingFunction = easeFunction,
        //    //};
        //    //animationRight.Completed += (s1, e1) =>
        //    //{
        //    //    Console.WriteLine($"收起this.Left：{this.Left}");
        //    //    if (animationRightBurden != null)
        //    //    {
        //    //        animationRightBurden = null;
        //    //    }
        //    //    var widthAnimation = new DoubleAnimation
        //    //    {
        //    //        To = miniWdith,
        //    //        Duration = new Duration(TimeSpan.FromMilliseconds(300)),
        //    //        EasingFunction = easeFunction
        //    //    };
        //    //    widthAnimation.Completed += (s2, e2) =>
        //    //    {
        //    //        isLeave = true;
        //    //        isMove = false;
        //    //    };
        //    //    this.BeginAnimation(WidthProperty, widthAnimation);
        //    //    //this.Width = miniWdith;
        //    //};
        //    //this.BeginAnimation(LeftProperty, animationRight); 
        //    #endregion
        //}
        #endregion

        #region 展开
        //void Unfold()
        //{
        //    //Console.WriteLine($"鼠标移入{this.ActualWidth}");
        //    if (!isLeave || animationRightBurden != null)
        //        return;
        //    animationRightBurden = new DoubleAnimation
        //    {
        //        To = 0,
        //        Duration = new Duration(TimeSpan.FromMilliseconds(500)),
        //        EasingFunction = new SineEase { EasingMode = EasingMode.EaseIn },
        //    };
        //    animationRightBurden.Completed += (s1, e1) =>
        //    {
        //        Console.WriteLine($"展开后translateForm.X：{translateForm.X}");
        //        if (animationRight != null)
        //        {
        //            animationRight = null;
        //        }
        //        isMove = true;
        //        isLeave = false;
        //    };
        //    translateForm.BeginAnimation(TranslateTransform.XProperty, animationRightBurden);

        //    #region 注释
        //    //if (!IsEdgeHide || animationRightBurden != null)
        //    //    return;
        //    //if (!isLeave)
        //    //    return;
        //    //EasingFunctionBase easeFunction = new SineEase()
        //    //{
        //    //    EasingMode = EasingMode.EaseIn,
        //    //};
        //    //var widthAnimation = new DoubleAnimation
        //    //{
        //    //    To = 120,
        //    //    Duration = new Duration(TimeSpan.FromMilliseconds(100)),
        //    //    EasingFunction = easeFunction
        //    //};
        //    //widthAnimation.Completed += (s, y) =>
        //    //{
        //    //    animationRightBurden = new DoubleAnimation
        //    //    {
        //    //        To = desktopWorkingArea.Width - this.ActualWidth,
        //    //        Duration = new Duration(TimeSpan.FromMilliseconds(500)),
        //    //        EasingFunction = easeFunction,
        //    //    };
        //    //    animationRightBurden.Completed += (s1, e1) =>
        //    //    {
        //    //        Console.WriteLine($"展开后this.Left：{this.Left}");
        //    //        if (animationRight != null)
        //    //        {
        //    //            animationRight = null;
        //    //        }
        //    //        isMove = true;
        //    //        isLeave = false;
        //    //    };

        //    //    this.BeginAnimation(LeftProperty, animationRightBurden);
        //    //};
        //    //this.BeginAnimation(WidthProperty, widthAnimation); 
        //    #endregion
        //}
        #endregion

        #endregion

    }
}
