using Substrate.Generation.Core.Nodes;
using System.Text;

namespace Substrate.Generation.Core.Documents
{
    internal sealed class PartialHooksBuilder : NodeBuilder<SubstrateNode>
    {
        private sealed record Hook(string PropertyName, string TypeName);

        private readonly List<Hook> _notifyHooks = new();
        private readonly List<Hook> _dpChangedHooks = new();
        private readonly List<Hook> _dpCoerceHooks = new();

        public PartialHooksBuilder(TypeDocument parent) : base(parent) { }

        public void AddNotifyHooks(string propertyName, string typeName)
            => _notifyHooks.Add(new Hook(propertyName, typeName));

        public void AddDependencyChangedHook(string propertyName, string typeName)
            => _dpChangedHooks.Add(new Hook(propertyName, typeName));

        public void AddDependencyCoerceHook(string propertyName, string typeName)
            => _dpCoerceHooks.Add(new Hook(propertyName, typeName));

        public override void Build(StringBuilder sb)
        {
            if (_notifyHooks.Count == 0 &&
                _dpChangedHooks.Count == 0 &&
                _dpCoerceHooks.Count == 0)
                return;

            sb.StartRegion("Hooks");

            // Notify hooks (changing + changed)
            foreach (var h in _notifyHooks)
            {
                sb.AppendIndented(1,
                    $"partial void On{h.PropertyName}Changing({h.TypeName} oldValue, ref {h.TypeName} newValue, ref bool cancel);");

                sb.AppendIndented(1,
                    $"partial void On{h.PropertyName}Changed({h.TypeName} oldValue, {h.TypeName} newValue);");
            }

            // DependencyProperty changed hooks
            foreach (var h in _dpChangedHooks)
            {
                sb.AppendIndented(1,
                    $"partial void On{h.PropertyName}Changed({h.TypeName}? oldValue, {h.TypeName}? newValue);");
            }

            // DependencyProperty coerce hooks
            foreach (var h in _dpCoerceHooks)
            {
                sb.AppendIndented(1,
                    $"partial void On{h.PropertyName}Coerce(ref {h.TypeName} value);");
            }

            sb.BlankLine();
            sb.EndRegion();
        }
    }
}