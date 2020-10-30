using SoftWareHelper.Helpers;
using SoftWareHelper.Views;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace SoftWareHelper
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        private string appName;
        protected override void OnStartup(StartupEventArgs e)
        {
            appName = System.IO.Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            bool createdNew = true;
            using (Mutex mutex = new Mutex(true, appName, out createdNew))
            {
                if (createdNew)
                {
                    var main = new MainView();
                    main.Show();
                    Common common = new Common();
                }
                else
                {
                    Process current = Process.GetCurrentProcess();
                    foreach (Process process in Process.GetProcessesByName(current.ProcessName))
                    {
                        if (process.Id != current.Id)
                        {
                            Win32Api.SetForegroundWindow(process.MainWindowHandle);
                            break;
                        }
                    }
                }
            }
            base.OnStartup(e);
        }
       
    }
}
