using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

using Substrate.Generation.Attributes;

namespace Substrate.Generation.Test
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var anim = new DoubleAnimation(.25, TimeSpan.FromSeconds(1)) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever };
            if (GetTemplateChild("LayoutRoot") is Border border && border.BorderBrush is SolidColorBrush brush)
            {
                border.BorderBrush = brush.Clone();
                border.BorderBrush.BeginAnimation(SolidColorBrush.OpacityProperty, anim);
            }
        }

        private Random _random = new Random();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            (int R, int G, int B) color = (_random.Next(0, 255), _random.Next(0, 255), _random.Next(0, 255));
            AppTheme.SetColor(ThemeColor.Blue, Color.FromRgb((byte)color.R, (byte)color.G, (byte)color.B));
        }
    }
}
