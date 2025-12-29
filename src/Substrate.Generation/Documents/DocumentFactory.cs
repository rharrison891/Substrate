using Substrate.Generation.Core.Nodes;

namespace Substrate.Generation.Core.Documents
{
    internal static class DocumentFactory
    {
        public static IDocument Create(
            TypeKey key,
            IReadOnlyList<SubstrateNode> nodes)
        {
            return new TypeDocument(key, nodes);
        }
    }
}