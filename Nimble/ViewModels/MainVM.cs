using Nimble.Helpers;
using Nimble.Helpers.MouseHelper;
using Nimble.Models;
using Nimble.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using WPFDevelopers.Controls;
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
            _mouseHook = new MouseHook();
        }

        #endregion

        #region 字段

        private double _currentOpacity;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private const int MousePullInfoIntervalInMs = 10;
        private readonly MouseHook _mouseHook;
        private WindowColor _colorView;
        private Rect _desktopWorkingArea;
        private MousePoint.POINT _point;
        private Process _ffplayProcess;
        private IntPtr _ffplayWindowHandle;
        private readonly string _exitWallpaper = "ExitWallpaper";
        private Color _color;
        private string _hex;
        private IntPtr _workerW = IntPtr.Zero;
        private IntPtr _progman = IntPtr.Zero;
        private IntPtr _desktopListView;
        private const string WORKERW_CLASS = "WorkerW";
        private const string PROGMAN_CLASS = "Progman";
        private const string SHELLDLL_DEFVIEW = "SHELLDLL_DefView";
        private IntPtr _shellView = IntPtr.Zero;

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
            _currentOpacity = ConfigHelper.Opacity;
            MainOpacity = _currentOpacity / 100;
            if (OpacityItemList.Any(x => x.Value == _currentOpacity))
                OpacityItemList.FirstOrDefault(x => x.Value == _currentOpacity).IsSelected = true;
            foreach (var item in OpacityItemList)
            {
                item.PropertyChanged -= Item_PropertyChanged;
                item.PropertyChanged += Item_PropertyChanged;
            }

            IsDark = ThemesHelper.GetLightDark();
            ThemesHelper.SetLightDark(IsDark);
            IsStartup = ConfigHelper.Startup;

            #endregion

            WallpaersFilePlay();

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

            _mouseHook.MouseMove += MouseHook_MouseMove;
            _mouseHook.MouseDown += MouseHook_MouseDown;

            App.Current.Exit += Current_Exit;
        });

        private void Current_Exit(object sender, ExitEventArgs e)
        {
            StopFFplayProcess();
        }

        private void MouseHook_MouseMove(object sender, MouseEventArgs e)
        {
            _point.X = e.Location.X;
            _point.Y = e.Location.Y;
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var model = sender as OpacityItem;
            if (!_currentOpacity.Equals(model.Value))
                if (model.IsSelected)
                {
                    _currentOpacity = model.Value;
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
            Process.Start("https://github.com/WPFDevelopersOrg/Nimble");
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
            _desktopWorkingArea = SystemParameters.WorkArea;

            _colorView = new WindowColor();
            _colorView.Show();
            if (!_timer.IsEnabled)
            {
                _timer.Start();
                _mouseHook.Start();
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

            var screenCaptureExt = new ScreenCaptureExt();
        });

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
                if (wallpaper.VideoPath == _exitWallpaper)
                {
                    if (!wallpaper.IsSelected)
                    {
                        ConfigHelper.SaveOpenWallpaper(false);
                        StopFFplayProcess();
                        wallpaper.ItemName = "壁纸已关闭";
                        wallpaper.IsSelected = false;
                    }
                    else
                    {
                        ConfigHelper.SaveOpenWallpaper(true);
                        WallpaersFilePlay();
                        wallpaper.ItemName = "壁纸已开启";
                        wallpaper.IsSelected = true;
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(wallpaper.VideoPath)) return;
                    if (File.Exists(wallpaper.VideoPath))
                    {
                        ShowWallpaper(wallpaper.VideoPath);
                    }

                }
            }

        });

        #endregion

        #region 方法

        private void Timer_Tick(object sender, EventArgs e)
        {
            _color = Win32Api.GetPixelColor(_point.X, _point.Y);
            var source = PresentationSource.FromVisual(_colorView);
            var dpiX = source.CompositionTarget.TransformToDevice.M11;
            var dpiY = source.CompositionTarget.TransformToDevice.M22;
            if (_point.X / dpiX >= _desktopWorkingArea.Width)
                _colorView.Left = _point.X / dpiX - 40;
            else if (_point.X <= 10)
                _colorView.Left = _point.X / dpiX;
            else
                _colorView.Left = _point.X / dpiX;
            if (_point.Y / dpiY >= _desktopWorkingArea.Height - 40)
                _colorView.Top = _point.Y / dpiY - 40;
            else
                _colorView.Top = _point.Y / dpiY;
            _colorView.MouseColor = new SolidColorBrush(_color);
            _hex = _color.ToString();
            if (_hex.Length > 7)
                _hex = _hex.Remove(1, 2);
            _colorView.MouseColorText = _hex;
        }

        private void MouseHook_MouseDown(object sender, MouseEventArgs e)
        {
            Clipboard.SetText(_hex);
            ShowBalloon("提示", $"已复制到剪切板  {_hex}  Nimble");
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                _mouseHook.Stop();
                if (_colorView != null)
                    _colorView.Close();
            }
        }

        private void ShowBalloon(string title, string message)
        {
            NotifyIcon.ShowBalloonTip(title, message, NotifyIconInfoType.None);
        }

        void ShowWallpaper(string wallpaperPath)
        {
            if (string.IsNullOrWhiteSpace(wallpaperPath))
            {
                if (WallpaperArray.Count >= 3 && ConfigHelper.OpenWallpaper)
                {
                    WallpaperArray.First().IsSelected = true;
                    wallpaperPath = WallpaperArray[0].VideoPath;
                }

            }
            if (!File.Exists(wallpaperPath)) return;
            if (ConfigHelper.WallpaperPath != wallpaperPath)
                ConfigHelper.SaveWallpaperPath(wallpaperPath);
            if (!ConfigHelper.OpenWallpaper) return;
            StopFFplayProcess();
            WallpaperArray.Where(x => x.VideoPath != wallpaperPath && x.VideoPath != _exitWallpaper).ToList().ForEach(x =>
            {
                x.IsSelected = false;
            });
            if (ConfigHelper.OpenWallpaper)
            {
                var wallpaper = WallpaperArray.FirstOrDefault(x => x.VideoPath == _exitWallpaper);
                if (wallpaper != null)
                {
                    wallpaper.ItemName = "壁纸已开启";
                    wallpaper.IsSelected = true;
                }
            }

            StartFFplayProcess(wallpaperPath);

            if (_ffplayWindowHandle != IntPtr.Zero)
            {
                bool success = WallpaperNative.SetWindowToDesktop(_ffplayWindowHandle);
                if (success)
                    Log.Info("setting wallpaper success ");
                else
                    Log.Info("setting wallpaper fail");
            }
        }

        void StartFFplayProcess(string videoFilePath)
        {
            var ffplayPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Core", "ffplay.exe");
            if (!File.Exists(ffplayPath))
                ffplayPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Video", "ffplay.exe");
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = ffplayPath;
            startInfo.Arguments = $"-loop 0 -an -noborder \"{videoFilePath}\"";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            try
            {
                _ffplayProcess = new Process();
                _ffplayProcess.StartInfo = startInfo;

                _ffplayProcess.Start();
                var startTime = DateTime.Now;
                var timeout = TimeSpan.FromSeconds(10);
                while (_ffplayProcess.MainWindowHandle == IntPtr.Zero)
                {
                    if (DateTime.Now - startTime > timeout)
                    {
                        throw new TimeoutException("无法获取到 MainWindowHandle。");
                    }
                    Thread.Sleep(500);
                }
                _ffplayWindowHandle = _ffplayProcess.MainWindowHandle;
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
                if (_ffplayProcess != null && !_ffplayProcess.HasExited)
                {
                    _ffplayProcess.Kill();
                    _ffplayProcess.Dispose();
                    _ffplayProcess = null;
                    _ffplayWindowHandle = IntPtr.Zero;
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error: StopFFplayProcess {ex.Message}");
            }
        }

        void WallpaersFilePlay()
        {
            WallpaperArray = null;
            #region Wallpaper
            var wallpapersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LiveWallpapers");
            if (!Directory.Exists(wallpapersPath))
                wallpapersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Video");
            if (Directory.Exists(wallpapersPath))
            {
                var names = new List<WallpaperItem>();
                var files = Directory.GetFiles(wallpapersPath);
                foreach (var filePath in files)
                {
                    var fileName = Path.GetFileNameWithoutExtension(filePath);
                    names.Add(new WallpaperItem { ItemName = fileName, VideoPath = filePath, IsSelected = ConfigHelper.WallpaperPath == filePath ? true : false });
                }
                if (names.Count > 0)
                    names.Add(new WallpaperItem { ItemName = ConfigHelper.OpenWallpaper == true ? "壁纸已开启" : "壁纸已关闭", VideoPath = _exitWallpaper, IsSelected = ConfigHelper.OpenWallpaper });
                names.Add(new WallpaperItem { ItemName = "更多壁纸请加QQ群：929469013" });
                WallpaperArray = names;
                if (WallpaperArray.Count > 0)
                    ShowWallpaper(ConfigHelper.WallpaperPath);

            }
            #endregion
        }

        #endregion
    }
}