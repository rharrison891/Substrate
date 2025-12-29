using System;

namespace Substrate.Generation.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class NotifyAttribute : Attribute
    {
        public bool CreatePartials { get; }

        public NotifyAttribute(bool CreatePartials = false)
        {
            this.CreatePartials = CreatePartials;
        }
    }
}
