using Nimble.Helpers;
using Nimble.Views;
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

namespace Nimble
{
    /// <summary>
    ///     App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private string appName;
        public EventWaitHandle ProgramStarted { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
            appName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.FileName);
            ProgramStarted = new EventWaitHandle(false, EventResetMode.AutoReset, appName, out var createNew);
            if (!createNew)
            {
                // 唤起已经启动的进程
                var current = Process.GetCurrentProcess();
                foreach (var process in Process.GetProcessesByName(current.ProcessName))
                    if (process.Id != current.Id)
                    {
                        Win32Api.SetForegroundWindow(process.MainWindowHandle);
                        Current.Shutdown();
                        Environment.Exit(-1);
                        break;
                    }
            }
            else
            {
#if !DEBUG
                var autoUpdater = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoUpdater.exe");
                if (File.Exists(autoUpdater))
                {
                    var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                    Log.Debug($"版本号：{version}");
                    Process.Start(autoUpdater, version);
                }
                else
                    Log.Warn("警告:未找到 AutoUpdater.exe，无法及时更新到最新版。");
#endif
                //FPS设置为20
                //Timeline.DesiredFrameRateProperty.OverrideMetadata(typeof(Timeline),
                //    new FrameworkPropertyMetadata { DefaultValue = 20 });
                var common = new Common();
                ConfigHelper.GetConfigHelper();
                var main = new StartView();
                main.Show();
            }

            base.OnStartup(e);
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            Win32Api.UnRegisterDesktop(true);
        }
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;   
                Log.Error("UI线程异常:" + e.Exception.Message);
            }
            catch (Exception ex)
            {
                Log.Error("UI线程发生致命错误！" + ex.Message);
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var sbEx = new StringBuilder();
            if (e.IsTerminating) sbEx.Append("非UI线程发生致命错误");
            sbEx.Append("非UI线程异常：");
            if (e.ExceptionObject is Exception)
                sbEx.Append(((Exception)e.ExceptionObject).Message);
            else
                sbEx.Append(e.ExceptionObject);
            Log.Error(sbEx.ToString());
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            Log.Error("Task线程异常：" + e.Exception.Message);
            e.SetObserved(); 
        }

    }
}