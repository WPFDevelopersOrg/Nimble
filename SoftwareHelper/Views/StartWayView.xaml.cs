using SoftwareHelper.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SoftwareHelper.Views
{
    /// <summary>
    /// StartWayView.xaml 的交互逻辑
    /// </summary>
    public partial class StartWayView : Window
    {
        public StartWayView()
        {
            InitializeComponent();
            if (ConfigHelper.StartMode != 0) this.Opacity = 0;
            this.Loaded += StartWayView_Loaded;
        }

        private void StartWayView_Loaded(object sender, RoutedEventArgs e)
        {
            StartMode(ConfigHelper.StartMode);
        }
        void StartMode(int startMode,bool newBegin = false)
        {
            switch (startMode)
            {
                case 0:
                    break;
                case 1:
                    var embedDeasktopView = new EmbedDeasktopView();
                    ShowWindow(embedDeasktopView);
                    break;
                case 2:
                    var mainView = new MainView();
                    ShowWindow(mainView);
                    break;
                default:
                    break;
            }
            if(newBegin)
                ConfigHelper.SaveStartMode(startMode); 
        }

        private void BtnEnterThe_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            StartMode(Convert.ToInt32(btn.Tag),true);
        }
        void ShowWindow(Window win)
        {
            win.Show();
            Thread.Sleep(100);
            this.Close();
        }

        private void ExitClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
