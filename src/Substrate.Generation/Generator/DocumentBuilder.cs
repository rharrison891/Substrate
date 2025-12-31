using System.Collections.Immutable;

using Microsoft.CodeAnalysis;

using Substrate.Generation.Core.Documents;
using Substrate.Generation.Core.Nodes;

namespace Substrate.Generation.Core.Generator
{
    internal static class IncrementalConcatExtensions
    {
        public static IncrementalValuesProvider<T> Concat<T>(
            this IncrementalValuesProvider<T> a,
            IncrementalValuesProvider<T> b)
        {
            // collect both, combine, then flatten
            return a.Collect()
                    .Combine(b.Collect())
                    .SelectMany(static (pair, _) =>
                        pair.Left.Concat(pair.Right));
        }

        internal static IncrementalValuesProvider<SubstrateNode>
            ToSubstrateNodes<T>(this IncrementalValuesProvider<T> pipe)
            where T : SubstrateNode
            => pipe.Select((n, _) => (SubstrateNode)n);
    }

    internal static class DocumentBuilder
    {
        public static IncrementalValuesProvider<IDocument> BuildDocuments(
            params IncrementalValuesProvider<SubstrateNode>[] nodePipelines)
        {
            // merge all node streams
            var merged = nodePipelines.Aggregate((a, b) => a.Concat(b));

            // collect once — then group + build
            return merged
                .Collect()
                .SelectMany(static (nodes, _) =>
                {
                    var groups = nodes.GroupBy(n => new TypeKey(n.Namespace, n.TypeName));

                    return groups
                        .SelectMany(g =>
                            DocumentFactory.CreateAll(
                                g.Key,
                                g.Cast<SubstrateNode>().ToList()))
                        .ToImmutableArray();
                });
        }
    }
}
