using Microsoft.CodeAnalysis;
using Substrate.Generation.Attributes;
using Substrate.Generation.Core.Nodes;
using System;

namespace Substrate.Generation.Core.Rules
{
    
    internal sealed class NotifyAttributeRule : IAttributeRule
    {
        public string AttributeName => "Notify";

        public SubstrateNode? TryCreate(ISymbol symbol, AttributeData attribute, ReportDiagnostic report)
        {
            if (symbol is not IFieldSymbol field)
                return null;

            if (field.DeclaredAccessibility != Accessibility.Private)
                report(Diagnostic.Create(
                    DiagnosticDescriptors.NotifyFieldShouldBePrivate,
                    field.Locations.FirstOrDefault(),
                    field.Name));
            

            var containingType = field.ContainingType;
            var ns = containingType.ContainingNamespace?.ToDisplayString() ?? string.Empty;
            var typeName = containingType.Name;

            // Default ToDisplayString() gives nice C# aliases (string, int, etc.)
            var fieldType = field.Type.ToDisplayString();

            var implementsINotify =
                containingType.AllInterfaces.Any(i =>
                    i.ToDisplayString() == "System.ComponentModel.INotifyPropertyChanged");

            var createPartials =
                StaticUtils.GetBool(attribute, nameof(NotifyAttribute.CreatePartials));

            return new NotifyNode(
                ns,
                typeName,
                field.Name,
                field.Type.ToDisplayString(),
                implementsINotify,
                createPartials
            );
        }
    }
}