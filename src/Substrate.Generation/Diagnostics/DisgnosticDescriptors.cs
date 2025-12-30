using Microsoft.CodeAnalysis;

internal static class DiagnosticDescriptors
{
    internal static readonly DiagnosticDescriptor LogEntry =
        new(
            id: "LOG001",
            title: "Test log",
            messageFormat: "[{0}]",
            category: "Substrate.Log",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor NotifyFieldShouldBePrivate =
        new(
            id: "SUB001",
            title: "Notify attribute should only be applied to private fields",
            messageFormat: "The 'Notify' attribute should not be applied to '{0}'. It is only valid on private fields.",
            category: "Substrate.Notify",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor DependencyPropertyNotOnDependencyObject =
        new(
            id: "SUB010",
            title: "DependencyProperty requires the containing type to derive from DependencyObject",
            messageFormat: "The DependencyProperty attribute must only be used in types deriving from DependencyObject",
            category: "Substrate.DependencyProperty",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor DependencyPropertyReadonlySuggestion =
        new(
            id: "SUB020",
            title: "Field attributed with DependencyProperty is never used",
            messageFormat: "The field '{0}' is only used as a marker for the generator. Consider marking it as 'readonly'.",
            category: "Substrate.DependencyProperty",
            DiagnosticSeverity.Info,
            isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor MultipleThemeClassesDetected =
        new(
            id: "SUB030",
            title: "Multiple Theme Classes Detected",
            messageFormat: "Only 1 [Theme] class is allowed in a project",
            category: "Substrate.Theme",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor InvalidColorEntry =
        new(
            id: "SUB031",
            title: "Invalid color entry",
            messageFormat: "Theme entry '{0}' is not a valid hex or Color.FromArgb value",
            category: "Substrate.Theme",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true);


}
