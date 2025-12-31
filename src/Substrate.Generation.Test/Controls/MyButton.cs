using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Substrate.Generation.Attributes;

namespace Substrate.Generation.Test.Controls
{
    public partial class MyButton:Control
    {
        [DependencyProperty]
        private readonly Brush _iconBrush;

        [DependencyProperty]
        private ICommand _command;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            MouseLeftButtonDown += (s, e) => _command?.Execute(e);
        }

    }
}
