using System;

namespace Substrate.Generation.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class DependencyPropertyAttribute : Attribute
    {
        public string? DefaultValue { get; }
        public bool HasChangeCallback { get; }
        public bool HasCoerceCallback { get; }
        public bool BindsTwoWayByDefault { get; }
        public bool IsReadOnly { get; }

        public DependencyPropertyAttribute(
            string? DefaultValue = null,
            bool HasChangeCallback = false,
            bool HasCoerceCallback = false,
            bool BindsTwoWayByDefault = false,
            bool IsReadOnly = false)
        {
            this.DefaultValue = DefaultValue;
            this.HasChangeCallback = HasChangeCallback;
            this.HasCoerceCallback = HasCoerceCallback;
            this.BindsTwoWayByDefault = BindsTwoWayByDefault;
            this.IsReadOnly = IsReadOnly;
        }
    }
}
