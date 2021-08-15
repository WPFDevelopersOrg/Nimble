using SoftwareHelper.Helpers;
using SoftwareHelper.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace SoftwareHelper
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public EventWaitHandle ProgramStarted { get; set; }
        private string appName;
        protected override void OnStartup(StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = System.Windows.Interop.RenderMode.SoftwareOnly;
            appName = System.IO.Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, appName, out var createNew);
            if (!createNew)
            {
                // 唤起已经启动的进程
                Process current = Process.GetCurrentProcess();
                foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                {
                    if (process.Id != current.Id)
                    {
                        Win32Api.SetForegroundWindow(process.MainWindowHandle);
                        App.Current.Shutdown();
                        Environment.Exit(-1);
                        break;
                    }
                }

            }
            else
            {
#if !DEBUG
                var autoUpdater = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoUpdater.exe");
                if (File.Exists(autoUpdater))
                {
                    Process.Start(autoUpdater, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString());
                }
                else
                    Log.Error("警告:未找到AutoUpdater.exe，无法及时更新到最新版。");
#endif
                //FPS设置为20
                //Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                //    new FrameworkPropertyMetadata { DefaultValue = 20 });
                Common common = new Common();
                ConfigHelper.GetConfigHelper();
                appShortcutToStartup();
                var main = new StartView();
                main.Show();
                //WindowInteropHelper wndHelper = new WindowInteropHelper(main);
                //IntPtr handle = wndHelper.Handle;
                //int exStyle = (int)Win32Api.GetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE);
                //exStyle |= (int)Win32Api.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
                //Win32Api.SetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

                
            }

            base.OnStartup(e);

            //UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

        }


        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true; //把 Handled 属性设为true，表示此异常已处理，程序可以继续运行，不会强制退出      
                Log.Error("UI线程异常:" + e.Exception.Message);
            }
            catch (Exception ex)
            {
                //此时程序出现严重异常，将强制结束退出
                Log.Error("UI线程发生致命错误！" + ex.Message);
            }

        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            StringBuilder sbEx = new StringBuilder();
            if (e.IsTerminating)
            {
                sbEx.Append("非UI线程发生致命错误");
            }
            sbEx.Append("非UI线程异常：");
            if (e.ExceptionObject is Exception)
            {
                sbEx.Append(((Exception)e.ExceptionObject).Message);
            }
            else
            {
                sbEx.Append(e.ExceptionObject);
            }
            Log.Error(sbEx.ToString());
        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            //task线程内未处理捕获
            Log.Error("Task线程异常：" + e.Exception.Message);
            e.SetObserved();//设置该异常已察觉（这样处理后就不会引起程序崩溃）
        }
        void appShortcutToStartup()
        {
            string startupDir = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            if (!Directory.Exists(startupDir)) return;
            var path = startupDir + "\\" + "SoftwareHelperStart" + ".url";
            if (!System.IO.File.Exists(path))
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    writer.WriteLine("[InternetShortcut]");
                    writer.WriteLine("URL=file:///" + app);
                    writer.WriteLine("IconIndex=0");
                    string icon = app.Replace('\\', '/');
                    writer.WriteLine("IconFile=" + icon);
                }
            }

        }
    }
}
