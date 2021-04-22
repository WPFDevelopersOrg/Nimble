using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Windows.Shell;
using SoftwareHelper.Helpers;
using SoftwareHelper.Helpers.MouseHelper;
using SoftwareHelper.Models;
using SoftwareHelper.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SoftwareHelper.ViewModels
{
    public class MainVM : ViewModelBase
    {

        #region 字段
        private double currentOpacity;
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private const int MousePullInfoIntervalInMs = 10;
        private MouseHook mouseHook;
        private WindowColor colorView;
        private Rect desktopWorkingArea;
        private TaskbarIcon taskbar;
        #endregion

        #region 属性
        private ObservableCollection<ApplicationModel> _applicationList;
        /// <summary>
        /// 所有应用集合
        /// </summary>
        public ObservableCollection<ApplicationModel> ApplicationList
        {
            get { return _applicationList; }
            set
            {
                _applicationList = value;
                this.NotifyPropertyChange("ApplicationList");
            }
        }

        private bool _isDark;
        /// <summary>
        /// 当前为Dark还是Light
        /// </summary>
        public bool IsDark
        {
            get { return _isDark; }
            set
            {
                _isDark = value;
                this.NotifyPropertyChange("IsDark");
            }
        }

        private ObservableCollection<OpacityItem> _opacityItemList;
        /// <summary>
        /// 透明度
        /// </summary>
        public ObservableCollection<OpacityItem> OpacityItemList
        {
            get { return _opacityItemList; }
            set
            {
                _opacityItemList = value;
                this.NotifyPropertyChange("OpacityItemList");
            }
        }

        private double _mainOpacity;
        /// <summary>
        /// 透明度
        /// </summary>
        public double MainOpacity
        {
            get { return _mainOpacity; }
            set
            {
                _mainOpacity = value;
                this.NotifyPropertyChange("MainOpacity");
            }
        }

        private bool _isDragDrop;
        /// <summary>
        /// 是否拖动
        /// </summary>
        public bool IsDragDrop
        {
            get { return _isDragDrop; }
            set
            {
                _isDragDrop = value;
                this.NotifyPropertyChange("IsDragDrop");
            }
        }
        //private bool _isEdgeHide;
        ///// <summary>
        ///// 是否边缘隐藏
        ///// </summary>
        //public bool IsEdgeHide
        //{
        //    get { return _isEdgeHide; }
        //    set
        //    {
        //        _isEdgeHide = value;
        //        this.NotifyPropertyChange("IsEdgeHide");
        //    }
        //}
        #endregion

        #region 构造
        public MainVM()
        {
            _timer.Interval = TimeSpan.FromMilliseconds(MousePullInfoIntervalInMs);
            _timer.Tick += Timer_Tick;
            mouseHook = new MouseHook();
        }
        #endregion

        #region 命令
        /// <summary>
        /// loaded
        /// </summary>    
        public ICommand ViewLoaded => new RelayCommand(obj =>
        {
            if (obj != null && obj is TaskbarIcon taskbarIcon)
                taskbar = taskbarIcon;
            #region Themes
            OpacityItemList = new ObservableCollection<OpacityItem>()
            {
                new OpacityItem{ Value = 100.00 },
                new OpacityItem{ Value = 80.00 },
                new OpacityItem{ Value = 60.00 },
                new OpacityItem{ Value = 40.00 }
            };
            currentOpacity = ConfigHelper.Opacity;
            MainOpacity = currentOpacity / 100;
            if (OpacityItemList.Any(x => x.Value == currentOpacity))
            {
                OpacityItemList.FirstOrDefault(x => x.Value == currentOpacity).IsSelected = true;
            }
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
                Action initAction = new Action(Common.Init);
                IAsyncResult result = initAction.BeginInvoke(null, null);
                initAction.EndInvoke(result);
                ApplicationList = Common.ApplicationListCache;
            }
            else
            {
                ApplicationList = Common.ApplicationListCache;
            }
            mouseHook.MouseDown += MouseHook_MouseDown;
            //if (!File.Exists(Common.temporaryApplicationJson))
            //{
            //    ApplicationList = Common.AllApplictionInstalled();
            //    string json = JsonHelper.Serialize(ApplicationList);
            //    FileHelper.WriteFile(json, Common.temporaryApplicationJson);
            //}
            //else
            //{
            //    string json = FileHelper.ReadFile(Common.temporaryApplicationJson);
            //    ApplicationList = JsonHelper.Deserialize<ObservableCollection<ApplicationModel>>(json);
            //}
        });

        private void Item_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var model = sender as OpacityItem;
            if (!currentOpacity.Equals(model.Value))
            {
                if (model.IsSelected)
                {
                    currentOpacity = model.Value;
                    foreach (var item in OpacityItemList)
                    {
                        if (item.Value != model.Value)
                        {
                            item.IsSelected = false;
                        }
                    }
                    //if (OpacityItemList.Any(x => x.Value == currentOpacity))
                    //{
                    //    OpacityItemList.FirstOrDefault(x => x.Value == currentOpacity).IsSelected = true;
                    //}
                    if (ConfigHelper.Opacity != model.Value)
                    {
                        ConfigHelper.SaveOpacity(model.Value);
                    }
                    MainOpacity = model.Value / 100;

                }

            }
        }

        /// <summary>
        /// SelectionChangedCommand
        /// </summary>
        public ICommand SelectionChangedCommand => new RelayCommand(obj =>
        {
            if (IsDragDrop)
                return;
            ApplicationModel model = obj as ApplicationModel;
            //ApplicationList.Move(ApplicationList.IndexOf(model),0);

            Process.Start(model.ExePath);
        });
        /// <summary>
        /// ExitCommand
        /// </summary>
        public ICommand ExitCommand => new RelayCommand(obj =>
        {
            //Environment.Exit(-1);
            //Application.Current.Shutdown(0);
            //taskbar.Dispose();
            System.Environment.Exit(0);
        });
        /// <summary>
        /// ThemesCommand
        /// </summary>
        public ICommand ThemesCommand => new RelayCommand(obj =>
        {
            var dark = !ThemesHelper.GetLightDark();
            IsDark = dark;
            ThemesHelper.SetLightDark(dark);
        });
        /// <summary>
        /// GithubCommand
        /// </summary>
        public ICommand GithubCommand => new RelayCommand(obj =>
        {
            Process.Start("chrome.exe", "https://github.com/yanjinhuagood/SoftwareHelper");
        });

        /// <summary>
        /// MouseRightDragCommand
        /// </summary>
        public ICommand MouseRightDragCommand => new RelayCommand(obj =>
        {
            var drag = !IsDragDrop;
            IsDragDrop = drag;
        });
        /// <summary>
        /// RemoveApplictionCommand
        /// </summary>
        public ICommand RemoveApplictionCommand => new RelayCommand(obj =>
        {
            ApplicationModel model = obj as ApplicationModel;
            ApplicationList.Remove(model);
        });

        /// <summary>
        ///ColorCommand 
        /// </summary>
        public ICommand ColorCommand => new RelayCommand(obj =>
        {
            desktopWorkingArea = System.Windows.SystemParameters.WorkArea;

            colorView = new WindowColor();
            colorView.Show();
            //Mouse.OverrideCursor = Cursors.Pen;
            if (!_timer.IsEnabled)
            {
                _timer.Start();
                mouseHook.Start();
            }
        });
        #endregion

        #region 方法
        private void Timer_Tick(object sender, EventArgs e)
        {
            MousePoint.POINT point = new MousePoint.POINT();
            var isMouseDown = MousePoint.GetCursorPos(out point);
            var color = Win32Api.GetPixelColor(point.X, point.Y);

            if (point.X >= desktopWorkingArea.Width)
                colorView.Left = point.X - 40;
            else if (point.X <= 10)
                colorView.Left = point.X;
            else
                colorView.Left = point.X;

            if (point.Y >= (desktopWorkingArea.Height - 40))
                colorView.Top = point.Y - 40;
            else
                colorView.Top = point.Y;

            colorView.MouseColor = new SolidColorBrush(color);
        }

        private void MouseHook_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Clipboard.SetText(colorView.MouseColor.ToString());
            ShowBalloon("提示", $"已复制到剪切板  {colorView.MouseColor.ToString()}  SoftwareHelper");
            if (_timer.IsEnabled)
            {
                _timer.Stop();
                mouseHook.Stop();
                if (colorView != null)
                {
                    colorView.Close();
                }
            }
        }
        private void ShowBalloon(string title, string message)
        {
            if (taskbar != null) taskbar.ShowBalloonTip(title, message, BalloonIcon.None);
        }
        #endregion

    }
}
