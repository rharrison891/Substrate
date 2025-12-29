using Substrate.Generation.Attributes;
using System.Windows;

namespace Substrate.Generation.Test
{
    public partial class Person : DependencyObject
    {
        [Notify]
        private string? _name;

        [Notify(CreatePartials: true)]
        private string? _email;

        [DependencyProperty(IsReadOnly: true)]
        private readonly string _title;

        [DependencyProperty(HasCoerceCallback: true, HasChangeCallback: true)]
        private readonly string _firstName;

        [DependencyProperty(HasCoerceCallback: true, HasChangeCallback: true)]
        private readonly string _lastName;

    }
}
