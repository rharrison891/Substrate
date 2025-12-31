using Microsoft.CodeAnalysis;

using Substrate.Generation.Attributes;
using Substrate.Generation.Core.Nodes;

namespace Substrate.Generation.Core.Rules
{
    internal sealed class NotifyAttributeRule : IAttributeRule
    {
        public string AttributeName => "Notify";

        public SubstrateNode? TryCreate(
            ISymbol symbol,
            AttributeData attribute,
            ReportDiagnostic report)
        {
            if (symbol is not IFieldSymbol field)
                return null;

            if (field.DeclaredAccessibility != Accessibility.Private)
            {
                report(Diagnostic.Create(
                    DiagnosticDescriptors.NotifyFieldShouldBePrivate,
                    field.Locations.FirstOrDefault(),
                    field.Name));
            }

            var containingType = field.ContainingType;

            var ns = containingType.ContainingNamespace?.ToDisplayString() ?? string.Empty;
            var typeName = containingType.Name;

            var fieldTypeDisplay = field.Type.ToDisplayString();

            var implementsINotify =
                containingType.AllInterfaces.Any(i =>
                    i.ToDisplayString() == "System.ComponentModel.INotifyPropertyChanged");

            var createPartials =
                StaticUtils.GetBool(attribute, nameof(NotifyAttribute.CreatePartials));

            //
            // 👇 build the usings set
            //
            var usings = new HashSet<string>
            {
                ns,
                "System",
                "System.ComponentModel"
            };

            // add namespace of the field type (if it has one)
            var fieldNs = field.Type.ContainingNamespace?.ToDisplayString();
            if (!string.IsNullOrWhiteSpace(fieldNs))
                usings.Add(fieldNs!);

            return new NotifyNode(
                Namespace: ns,
                TypeName: typeName,
                FieldName: field.Name,
                FieldType: fieldTypeDisplay,
                ImplementsINotify: implementsINotify,
                CreatePartials: createPartials,
                Location: symbol.Locations.FirstOrDefault(),
                Usings: usings
            );
        }
    }
}
