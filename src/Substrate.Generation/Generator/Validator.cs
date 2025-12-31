using Microsoft.CodeAnalysis;
using Substrate.Generation.Core.Nodes;

namespace Substrate.Generation.Core.Generator
{
    internal static class Validator
    {
        public static (IncrementalValuesProvider<T>,IncrementalValuesProvider<Diagnostic?>)
            ValidatePipeline<T>(
            IncrementalValuesProvider<(SubstrateNode? Node, Diagnostic? Diagnostic)> pipeline,
            DiagnosticDescriptor descriptor)
            where T : SubstrateNode
        {
            var nodes = pipeline
                .Where(t => t.Node is T)
                .Select((t, _) => (T)t.Node!)
                .Collect();

            var validated = nodes.SelectMany((batches, _) =>
            {
                var output = new List<(T? node, Diagnostic? diag)>();
                var groups = batches.GroupBy(n => new { n.Namespace, n.TypeName });
                foreach (var g in groups)
                {
                    var first = g.First();
                    output.Add((first, null));

                    foreach (var extra in g.Skip(1))
                    {
                        output.Add((
                            null,
                            Diagnostic.Create(
                                descriptor,
                                extra.Location,
                                extra.TypeName)));
                    }
                }
                return output;
            });
            var good =
            validated
                .Where(t => t.node is not null)
                .Select((t, _) => t.node!);

            var diagnostics =
                validated
                    .Where(t => t.diag is not null)
                    .Select((t, _) => t.diag!);
            return (good, diagnostics);
        }
    }
}
