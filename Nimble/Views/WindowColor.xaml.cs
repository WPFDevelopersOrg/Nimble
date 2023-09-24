using System.Windows;
using System.Windows.Media;

namespace Nimble.Views
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

        public static readonly DependencyProperty MouseColorProperty =
            DependencyProperty.Register("MouseColor", typeof(Brush), typeof(WindowColor), new PropertyMetadata(null));



        public string MouseColorText
        {
            get { return (string)GetValue(MouseColorTextProperty); }
            set { SetValue(MouseColorTextProperty, value); }
        }

        public static readonly DependencyProperty MouseColorTextProperty =
            DependencyProperty.Register("MouseColorText", typeof(string), typeof(WindowColor), new PropertyMetadata(null));


        public WindowColor()
        {
            InitializeComponent();
        }
    }
}
