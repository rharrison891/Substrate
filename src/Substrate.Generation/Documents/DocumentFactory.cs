using Substrate.Generation.Core.Nodes;

namespace Substrate.Generation.Core.Documents
{
    internal static class DocumentFactory
    {
        public static IDocument Create(
            TypeKey key,
            IReadOnlyList<SubstrateNode> nodes)
        {
            if (nodes.Any(n => n is ThemeNode))
                return new ThemeDocument(key, nodes);

            return new TypeDocument(key, nodes);
        }
    }
}
