using System.Windows;
using System.Windows.Media;

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
