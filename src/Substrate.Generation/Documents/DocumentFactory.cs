using Substrate.Generation.Core.Nodes;

namespace Substrate.Generation.Core.Documents
{
    internal static class DocumentFactory
    {
        public static IEnumerable<IDocument> CreateAll(
            TypeKey key,
            IReadOnlyList<SubstrateNode> nodes)
        {
            // Theme
            if (nodes.Any(n => n is ThemeNode))
                yield return new ThemeDocument(key, nodes);

            // Icons
            if (nodes.Any(n => n is IconPackNode))
                yield return new IconPackDocument(key, nodes);

            // Normal type doc (Notify, DP, etc.)
            if (nodes.Any(n =>
                    n is NotifyNode or DependencyPropertyNode))
            {
                yield return new TypeDocument(key, nodes);
            }
        }
    }
}
