using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Nimble.Helpers;
using Nimble.Helpers.MouseHelper;
using Nimble.Models;
using Nimble.Views;
using WPFDevelopers.Controls;
using WPFDevelopers.Controls.ScreenCapturer;
using WPFDevelopers.Helpers;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace Nimble.ViewModels
{
    public class MainVM : ViewModelBase
    {
        #region 构造

        public MainVM()
        {
            _timer.Interval = TimeSpan.FromMilliseconds(MousePullInfoIntervalInMs);
            _timer.Tick += Timer_Tick;
            mouseHook = new MouseHook();
        }

        #endregion

        #region 字段

        private double currentOpacity;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private const int MousePullInfoIntervalInMs = 10;
        private readonly MouseHook mouseHook;
        private WindowColor colorView;
        private Rect desktopWorkingArea;
        private MousePoint.POINT point;
        private IntPtr desktopHandle;
        private Process ffplayProcess;
        private IntPtr ffplayWindowHandle;

        #endregion

        #region 属性

        private ObservableCollection<ApplicationModel> _applicationList;

        /// <summary>
        ///     所有应用集合
        /// </summary>
        public ObservableCollection<ApplicationModel> ApplicationList
        {
            get => _applicationList;
            set
            {
                _applicationList = value;
                NotifyPropertyChange("ApplicationList");
            }
        }

        private bool _isDark;

        /// <summary>
        ///     当前为Dark还是Light
        /// </summary>
        public bool IsDark
        {
            get => _isDark;
            set
            {
                _isDark = value;
                NotifyPropertyChange("IsDark");
            }
        }

        private ObservableCollection<OpacityItem> _opacityItemList;

        /// <summary>
        ///     透明度
        /// </summary>
        public ObservableCollection<OpacityItem> OpacityItemList
        {
            get => _opacityItemList;
            set
            {
                _opacityItemList = value;
                NotifyPropertyChange("OpacityItemList");
            }
        }

        private double _mainOpacity;

        /// <summary>
        ///     透明度
        /// </summary>
        public double MainOpacity
        {
            get => _mainOpacity;
            set
            {
                _mainOpacity = value;
                NotifyPropertyChange("MainOpacity");
            }
        }

        private bool _isDragDrop;

        /// <summary>
        ///     是否拖动
        /// </summary>
        public bool IsDragDrop
        {
            get => _isDragDrop;
            set
            {
                _isDragDrop = value;
                NotifyPropertyChange("IsDragDrop");
            }
        }

        private bool _isEmbedded = true;

        /// <summary>
        ///     是否嵌入
        /// </summary>
        public bool IsEmbedded
        {
            get => _isEmbedded;
            set
            {
                _isEmbedded = value;
                NotifyPropertyChange("IsEmbedded");
            }
        }

        private bool _isStartup = true;

        /// <summary>
        ///     是否自启
        /// </summary>
        public bool IsStartup
        {
            get => _isStartup;
            set
            {
                _isStartup = value;
                NotifyPropertyChange("IsStartup");
            }
        }

        private bool _isOpenContextMenu;

        /// <summary>
        ///     是否关闭
        /// </summary>
        public bool IsOpenContextMenu
        {
            get => _isOpenContextMenu;
            set
            {
                _isOpenContextMenu = value;
                NotifyPropertyChange("IsOpenContextMenu");
            }
        }

        private Cursor _cursor = Cursors.SizeAll;

        /// <summary>
        ///     鼠标样式
        /// </summary>
        public Cursor Cursor
        {
            get => _cursor;
            set
            {
                _cursor = value;
                NotifyPropertyChange("Cursor");
            }
        }

        private List<WallpaperItem> _wallpaperArray;

        /// <summary>
        ///     动态壁纸集合
        /// </summary>
        public List<WallpaperItem> WallpaperArray
        {
            get => _wallpaperArray;
            set
            {
                _wallpaperArray = value;
                NotifyPropertyChange("WallpaperArray");
            }
        }

        #endregion

        #region 命令

        /// <summary>
        ///     loaded
        /// </summary>
        public ICommand ViewLoaded => new RelayCommand(obj =>
        {
            #region Themes

            OpacityItemList = new ObservableCollection<OpacityItem>
        {
            new OpacityItem { Value = 100.00 },
            new OpacityItem { Value = 80.00 },
            new OpacityItem { Value = 60.00 },
            new OpacityItem { Value = 40.00 }
        };
            currentOpacity = ConfigHelper.Opacity;
            MainOpacity = currentOpacity / 100;
            if (OpacityItemList.Any(x => x.Value == currentOpacity))
                OpacityItemList.FirstOrDefault(x => x.Value == currentOpacity).IsSelected = true;
            foreach (var item in OpacityItemList)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                item.PropertyChanged += Item_PropertyChanged;
            }

            IsDark = ThemesHelper.GetLightDark();
            ThemesHelper.SetLightDark(IsDark);
            IsStartup = ConfigHelper.Startup;

            #endregion

            #region Wallpaper
            var wallpaersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Video");
            if (Directory.Exists(wallpaersPath))
            {
                var names = new List<WallpaperItem>();
                var files = Directory.GetFiles(wallpaersPath);
                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    names.Add(new WallpaperItem { ItemName = fileName, VideoPath = filePath, IsSelected = ConfigHelper.WallpaperPath == filePath ? true : false });
                }
                names.Add(new WallpaperItem { ItemName = "更多壁纸请加QQ群：929469013" });
                WallpaperArray = names;
                if (WallpaperArray.Count > 0)
                    ShowWallpaper(ConfigHelper.WallpaperPath);
            }
            #endregion

            if (Common.ApplicationListCache == null)
            {
                Action initAction = Common.Init;
                var result = initAction.BeginInvoke(null, null);
                initAction.EndInvoke(result);
                ApplicationList = Common.ApplicationListCache;
            }
            else
            {
                ApplicationList = Common.ApplicationListCache;
            }

            mouseHook.MouseMove += MouseHook_MouseMove;
            mouseHook.MouseDown += MouseHook_MouseDown;

            App.Current.Exit += Current_Exit;
        });

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            StopFFplayProcess();
        }

        private void MouseHook_MouseMove(object sender, MouseEventArgs e)
        {
            point.X = e.Location.X;
            point.Y = e.Location.Y;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = sender as OpacityItem;
            if (!currentOpacity.Equals(model.Value))
                if (model.IsSelected)
                {
                    currentOpacity = model.Value;
                    foreach (var item in OpacityItemList)
                        if (item.Value != model.Value)
                            item.IsSelected = false;
                    if (ConfigHelper.Opacity != model.Value) ConfigHelper.SaveOpacity(model.Value);
                    MainOpacity = model.Value / 100;
                }
        }

        /// <summary>
        ///     SelectionChangedCommand
        /// </summary>
        public ICommand SelectionChangedCommand => new RelayCommand(obj =>
        {
            if (IsDragDrop)
                return;
            var model = obj as ApplicationModel;
            Process.Start(model.ExePath);
        });

        /// <summary>
        ///     ExitCommand
        /// </summary>
        public ICommand ExitCommand => new RelayCommand(obj => { Environment.Exit(0); });

        /// <summary>
        ///     ThemesCommand
        /// </summary>
        public ICommand ThemesCommand => new RelayCommand(obj =>
        {
            var dark = !ThemesHelper.GetLightDark();
            IsDark = dark;
            ThemesHelper.SetLightDark(dark);
        });

        /// <summary>
        ///     GithubCommand
        /// </summary>
        public ICommand GithubCommand => new RelayCommand(obj =>
        {
            Process.Start("https://github.com/yanjinhuagood/Nimble");
        });

        /// <summary>
        ///     MouseRightDragCommand
        /// </summary>
        public ICommand MouseRightDragCommand => new RelayCommand(obj =>
        {
            var drag = !IsDragDrop;
            IsDragDrop = drag;
            if (!IsDragDrop)
                Cursor = Cursors.SizeAll;
            else
                Cursor = Cursors.No;
        });

        /// <summary>
        ///     RemoveApplictionCommand
        /// </summary>
        public ICommand RemoveApplictionCommand => new RelayCommand(obj =>
        {
            var model = obj as ApplicationModel;
            ApplicationList.Remove(model);
        });

        /// <summary>
        ///     ColorCommand
        /// </summary>
        public ICommand ColorCommand => new RelayCommand(obj =>
        {
            desktopWorkingArea = SystemParameters.WorkArea;

            colorView = new WindowColor();
            colorView.Show();
            if (!_timer.IsEnabled)
            {
                _timer.Start();
                mouseHook.Start();
            }
        });

        /// <summary>
        ///     EmbeddedCommand
        /// </summary>
        public ICommand EmbeddedCommand => new RelayCommand(obj =>
        {
            IsEmbedded = !IsEmbedded;
            if (IsEmbedded)

                Win32Api.RegisterDesktop();
            else
                Win32Api.UnRegisterDesktop();
        });

        /// <summary>
        ///     ScreenCutCommand
        /// </summary>
        public ICommand ScreenCutCommand => new RelayCommand(obj =>
        {
            IsOpenContextMenu = false;
            Keyboard.ClearFocus();
            Application.Current.Dispatcher.Invoke(new Action(delegate
             {
                 var screenCapturer = new ScreenCapture();
                 screenCapturer.SnapCompleted += ScreenCapturer_SnapCompleted;
                 screenCapturer.SnapCanceled += ScreenCapturer_SnapCanceled;
                 screenCapturer.Capture();
             }));
            //var screenCapturer = new ScreenCapture();
            //screenCapturer.Capture();
        });

        private void ScreenCapturer_SnapCanceled()
        {
        }

        private void ScreenCapturer_SnapCompleted(CroppedBitmap bitmap)
        {
        }

        /// <summary>
        ///     StartUpCommand
        /// </summary>
        public ICommand StartUpCommand => new RelayCommand(obj =>
        {
            IsStartup = !IsStartup;
            if (IsStartup)
                Common.AppShortcutToStartup(IsStartup);
            else
                Common.AppShortcutToStartup();
            ConfigHelper.SaveStartup(IsStartup);
        });

        /// <summary>
        ///     WallpaperSelectedCommand
        /// </summary>
        public ICommand WallpaperSelectedCommand => new RelayCommand(obj =>
        {
            if (obj is WallpaperItem wallpaper)
            {
                if (string.IsNullOrWhiteSpace(wallpaper.VideoPath)) return;
                ShowWallpaper(wallpaper.VideoPath);
            }

        });

        #endregion

        #region 方法

        void ShowWallpaper(string wallpaperPath)
        {
            if (string.IsNullOrWhiteSpace(wallpaperPath)) return;
            StopFFplayProcess();
            WallpaperArray.Where(x => x.VideoPath != wallpaperPath).ToList().ForEach(x =>
            {
                x.IsSelected = false;
            });
            StartFFplayProcess(wallpaperPath);
            if (ffplayWindowHandle != IntPtr.Zero)
            {
                SendMsgToProgman();
                Win32Api.SetParent(ffplayWindowHandle, desktopHandle);
                if (ConfigHelper.WallpaperPath != wallpaperPath)
                    ConfigHelper.SaveWallpaperPath(wallpaperPath);
            }
        }

        private Color color;
        private string hex;

        private void Timer_Tick(object sender, EventArgs e)
        {
            //var point = new MousePoint.POINT();
            //var isMouseDown = MousePoint.GetCursorPos(out point);
            //color = Win32Api.GetPixelColor(point.X, point.Y);
            //var source = PresentationSource.FromVisual(colorView);
            //var dpiX = source.CompositionTarget.TransformToDevice.M11;
            //var dpiY = source.CompositionTarget.TransformToDevice.M22;
            //if (point.X / dpiX >= desktopWorkingArea.Width)
            //    colorView.Left = point.X / dpiX - 40;
            //else if (point.X <= 10)
            //    colorView.Left = point.X / dpiX;
            //else
            //    colorView.Left = point.X / dpiX;
            //if (point.Y / dpiY >= desktopWorkingArea.Height - 40)
            //    colorView.Top = point.Y / dpiY - 40;
            //else
            //    colorView.Top = point.Y / dpiY;

            color = Win32Api.GetPixelColor(point.X, point.Y);
            var source = PresentationSource.FromVisual(colorView);
            var dpiX = source.CompositionTarget.TransformToDevice.M11;
            var dpiY = source.CompositionTarget.TransformToDevice.M22;
            if (point.X / dpiX >= desktopWorkingArea.Width)
                colorView.Left = point.X / dpiX - 40;
            else if (point.X <= 10)
                colorView.Left = point.X / dpiX;
            else
                colorView.Left = point.X / dpiX;
            if (point.Y / dpiY >= desktopWorkingArea.Height - 40)
                colorView.Top = point.Y / dpiY - 40;
            else
                colorView.Top = point.Y / dpiY;
            colorView.MouseColor = new SolidColorBrush(color);
            hex = color.ToString();
            if (hex.Length > 7)
                hex = hex.Remove(1, 2);
            colorView.MouseColorText = hex;
        }

        private void MouseHook_MouseDown(object sender, MouseEventArgs e)
        {
            Clipboard.SetText(hex);
            ShowBalloon("提示", $"已复制到剪切板  {hex}  Nimble");
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                mouseHook.Stop();
                if (colorView != null) colorView.Close();
            }
        }

        private void ShowBalloon(string title, string message)
        {
            NotifyIcon.ShowBalloonTip(title, message, NotifyIconInfoType.None);
        }


        void SendMsgToProgman()
        {
            desktopHandle = Win32.FindWindow("Progman", null);
            IntPtr result = IntPtr.Zero;
            Win32Api.SendMessageTimeout(desktopHandle, 0x52c, IntPtr.Zero, IntPtr.Zero, 0, 2, result);
            Win32Api.EnumWindows(EnumWindowsCallback, IntPtr.Zero);
        }

        bool EnumWindowsCallback(IntPtr hwnd, IntPtr lParam)
        {
            if (Win32Api.FindWindowEx(hwnd, IntPtr.Zero, "SHELLDLL_DefView", null) != IntPtr.Zero)
            {
                IntPtr workerW = Win32Api.FindWindowEx(IntPtr.Zero, hwnd, "WorkerW", null);
                Win32Api.ShowWindow(workerW, 0);
            }
            return true;
        }

        void StartFFplayProcess(string videoFilePath)
        {
            var ffplayPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DLL", "ffplay.exe");
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = ffplayPath;
            //startInfo.WindowStyle = ProcessWindowStyle.Maximized;
            startInfo.Arguments = $"-loop 0 -fs \"{videoFilePath}\" ";
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            try
            {
                ffplayProcess = new Process();
                ffplayProcess.StartInfo = startInfo;

                ffplayProcess.Start();
                var startTime = DateTime.Now;
                var timeout = TimeSpan.FromSeconds(10);
                while (ffplayProcess.MainWindowHandle == IntPtr.Zero)
                {
                    if (DateTime.Now - startTime > timeout)
                    {
                        throw new TimeoutException("无法获取到 MainWindowHandle。");
                    }
                    Thread.Sleep(500);
                }
                ffplayWindowHandle = ffplayProcess.MainWindowHandle;
            }
            catch (Exception ex)
            {
                Log.Error($"Error: StartFFplayProcess {ex.Message}");
            }
        }
        void StopFFplayProcess()
        {
            try
            {
                if (ffplayProcess != null && !ffplayProcess.HasExited)
                {
                    ffplayProcess.Kill();
                    ffplayProcess.Dispose();
                    ffplayProcess = null;
                    ffplayWindowHandle = IntPtr.Zero;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error: StopFFplayProcess {ex.Message}");
            }
        }
        #endregion
    }
}