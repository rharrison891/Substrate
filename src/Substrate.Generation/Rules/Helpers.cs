using Microsoft.CodeAnalysis;
using System.Xml.Linq;

internal static class StaticUtils
{
    internal static bool GetNamedBool(AttributeData attribute, string name)
    {
        foreach (var arg in attribute.NamedArguments)
            if (arg.Key == name && arg.Value.Value is bool b)
                return b;

        return false;
    }
    internal static bool GetBool(AttributeData attribute, string name, bool defaultValue = false)
    {
        // named arguments first
        foreach (var arg in attribute.NamedArguments)
            if (arg.Key == name && arg.Value.Value is bool b)
                return b;

        // constructor parameters — look up by PARAMETER NAME
        for (int i = 0; i < attribute.AttributeConstructor?.Parameters.Length; i++)
        {
            var p = attribute.AttributeConstructor.Parameters[i];

            if (p.Name == name && attribute.ConstructorArguments[i].Value is bool b)
                return b;
        }

        return defaultValue;
    }

    internal static bool IsOrDerivesFromDependencyObject(INamedTypeSymbol type)
    {
        var t = type;

        while (t is not null)
        {
            // Fully-qualified comparison: global::System.Windows.DependencyObject
            var fullName = t.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            if (fullName == "global::System.Windows.DependencyObject")
                return true;

            t = t.BaseType;
        }

        return false;
    }

    internal static string? GetString(AttributeData attribute, string name, string? defaultValue = null)
    {
        // named argument case
        foreach (var arg in attribute.NamedArguments)
            if (arg.Key == name && arg.Value.Value is string s)
                return s;

        // positional constructor case
        foreach (var arg in attribute.ConstructorArguments)
            if (arg.Value is string s)
                return s;

        return defaultValue;
    }
}