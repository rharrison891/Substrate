using Microsoft.CodeAnalysis;
using Substrate.Generation.Core.Nodes;

namespace Substrate.Generation.Core.Rules
{
    internal delegate void ReportDiagnostic(Diagnostic diagnostic);

    interface IAttributeRule
    {
        string AttributeName { get; }
        SubstrateNode? TryCreate(
            ISymbol symbol,
            AttributeData attribute,
            ReportDiagnostic report);
    }
}