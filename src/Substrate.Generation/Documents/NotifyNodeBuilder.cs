using Substrate.Generation.Core.Helpers;
using Substrate.Generation.Core.Nodes;
using System.Text;

namespace Substrate.Generation.Core.Documents
{
    internal sealed class NotifyNodeBuilder : NodeBuilder<NotifyNode>
    {
        public NotifyNodeBuilder(TypeDocument parent) : base(parent) { }

        public override void Build(StringBuilder sb)
        {
            //Notify implementation
            if (!Nodes.Any(n => n.ImplementsINotify))
            {
                EmitNotifyInfrastructure(sb, 1);
                sb.BlankLine();
            }

            sb.StartRegion("Properties");

            foreach (var node in Nodes)
            {
                if(node.CreatePartials)
                {
                    var propName = DocumentHelpers.GetPropertyName(node.FieldName);
                    Hooks?.AddNotifyHooks(propName, node.FieldType);
                }
                EmitProperty(sb, node, 1);
                
            }

            sb.EndRegion();
        }
        private static void EmitProperty(StringBuilder sb, NotifyNode node, int tabs)
        {
            var propName = DocumentHelpers.GetPropertyName(node.FieldName);

            sb.AppendIndented(tabs, $"public {node.FieldType} {propName}");
            sb.OpenBrace(tabs);

            sb.AppendIndented(tabs + 1, $"get => {node.FieldName};");

            sb.AppendIndented(tabs + 1, "set");
            sb.OpenBrace(tabs + 1);

            if (node.CreatePartials)
            {
                sb.AppendIndented(tabs + 2, "var cancel = false;");
                sb.AppendIndented(tabs + 2, "var coerced = value;");
                sb.BlankLine();

                sb.AppendIndented(tabs + 2,
                    $"On{propName}Changing({node.FieldName}, ref coerced, ref cancel);");
                sb.AppendIndented(tabs + 2, "if (cancel) return;");
                sb.BlankLine();

                sb.AppendIndented(tabs + 2, $"var oldValue = {node.FieldName};");
                sb.AppendIndented(tabs + 2, $"if (global::System.Collections.Generic.EqualityComparer<{node.FieldType}>.Default.Equals(oldValue, coerced))");
                sb.AppendIndented(tabs + 3, "return;");
                sb.BlankLine();

                sb.AppendIndented(tabs + 2, $"{node.FieldName} = coerced;");
                sb.AppendIndented(tabs + 2, $"On{propName}Changed(oldValue, coerced);");
            }
            else
            {
                sb.AppendIndented(tabs + 2, $"var oldValue = {node.FieldName};");
                sb.AppendIndented(tabs + 2,
                    $"if (global::System.Collections.Generic.EqualityComparer<{node.FieldType}>.Default.Equals(oldValue, value))");
                sb.AppendIndented(tabs + 3, "return;");
                sb.BlankLine();

                sb.AppendIndented(tabs + 2, $"{node.FieldName} = value;");
            }

            sb.AppendIndented(tabs + 2, $"OnPropertyChanged(nameof({propName}));");

            sb.CloseBrace(tabs + 1);
            sb.CloseBrace(tabs);
            sb.BlankLine();
        }

        private static void EmitNotifyInfrastructure(StringBuilder sb, int tabs)
        {
            sb.AppendIndented(tabs,
                "public event global::System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;");

            sb.BlankLine();

            sb.AppendIndented(tabs,
                "protected virtual void OnPropertyChanged(string propertyName)");
            sb.OpenBrace(tabs);

            sb.AppendIndented(
                tabs + 1,
                "PropertyChanged?.Invoke(this, new global::System.ComponentModel.PropertyChangedEventArgs(propertyName));");

            sb.CloseBrace(tabs);
        }

    }
}
