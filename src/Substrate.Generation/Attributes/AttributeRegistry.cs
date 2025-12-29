using Microsoft.CodeAnalysis;
using Substrate.Generation.Core.Nodes;
using Substrate.Generation.Core.Rules;

namespace Substrate.Generation.Core.Attributes
{
    internal static class AttributeRegistry
    {
        private static readonly Dictionary<string, IAttributeRule> _rules = new();

        public static void Register(IAttributeRule rule)
            => _rules[rule.AttributeName] = rule;

        public static bool TryGet(string name, out IAttributeRule rule)
            => _rules.TryGetValue(name, out rule);

        internal static IncrementalValuesProvider<(SubstrateNode? Node, Diagnostic? Diagnostic)>
            FilterByRegisteredAttributes<TSymbol>(
                this IncrementalValuesProvider<TSymbol> pipe)
            where TSymbol : ISymbol
        {
            return pipe.SelectMany(static (symbol, _) =>
            {
                var results = new List<(SubstrateNode?, Diagnostic?)>();

                foreach (var attr in symbol.GetAttributes())
                {
                    var name = attr.AttributeClass?.Name;
                    if (name?.EndsWith("Attribute") == true)
                        name = name.Substring(0, name.Length - 9);

                    if (name is null) continue;

                    if (AttributeRegistry.TryGet(name, out var rule))
                    {
                        Diagnostic? diagnostic = null;

                        var node = rule.TryCreate(
                            symbol,
                            attr,
                            d => diagnostic = d);   // capture instead of report

                        results.Add((node, diagnostic));
                    }
                }

                return results;
            });
        }
    }
}