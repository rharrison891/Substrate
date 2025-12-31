using Microsoft.CodeAnalysis;

namespace Substrate.Generation.Core.Nodes
{
    public abstract record SubstrateNode(
        string Namespace,
        string TypeName,
        IReadOnlyCollection<string> Usings,
        Microsoft.CodeAnalysis.Location? Location
    );

    public sealed record NotifyNode(
        string Namespace,
        string TypeName,
        string FieldName,
        string FieldType,
        bool ImplementsINotify,
        bool CreatePartials,
        Microsoft.CodeAnalysis.Location? Location,
        IReadOnlyCollection<string> Usings
    ) : SubstrateNode(Namespace, TypeName, Usings, Location);

    public sealed record DependencyPropertyNode(
        string Namespace,
        string TypeName,
        string FieldName,
        string FieldType,
        bool HasChangeCallback,
        bool HasCoerceCallback,
        bool BindsTwoWayByDefault,
        bool IsReadOnly,
        string? DefaultValue,
        Microsoft.CodeAnalysis.Location? Location,
        IReadOnlyCollection<string> Usings
    ) : SubstrateNode(Namespace, TypeName, Usings, Location);

    public sealed record ThemeNode(
        string Namespace,
        string TypeName,
        IReadOnlyList<(string Key, int A, int R, int G, int B)> Colors,
        bool UsesFallbackPalette,
        Microsoft.CodeAnalysis.Location? Location,
        IReadOnlyCollection<string> Usings
    ) : SubstrateNode(Namespace, TypeName, Usings, Location);

    public sealed record IconPackNode(
        string Namespace,
        string TypeName,
        string Pack,
        Microsoft.CodeAnalysis.Location? Location,
        IReadOnlyCollection<string> Usings
    ) : SubstrateNode(Namespace, TypeName, Usings, Location);
}
