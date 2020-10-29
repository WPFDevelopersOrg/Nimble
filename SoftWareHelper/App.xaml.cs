using SoftWareHelper.Helpers;
using System.Windows;

namespace SoftWareHelper
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Common common = new Common();
        }
    }
}
