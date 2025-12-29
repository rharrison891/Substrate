using Microsoft.CodeAnalysis;
using Substrate.Generation.Attributes;
using Substrate.Generation.Core.Nodes;

namespace Substrate.Generation.Core.Rules
{
    internal sealed class DependencyPropertyAttributeRule : IAttributeRule
    {
        public string AttributeName => "DependencyProperty";

        public SubstrateNode? TryCreate(
            ISymbol symbol,
            AttributeData attribute,
            ReportDiagnostic report)
        {
            if (symbol is not IFieldSymbol field)
                return null;

            var containingType = field.ContainingType;

            // ✅ Ensure the containing type derives from DependencyObject
            if (!StaticUtils.IsOrDerivesFromDependencyObject(containingType))
            {
                report(Diagnostic.Create(
                    DiagnosticDescriptors.DependencyPropertyNotOnDependencyObject,
                    field.Locations.FirstOrDefault()));

                // Don't generate a DP node for invalid usage
                return null;
            }

            if (!field.IsReadOnly)
            {
                report(Diagnostic.Create(
                    DiagnosticDescriptors.DependencyPropertyReadonlySuggestion,
                    field.Locations.FirstOrDefault(),
                    field.Name));
            }

            // 🔽 your existing arg parsing here
            var hasChange = StaticUtils.GetBool(attribute, nameof(DependencyPropertyAttribute.HasChangeCallback));
            var hasCoerce = StaticUtils.GetBool(attribute, nameof(DependencyPropertyAttribute.HasCoerceCallback));
            var bindsTwoWay = StaticUtils.GetBool(attribute, nameof(DependencyPropertyAttribute.BindsTwoWayByDefault));
            var isReadOnly = StaticUtils.GetBool(attribute, nameof(DependencyPropertyAttribute.IsReadOnly));
            var defaultValue = StaticUtils.GetString(attribute, nameof(DependencyPropertyAttribute.DefaultValue));

            return new DependencyPropertyNode(
                Namespace: containingType.ContainingNamespace.ToDisplayString(),
                TypeName: containingType.Name,
                FieldName: field.Name,
                FieldType: field.Type.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                HasChangeCallback: hasChange,
                HasCoerceCallback: hasCoerce,
                BindsTwoWayByDefault: bindsTwoWay,
                IsReadOnly: isReadOnly,
                DefaultValue: defaultValue
            );
        }

        
    }
}
