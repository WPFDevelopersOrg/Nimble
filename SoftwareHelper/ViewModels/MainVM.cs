using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification;
using SoftwareHelper.Helpers;
using SoftwareHelper.Helpers.MouseHelper;
using SoftwareHelper.Models;
using SoftwareHelper.Views;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace SoftwareHelper.ViewModels
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
        private TaskbarIcon taskbar;

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
        /// 是否嵌入
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
        private bool _isOpenContextMenu = false;

        /// <summary>
        /// 是否关闭
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

        #endregion

        #region 命令

        /// <summary>
        ///     loaded
        /// </summary>
        public ICommand ViewLoaded => new RelayCommand(obj =>
        {
            if (obj != null && obj is TaskbarIcon taskbarIcon)
                taskbar = taskbarIcon;

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
            mouseHook.MouseDown += MouseHook_MouseDown;
        });

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
        public ICommand ExitCommand => new RelayCommand(obj =>
        {
            Environment.Exit(0);
        });

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
            Process.Start("https://github.com/yanjinhuagood/SoftwareHelper");
        });

        /// <summary>
        ///     MouseRightDragCommand
        /// </summary>
        public ICommand MouseRightDragCommand => new RelayCommand(obj =>
        {
            var drag = !IsDragDrop;
            IsDragDrop = drag;
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
            if(IsEmbedded)
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
            Thread.Sleep(1000);
            var screenCut = new WPFDevelopers.Controls.ScreenCut();
            screenCut.ShowDialog();

        });
        #endregion

        #region 方法

        private void Timer_Tick(object sender, EventArgs e)
        {
            var point = new MousePoint.POINT();
            var isMouseDown = MousePoint.GetCursorPos(out point);
            var color = Win32Api.GetPixelColor(point.X, point.Y);

            if (point.X >= desktopWorkingArea.Width)
                colorView.Left = point.X - 40;
            else if (point.X <= 10)
                colorView.Left = point.X;
            else
                colorView.Left = point.X;

            if (point.Y >= desktopWorkingArea.Height - 40)
                colorView.Top = point.Y - 40;
            else
                colorView.Top = point.Y;

            colorView.MouseColor = new SolidColorBrush(color);
        }

        private void MouseHook_MouseDown(object sender, MouseEventArgs e)
        {
            Clipboard.SetText(colorView.MouseColor.ToString());
            ShowBalloon("提示", $"已复制到剪切板  {colorView.MouseColor.ToString()}  SoftwareHelper");
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                mouseHook.Stop();
                if (colorView != null) colorView.Close();
            }
        }

        private void ShowBalloon(string title, string message)
        {
            if (taskbar != null) taskbar.ShowBalloonTip(title, message, BalloonIcon.None);
        }

        #endregion
    }
}