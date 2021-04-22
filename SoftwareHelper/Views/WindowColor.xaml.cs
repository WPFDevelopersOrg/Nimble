using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// WindowColor.xaml 的交互逻辑
    /// </summary>
    public partial class WindowColor : Window
    {

        public Brush MouseColor
        {
            get { return (Brush)GetValue(MouseColorProperty); }
            set { SetValue(MouseColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MouseColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MouseColorProperty =
            DependencyProperty.Register("MouseColor", typeof(Brush), typeof(WindowColor), new PropertyMetadata(null));


        public WindowColor()
        {
            InitializeComponent();
        }
    }
}
