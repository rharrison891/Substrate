using Substrate.Generation.Core.Documents;

namespace Substrate.Generation.Core.Helpers
{
    internal static class DocumentHelpers
    {
        internal static string GetHintName(TypeKey key)
        {
            // Keep it simple & unique-ish for now
            var prefix = string.IsNullOrEmpty(key.Namespace)
                ? key.TypeName
                : $"{key.Namespace}.{key.TypeName}";

            return $"{prefix}.Notify.g.cs";
        }
        internal static string GetPropertyName(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
                return fieldName;

            var name = fieldName.TrimStart('_');

            if (string.IsNullOrEmpty(name))
                name = fieldName; // fallback

            return char.ToUpperInvariant(name[0]) + name.Substring(1);
        }
    }
}
