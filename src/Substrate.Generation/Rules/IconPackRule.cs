using Microsoft.CodeAnalysis;
using Substrate.Generation.Core.Nodes;

namespace Substrate.Generation.Core.Rules
{
    internal sealed class IconPackRule : IAttributeRule
    {
        public string AttributeName => "IconPack";

        public SubstrateNode? TryCreate(ISymbol symbol, AttributeData attribute, ReportDiagnostic report)
        {
            if (symbol is not INamedTypeSymbol type)
                return null;

            var pack = StaticUtils.GetString(attribute, "pack") ?? "Mdl2Assets";

            var usings = new HashSet<string>
            {
                type.ContainingNamespace.ToDisplayString(),
                "System",
            };

            return new IconPackNode(
                type.ContainingNamespace.ToDisplayString(),
                type.Name,
                pack,
                Location: symbol.Locations.FirstOrDefault(),
                usings
            );
        }
    }
}
