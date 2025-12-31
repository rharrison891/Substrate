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

            // ⛔ must be on DependencyObject
            if (!StaticUtils.IsOrDerivesFromDependencyObject(containingType))
            {
                report(Diagnostic.Create(
                    DiagnosticDescriptors.DependencyPropertyNotOnDependencyObject,
                    field.Locations.FirstOrDefault()));

                return null;
            }

            // ⚠️ Suggest readonly if not readonly
            if (!field.IsReadOnly)
            {
                report(Diagnostic.Create(
                    DiagnosticDescriptors.DependencyPropertyReadonlySuggestion,
                    field.Locations.FirstOrDefault(),
                    field.Name));
            }

            // 👇 THIS matters — gather usings for generation
            var usings = new HashSet<string>
        {
            containingType.ContainingNamespace.ToDisplayString(),   // the class
            "System",
            "System.Windows"
        };

            // ⭐⭐ IMPORTANT: include the field TYPE namespace
            var fieldNs = field.Type.ContainingNamespace?.ToDisplayString();
            if (!string.IsNullOrWhiteSpace(fieldNs))
                usings.Add(fieldNs!);

            // (optional but nice) include metadata helpers when callbacks exist
            if (StaticUtils.GetBool(attribute, nameof(DependencyPropertyAttribute.HasChangeCallback)) ||
                StaticUtils.GetBool(attribute, nameof(DependencyPropertyAttribute.HasCoerceCallback)))
            {
                usings.Add("System.Windows.Data");
            }

            // 🎛 parse parameters
            var hasChange = StaticUtils.GetBool(attribute, nameof(DependencyPropertyAttribute.HasChangeCallback));
            var hasCoerce = StaticUtils.GetBool(attribute, nameof(DependencyPropertyAttribute.HasCoerceCallback));
            var bindsTwoWay = StaticUtils.GetBool(attribute, nameof(DependencyPropertyAttribute.BindsTwoWayByDefault));
            var isReadOnly = StaticUtils.GetBool(attribute, nameof(DependencyPropertyAttribute.IsReadOnly));
            var defaultValue = StaticUtils.GetString(attribute, nameof(DependencyPropertyAttribute.DefaultValue));

            return new DependencyPropertyNode(
                Namespace: containingType.ContainingNamespace.ToDisplayString(),
                TypeName: containingType.Name,
                FieldName: field.Name,
                FieldType: field.Type.ToDisplayString(),
                HasChangeCallback: hasChange,
                HasCoerceCallback: hasCoerce,
                BindsTwoWayByDefault: bindsTwoWay,
                IsReadOnly: isReadOnly,
                Location: symbol.Locations.FirstOrDefault(),
                DefaultValue: defaultValue,
                Usings: usings
            );
        }
    }
}
