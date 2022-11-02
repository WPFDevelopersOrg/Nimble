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
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using SoftwareHelper.Helpers;
using SoftwareHelper.Models;
using SoftwareHelper.ViewModels;
using WPFDevelopers.Controls;
using static System.Windows.Forms.LinkLabel;
using System.IO;

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
        private string filePath;
        private BitmapSource fileIcon;

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
            SetKeyUp(e.Key);
            KeyDownPanel.Visibility = Visibility.Collapsed;
        }

        private void OnHookKeyDown(object sender, HookEventArgs e)
        {
            SetKeyDown(e.Key);
            if (IsKeyDown(Key.PrintScreen))
            {
                var screenCut = new ScreenCut() {Topmost = true};
                screenCut.Activate();
                screenCut.Closing += delegate 
                {
                    SetKeyUp(Key.PrintScreen);
                };
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
            Win32Api.UnRegisterDesktop(true);
            string json = JsonHelper.Serialize(mainVM.ApplicationList.Where(x=>x.IsDrag == true).OrderBy(x => x.Group));
            FileHelper.WriteFile(Common.ConvertJsonString(json), Common.LocalTemporaryApplicationJson);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Win32Api.RegisterDesktop(this);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
            App.Current.Shutdown();
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
            if (mainVM.IsEmbedded || mainVM.IsDragDrop) return;
            if (e.Source is Border) return;
            anchorPoint = e.GetPosition(this);
            inDrag = true;
            CaptureMouse();
            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            try
            {
                if (mainVM.IsEmbedded || mainVM.IsDragDrop) return;
                if (!inDrag || e.LeftButton != MouseButtonState.Pressed) return;
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


        private void DragCanvas_DragOver(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var msg = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
                    filePath = msg;
                    DragTextBlock.Text = System.IO.Path.GetFileName(filePath);
                    var icon = (BitmapSource)Common.GetIcon(filePath);
                    fileIcon = icon;
                    DragImage.Source = fileIcon;
                    var point = e.GetPosition(this);
                    var x = point.X - DragStackPanel.ActualWidth / 2;
                    var y = point.Y - DragStackPanel.ActualHeight / 2;
                    Canvas.SetLeft(DragStackPanel, x);
                    Canvas.SetTop(DragStackPanel, y);
                }
            }
            catch (Exception ex)
            {
                Log.Error("DragCanvas_DragOver:" + ex.Message);
            }
           
        }

        private void embedDeasktopView_DragEnter(object sender, DragEventArgs e)
        {
            AppSwitchListEmbedded.IsHitTestVisible = false;
            AppSwitchList.IsHitTestVisible = false;
            var doubleXAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(0)),
            };
            DragScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty,doubleXAnimation);
            var doubleYAnimation = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(0)),
            };
            DragScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, doubleXAnimation);
            DragCanvas.Visibility = Visibility.Visible;
        }

        private void embedDeasktopView_DragLeave(object sender, DragEventArgs e)
        {
            DragInitial();
        }
        void DisposeDrag()
        {
            var storyboard = new Storyboard();
            var doubleXAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseIn },
            };
            Storyboard.SetTargetName(doubleXAnimation, "DragStackPanel");
            Storyboard.SetTargetProperty(doubleXAnimation, new PropertyPath("(StackPanel.RenderTransform).(ScaleTransform.ScaleX)"));
            var doubleYAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                EasingFunction = new BackEase { EasingMode = EasingMode.EaseIn },
            };
            Storyboard.SetTargetName(doubleYAnimation, "DragStackPanel");
            Storyboard.SetTargetProperty(doubleYAnimation, new PropertyPath("(StackPanel.RenderTransform).(ScaleTransform.ScaleY)"));
            storyboard.Children.Add(doubleXAnimation);
            storyboard.Children.Add(doubleYAnimation);
            storyboard.Completed += delegate 
            {
                DragInitial();
                var model = new ApplicationModel();
                model.ExePath = filePath;
                model.Name = DragTextBlock.Text;
                var iconPath = System.IO.Path.Combine(Common.TemporaryIconFile, model.Name);
                iconPath = iconPath + ".png";
                model.IconPath = iconPath;
                model.IsDrag = true;
                var firstModel = mainVM.ApplicationList.FirstOrDefault(x => x.Name == model.Name && x.ExePath == model.ExePath);
                if (firstModel != null) return;
                string first = model.Name.Substring(0, 1);
                if (!Common.IsChinese(first))
                {
                    if (char.IsUpper(first.ToCharArray()[0]))
                        model.Group = first;
                    model.Group = model.Name.Substring(0, 1).ToUpper();
                }
                else
                {
                    model.Group = Common.GetCharSpellCode(first);
                }
                mainVM.ApplicationList.Insert(0, model);
                if (File.Exists(iconPath))
                    return;
                Common.SaveImage(fileIcon, iconPath);
            };
            storyboard.Begin(DragStackPanel);
        }
        void DragInitial()
        {
            try
            {
                DragCanvas.Visibility = Visibility.Collapsed;
                AppSwitchListEmbedded.IsHitTestVisible = true;
                AppSwitchList.IsHitTestVisible = true;
            }
            catch (Exception ex)
            {

                Log.Error("DragInitial:" + ex.Message);
            }
        }
        private void DragCanvas_Drop(object sender, DragEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                DragInitial();
                return; 
            }
            DisposeDrag();
        }

        

    }
}