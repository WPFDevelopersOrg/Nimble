using SoftWareHelper.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SoftWareHelper.Views
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

            //var langs = InputLanguageManager.Current.AvailableInputLanguages;
            //foreach (System.Globalization.CultureInfo lang in langs)
            //{
            //    if (!lang.Equals(InputLanguageManager.Current.CurrentInputLanguage))
            //    {
            //        InputLanguageManager.SetInputLanguage(this, lang);
            //        break;
            //    }
            //}
            this.Loaded += EmbedDeasktopView_Loaded;
            this.Closing += EmbedDeasktopView_Closing;
            _hook = new KeyboardHook();
            _hook.KeyDown += new KeyboardHook.HookEventHandler(OnHookKeyDown);
            _hook.KeyUp += new KeyboardHook.HookEventHandler(OnHookKeyUp);
            //this.KeyDown += EmbedDeasktopView_KeyDown;
            //this.KeyUp += EmbedDeasktopView_KeyUp;
            //InputMethod.SetIsInputMethodEnabled(input, false);
            //this.MouseEnter += (s, e) => 
            //{
            //    //FocusManager.SetIsFocusScope(this.AppSwitchList, true);
            //    //if (!IsActive)
            //    //{
            //    //WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            //    //Win32Api.SetFocus(wndHelper.Handle);
            //    //Show();
            //    //Activate();
            //    //Focus();
            //    //InputMethod.SetPreferredImeConversionMode(this, ImeConversionModeValues.Alphanumeric);
            //    //InputMethod.SetPreferredImeState(this, InputMethodState.On);
            //    //}
            //    //this.Focusable = true;
            //    //IInputElement element = Keyboard.Focus(this);
            //    //input.Focus();

            //    #region 鼠标移入点击
            //    //Point pos = new Point(0, 0);
            //    //pos = this.PointToScreen(pos);
            //    //Win32Api.mouse_event(Win32Api.MOUSEEVENTF_LEFTDOWN, (int)pos.X, (int)pos.Y, 0, 0);
            //    //System.Threading.Thread.Sleep(1000);
            //    //Win32Api.mouse_event(Win32Api.MOUSEEVENTF_LEFTUP, (int)pos.X, (int)pos.Y, 0, 0);
            //    //IMEUS(); 
            //    #endregion
            //};
            //this.MouseLeave += (s, e) =>
            //{
            //    #region 当鼠标移开窗体 切换为中文
            //    //var cultureType = "zh-CN";
            //    //var installedInputLanguages = System.Windows.Forms.InputLanguage.InstalledInputLanguages;

            //    //if (installedInputLanguages.Cast<System.Windows.Forms.InputLanguage>().Any(i => i.Culture.Name == cultureType))
            //    //{
            //    //    System.Windows.Forms.InputLanguage.CurrentInputLanguage = System.Windows.Forms.InputLanguage.FromCulture(System.Globalization.CultureInfo.GetCultureInfo(cultureType));
            //    //} 
            //    #endregion
            //    Keyboard.ClearFocus();
            //};
        }

        private void OnHookKeyUp(object sender, HookEventArgs e)
        {
            SetKeyUp(e.Key);
            Thread.Sleep(300);
            this.KeyDownPanel.Visibility = Visibility.Collapsed;
        }

        private void OnHookKeyDown(object sender, HookEventArgs e)
        {
            SetKeyDown(e.Key);
            if (keys.Count > 1 && IsKeyDown(Key.LeftAlt))
            {
                var _key = Win32Api.GetCharFromKey(e.Key).ToString().ToUpper();
                if (string.IsNullOrWhiteSpace(_key))
                    return;
                double offset = 0.0;
                ScrollViewer scrollViewer = ControlsHelper.FindChild<ScrollViewer>(this.AppSwitchList, "PART_ScrollViewer");
                var elementList = ControlsHelper.FindVisualChildren<Border>(this.AppSwitchList).ToList();
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
                this.KeyDownText.Text = _key;
                this.KeyDownPanel.Visibility = Visibility.Visible;
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
            String KeyBoardLayout = name.ToString();
            Win32Api.PostMessage(Win32Api.GetForegroundWindow(), Win32Api.WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, Win32Api.LoadKeyboardLayout("0000409", Win32Api.KLF_ACTIVATE));
        }
        private void EmbedDeasktopView_KeyUp(object sender, KeyEventArgs e)
        {
            //ObjectAnimationUsingKeyFrames animate = new ObjectAnimationUsingKeyFrames();
            //animate.Duration = new Duration(TimeSpan.FromSeconds(0.3));
            //DiscreteObjectKeyFrame objVis= new DiscreteObjectKeyFrame
            //{
            //    KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.3)),
            //    Value = Visibility.Collapsed
            //};
            //animate.KeyFrames.Add(objVis);
            //KeyDownPanel.BeginAnimation(Border.VisibilityProperty, animate);
            //EasingFunctionBase easeFunction = new CubicEase()
            //{
            //    EasingMode = EasingMode.EaseOut,
            //};
            //DoubleAnimation opacityAnimation = new DoubleAnimation
            //{
            //    To = 0,
            //    Duration = new Duration(TimeSpan.FromSeconds(10)),
            //    EasingFunction = easeFunction
            //};
            //opacityAnimation.Completed += (s1, e1) => 
            //{
            //    this.KeyDownPanel.Opacity = 1;
            //    this.KeyDownPanel.Visibility = Visibility.Collapsed;
            //};
            //this.KeyDownPanel.BeginAnimation(Border.OpacityProperty, opacityAnimation);
            Thread.Sleep(300);
            this.KeyDownPanel.Visibility = Visibility.Collapsed;
        }

        private void EmbedDeasktopView_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void EmbedDeasktopView_Loaded(object sender, RoutedEventArgs e)
        {
            //if (Common.IsWin10)
            //{
            //    WindowBlur.SetIsEnabled(this, true);
            //}
            #region 注释
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            int exStyle = (int)Win32Api.GetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)Win32Api.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            Win32Api.SetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
            #endregion
        }

        private void EmbedDeasktopView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Win32Api.UnRegisterDesktop(this);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Win32Api.RegisterDesktop(this);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            myNotifyIcon.Dispose();
            System.Environment.Exit(0);
        }
    }
}
