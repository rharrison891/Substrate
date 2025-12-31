using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Substrate.Generation.Attributes;

namespace Substrate.Generation.Test.Controls
{
    public partial class MyButton : Control
    {
        [DependencyProperty]
        private readonly Brush _iconBrush;

        [DependencyProperty]
        private readonly ICommand _command;

        [DependencyProperty(IsReadOnly: true)]
        private readonly string _glyph;

        [DependencyProperty(DefaultValue: "Icons.Warning", HasChangeCallback: true)]
        private readonly Icons _icon;

        partial void OnIconChanged(Icons? oldValue, Icons? newValue)
        {
            if (newValue is { } icon)
                SetGlyphInternal(icon.AsGlyph());
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            MouseLeftButtonDown += (s, e) => _command?.Execute(e);
        }
    }
}
