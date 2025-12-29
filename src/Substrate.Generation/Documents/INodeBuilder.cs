using Substrate.Generation.Core.Nodes;
using System.Text;

namespace Substrate.Generation.Core.Documents
{
    internal interface INodeBuilder
    {
        bool Accepts(SubstrateNode node);   // should this builder handle it?
        void Add(SubstrateNode node);       // collect nodes
        bool HasOutput { get; }             // does it produce anything?
        void Build(StringBuilder sb);       // write code into the document
    }

    internal abstract class NodeBuilder<TNode> : INodeBuilder
    where TNode : SubstrateNode
    {
        private PartialHooksBuilder? _hooks;
        protected readonly TypeDocument Parent;
        protected readonly List<TNode> Nodes = new();
        protected PartialHooksBuilder Hooks
            => _hooks ??= Parent.GetBuilder<PartialHooksBuilder>()!;

        protected NodeBuilder(TypeDocument parent)
            => Parent = parent;

        public bool Accepts(SubstrateNode node) => node is TNode;

        public void Add(SubstrateNode node)
            => Nodes.Add((TNode)node);

        public bool HasOutput => Nodes.Count > 0;


        public abstract void Build(StringBuilder sb);

    }
}
