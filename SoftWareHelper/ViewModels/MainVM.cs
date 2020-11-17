using Microsoft.Windows.Shell;
using SoftWareHelper.Helpers;
using SoftWareHelper.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace SoftWareHelper.ViewModels
{
    public class MainVM : ViewModelBase
    {

        #region 静态属性
        private double currentOpacity;
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

        #endregion

        #region 构造

        #endregion

        #region 命令
        /// <summary>
        /// loaded
        /// </summary>    
        public ICommand ViewLoaded => new RelayCommand(obj =>
        {
            #region Themes
            OpacityItemList = new ObservableCollection<OpacityItem>()
            {
                new OpacityItem{ Value = 100.00 },
                new OpacityItem{ Value = 80.00 },
                new OpacityItem{ Value = 60.00 },
                new OpacityItem{ Value = 40.00 }
            };
            currentOpacity = ThemesHelper.GetOpacity();
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
            IsDark = ThemesHelper.GetConfig();
            ThemesHelper.SetLightDark(IsDark); 
            #endregion

            Common.TemporaryFile();
            ApplicationList = Common.AllApplictionInstalled();
            string json = JsonHelper.Serialize(ApplicationList);
            FileHelper.WriteFile(json, Common.temporaryApplicationJson);

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
                    MainOpacity = model.Value / 100;
                }
              
            }
        }

        /// <summary>
        /// SelectionChangedCommand
        /// </summary>
        public ICommand SelectionChangedCommand => new RelayCommand(obj =>
        {
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
            Application.Current.Shutdown();
        });
        /// <summary>
        /// ThemesCommand
        /// </summary>
        public ICommand ThemesCommand => new RelayCommand(obj =>
        {
            var dark = !ThemesHelper.GetConfig();
            IsDark = dark;
            ThemesHelper.SetLightDark(dark);
        });
        /// <summary>
        /// GithubCommand
        /// </summary>
        public ICommand GithubCommand => new RelayCommand(obj =>
        {
            Process.Start("https://github.com/yanjinhuagood/SoftWareHelper");
        });
        #endregion

        #region 方法

        #endregion

    }
}
