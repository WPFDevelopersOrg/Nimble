using SoftWareHelper.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SoftWareHelper.Views
{
    /// <summary>
    /// EmbedDeasktopView.xaml 的交互逻辑
    /// </summary>
    public partial class EmbedDeasktopView : Window
    {
        public EmbedDeasktopView()
        {
            InitializeComponent();
            this.Loaded += EmbedDeasktopView_Loaded;
            this.Closing += EmbedDeasktopView_Closing;
        }

        private void EmbedDeasktopView_Loaded(object sender, RoutedEventArgs e)
        {
            #region 注释
            WindowInteropHelper wndHelper = new WindowInteropHelper(this);
            int exStyle = (int)Win32Api.GetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE);
            exStyle |= (int)Win32Api.ExtendedWindowStyles.WS_EX_TOOLWINDOW;
            Win32Api.SetWindowLong(wndHelper.Handle, (int)Win32Api.GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);
            #endregion
        }

        private void EmbedDeasktopView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Win32Api.UnRegisterDesktop(this);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            Win32Api.RegisterDesktop(this);
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            System.Environment.Exit(0);
        }
    }
}
