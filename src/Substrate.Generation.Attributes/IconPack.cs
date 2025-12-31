using System;

namespace Substrate.Generation.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class IconPackAttribute : Attribute
    {
        public string Pack { get; }
        public IconPackAttribute()
            => Pack = "Mdl2Assets";
        public IconPackAttribute(string pack)
            => Pack = pack;
    }
}
