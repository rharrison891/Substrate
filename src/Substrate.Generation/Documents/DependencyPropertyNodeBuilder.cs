using Substrate.Generation.Core.Helpers;
using Substrate.Generation.Core.Nodes;

using System.Text;

namespace Substrate.Generation.Core.Documents
{
    internal sealed class DependencyPropertyNodeBuilder : NodeBuilder<DependencyPropertyNode>
    {
        public DependencyPropertyNodeBuilder(TypeDocument parent) : base(parent) { }

        public override void Build(StringBuilder sb)
        {
            if (Nodes.Count == 0)
                return;

            sb.StartRegion("Dependency Properties");

            foreach (var node in Nodes)
            {
                var propName = DocumentHelpers.GetPropertyName(node.FieldName);

                if (node.HasChangeCallback && !node.IsReadOnly)
                    Hooks?.AddDependencyChangedHook(propName, node.FieldType);

                if (node.HasCoerceCallback && !node.IsReadOnly)
                    Hooks?.AddDependencyCoerceHook(propName, node.FieldType);

                EmitDependencyProperty(sb, node, 1);
            }

            sb.EndRegion();
        }

        private static string GetMetaArgs(DependencyPropertyNode node)
        {
            var metadataArgs = new List<string>();
            var propName = DocumentHelpers.GetPropertyName(node.FieldName);
            if (node.DefaultValue is { } dv)
                metadataArgs.Add(dv);
            else
                metadataArgs.Add("default");

            if (node.HasChangeCallback)
                metadataArgs.Add($"On{propName}ChangedInternal");
            else
                metadataArgs.Add("null");

            if (node.HasCoerceCallback)
                metadataArgs.Add($"On{propName}CoerceInternal");
            else
                metadataArgs.Add("null");


            return string.Join(", ", metadataArgs);
        }

        private static void EmitDependencyProperty(StringBuilder sb, DependencyPropertyNode node, int tabs)
        {
            var propName = DocumentHelpers.GetPropertyName(node.FieldName);

            if (node.IsReadOnly)
                EmitReadOnlyDependencyProperty(sb, node, propName, tabs);
            else
                EmitStandardDependencyProperty(sb, node, propName, tabs);

            sb.BlankLine();
        }

        private static void EmitReadOnlyDependencyProperty(
            StringBuilder sb,
            DependencyPropertyNode node,
            string propName,
            int tabs)
        {
            // KEY field
            sb.AppendIndented(tabs,
                $"private static readonly global::System.Windows.DependencyPropertyKey {propName}PropertyKey =");
            sb.AppendIndented(tabs + 1,
                $"global::System.Windows.DependencyProperty.RegisterReadOnly(");
            sb.AppendIndented(tabs + 2, $"nameof({propName}),");
            sb.AppendIndented(tabs + 2, $"typeof({node.FieldType}),");
            sb.AppendIndented(tabs + 2, $"typeof({node.TypeName}),");
            sb.AppendIndented(tabs + 2,
                $"new global::System.Windows.PropertyMetadata({GetMetaArgs(node)}));");
            sb.BlankLine();

            // Public DP wrapper
            sb.AppendIndented(tabs,
                $"public static readonly global::System.Windows.DependencyProperty {propName}Property =");
            sb.AppendIndented(tabs + 1,
                $"{propName}PropertyKey.DependencyProperty;");
            sb.BlankLine();

            // CLR wrapper
            sb.AppendIndented(tabs, $"public {node.FieldType} {propName}");
            sb.OpenBrace(tabs);
            sb.AppendIndented(tabs + 1,
                $"get => ({node.FieldType})GetValue({propName}Property);");
            sb.CloseBrace(tabs);

            sb.BlankLine();

            // internal implementation that always performs the actual set
            sb.AppendIndented(tabs,
                $"internal void Set{propName}Internal({node.FieldType} value)");
            sb.OpenBrace(tabs);

            sb.AppendIndented(tabs + 1,
                $"SetValue({propName}PropertyKey, value);");

            sb.CloseBrace(tabs);
        }

        private static void EmitStandardDependencyProperty(StringBuilder sb, DependencyPropertyNode node, string propName, int tabs)
        {
            sb.AppendIndented(tabs,
                $"public static readonly global::System.Windows.DependencyProperty {propName}Property =");
            sb.AppendIndented(tabs + 1,
                $"global::System.Windows.DependencyProperty.Register(");
            sb.AppendIndented(tabs + 2, $"nameof({propName}),");
            sb.AppendIndented(tabs + 2, $"typeof({node.FieldType}),");
            sb.AppendIndented(tabs + 2, $"typeof({node.TypeName}),");
            sb.AppendIndented(tabs + 2,
                $"new global::System.Windows.PropertyMetadata({GetMetaArgs(node)}));");
            sb.BlankLine();

            // CLR wrapper
            sb.AppendIndented(tabs, $"public {node.FieldType} {propName}");
            sb.OpenBrace(tabs);

            sb.AppendIndented(tabs + 1, $"get => ({node.FieldType})GetValue({propName}Property);");
            sb.AppendIndented(tabs + 1, $"set => SetValue({propName}Property, value);");

            sb.CloseBrace(tabs);

            if (node.HasChangeCallback)
                EmitChangeHandler(sb, propName, node.TypeName, node.FieldType, tabs);

            if (node.HasCoerceCallback)
                EmitCoerceHandler(sb, propName, node.TypeName, node.FieldType, tabs);

            sb.BlankLine();
        }

        private static void EmitCoerceHandler(
            StringBuilder sb,
            string propName,
            string typeName,
            string propType,
            int tabs)
        {
            sb.BlankLine();
            sb.AppendIndented(tabs,
                $"private static object On{propName}CoerceInternal(" +
                "global::System.Windows.DependencyObject d, object baseValue)");
            sb.OpenBrace(tabs);

            sb.AppendIndented(tabs + 1, $"if (d is {typeName} typed)");
            sb.OpenBrace(tabs + 1);

            sb.AppendIndented(tabs + 2, $"var coerced = ({propType})baseValue;");
            sb.AppendIndented(tabs + 2, $"typed.On{propName}Coerce(ref coerced);");
            sb.AppendIndented(tabs + 2, $"return coerced;");

            sb.CloseBrace(tabs + 1);

            sb.AppendIndented(tabs + 1, "return baseValue;");
            sb.CloseBrace(tabs);
        }

        private static void EmitChangeHandler(StringBuilder sb, string propName, string typeName, string propType, int tabs)
        {
            sb.BlankLine();
            sb.AppendIndented(tabs,
                $"private static void On{propName}ChangedInternal(" +
                "global::System.Windows.DependencyObject d, " +
                "global::System.Windows.DependencyPropertyChangedEventArgs e)");
            sb.OpenBrace(tabs);

            sb.AppendIndented(tabs + 1, $"if (d is {typeName} typed)");
            sb.OpenBrace(tabs + 1);

            sb.AppendIndented(tabs + 2,
                $"typed.On{propName}Changed((" +
                $"{propType}?)e.OldValue, ({propType}?)e.NewValue);");

            sb.CloseBrace(tabs + 1);
            sb.CloseBrace(tabs);
        }
    }
}
