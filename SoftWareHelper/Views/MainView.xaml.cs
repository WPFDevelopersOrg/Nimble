using SoftWareHelper.Helpers;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace SoftWareHelper.Views
{
    /// <summary>
    /// MainView.xaml 的交互逻辑
    /// </summary>
    public partial class MainView : Window
    {
        private Rect desktopWorkingArea;

        Point anchorPoint;
        bool inDrag;

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
            this.Top = desktopWorkingArea.Height / 2 -(this.Height / 2);
            this.Loaded += Window_Loaded;
            this.Deactivated += MainView_Deactivated;
        }

        private void MainView_Deactivated(object sender, EventArgs e)
        {
            MainView window = (MainView)sender;
            window.Topmost = true;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Common.IsWin10)
            {
                //WindowBlur.SetIsEnabled(this, true);
                //DataContext = new WindowBlureffect(this, AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND) { BlurOpacity = 100 };
            }
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);

            int exStyle = (int)Win32Api.GetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE);

            exStyle |= (int)Win32Api.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            Win32Api.SetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
            //Win32Api.HwndSourceAdd(this);
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            anchorPoint = e.GetPosition(this);
            inDrag = true;
            CaptureMouse();
            e.Handled = true;
        }
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (inDrag)
            {
                Point currentPoint = e.GetPosition(this);
                var y = this.Top + currentPoint.Y - anchorPoint.Y;
                Win32Api.MoveWindow(new WindowInteropHelper(this).Handle, Convert.ToInt32(desktopWorkingArea.Width - this.Width), (int)y, (int)this.Width, (int)this.Height, true);
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

        private void ToggleButtonMini_Checked(object sender, RoutedEventArgs e)
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
            widthAnimation.Completed += (s, e1) =>
            {
                this.Left = desktopWorkingArea.Width - this.Width;
            };
            //heightAnimation.Completed += Animation_Completed;
            //widthAnimation.Completed += Animation_Completed;
            this.BeginAnimation(Window.HeightProperty, heightAnimation);
            this.BeginAnimation(Window.WidthProperty,widthAnimation);
        }

        private void Animation_Completed(object sender, EventArgs e)
        {
            Timeline name = ((AnimationClock)sender).Timeline;
            switch (name.Name)
            {
                case "widthMini":
                    this.Left = desktopWorkingArea.Width - this.Width;
                    break;

            }
            Console.WriteLine(this.Width);
        }

        private void UnToggleButtonMini_Checked(object sender, RoutedEventArgs e)
        {
           
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

        }
        
    }

}
