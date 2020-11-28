using SoftWareHelper.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SoftWareHelper.Views
{
    /// <summary>
    /// StartView.xaml 的交互逻辑
    /// </summary>
    public partial class StartView : Window
    {
        DispatcherTimer timer = new DispatcherTimer();
        ImageBrush backgroundBrush = new ImageBrush();
        public StartView()
        {
            InitializeComponent();
            myCanvas.Focus();

            timer.Tick += Engine;
            timer.Interval = TimeSpan.FromMilliseconds(20);
            backgroundBrush.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Images/timg.jpg"));
            background.Fill = backgroundBrush;
            background2.Fill = backgroundBrush;
            Start();
            this.Loaded += StartView_Loaded;
        }

        private void StartView_Loaded(object sender, RoutedEventArgs e)
        {
            var bw = new BackgroundWorker();

            bw.DoWork += (s, y) =>
            {

                Common.TemporaryFile();
                Common.ApplicationListCache = Common.AllApplictionInstalled();
                Common.GetDesktopAppliction(Common.ApplicationListCache);

                string json = JsonHelper.Serialize(Common.ApplicationListCache);
                FileHelper.WriteFile(json, Common.temporaryApplicationJson);
                Thread.Sleep(6000);
            };

            bw.RunWorkerCompleted += (s, y) => 
            {
                tbMsg.Text = "开始体验";
                timer.Stop();
                MainView mView = new MainView();
                mView.Show();

                var closeAnimation = new DoubleAnimation 
                {
                    From = this.Width,
                    To = 0,
                    Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                    EasingFunction = new BackEase { EasingMode= EasingMode.EaseIn }
                };
                closeAnimation.Completed += (s1, e1) =>
                {
                    this.Close();
                };
                //this.BeginAnimation(Window.OpacityProperty, closeAnimation);
                this.BeginAnimation(Window.WidthProperty, closeAnimation);
            };
            tbMsg.Text = "即将进入";
            bw.RunWorkerAsync();
        }

        private void Engine(object sender, EventArgs e)
        {
            var backgroundLeft = Canvas.GetLeft(background) - 3;
            var background2Left = Canvas.GetLeft(background2) - 3;
            Canvas.SetLeft(background, backgroundLeft);
            Canvas.SetLeft(background2, background2Left);
            Canvas.SetLeft(tb1, Canvas.GetLeft(tb1) - 3);
            Canvas.SetLeft(tb2, Canvas.GetLeft(tb2) - 3);
            Canvas.SetLeft(tb3, Canvas.GetLeft(tb3) - 3);
            if (backgroundLeft <= -1262)
            {
                timer.Stop();
                Start();
            }

        }
        private void Start()
        {
            Canvas.SetLeft(background, 0);
            Canvas.SetLeft(background2, 1262);

            timer.Start();
        }
    }
}
