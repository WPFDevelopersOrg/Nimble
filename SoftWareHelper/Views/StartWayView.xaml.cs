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

namespace SoftWareHelper.Views
{
    /// <summary>
    /// StartWayView.xaml 的交互逻辑
    /// </summary>
    public partial class StartWayView : Window
    {
        public StartWayView()
        {
            InitializeComponent();
        }
        private void BtnEnterThe_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Tag)
            {
                case "EmbedDeasktopView":
                    var embedDeasktopView = new EmbedDeasktopView();
                    ShowWindow(embedDeasktopView);
                    break;
                case "MainView":
                    var mainView = new MainView();
                    ShowWindow(mainView);
                    break;
                default:
                    break;
            }
        }
        void ShowWindow(Window win)
        {
            win.Show();
            Thread.Sleep(100);
            this.Close();
        }
    }
}
