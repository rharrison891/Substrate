namespace Substrate.Generation.Core.Nodes
{
    public abstract record SubstrateNode(
        string Namespace,
        string TypeName
    );

    public sealed record NotifyNode(
        string Namespace,
        string TypeName,
        string FieldName,
        string FieldType,
        bool ImplementsINotify,
        bool CreatePartials
    ) : SubstrateNode(Namespace,TypeName);

    public sealed record DependencyPropertyNode(
        string Namespace,
        string TypeName,
        string FieldName,
        string FieldType,
        bool HasChangeCallback,
        bool HasCoerceCallback,
        bool BindsTwoWayByDefault,
        bool IsReadOnly,
        string? DefaultValue
    ) : SubstrateNode(Namespace,TypeName);

    public sealed record ThemeNode(
        string Namespace,
        string TypeName,
        IReadOnlyList<(string Key, int A, int R, int G, int B)> Colors,
        bool UsesFallbackPalette,
        Microsoft.CodeAnalysis.Location? Location
    ) : SubstrateNode(Namespace, TypeName);
}
