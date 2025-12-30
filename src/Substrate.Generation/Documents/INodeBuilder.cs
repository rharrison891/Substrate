using Substrate.Generation.Core.Nodes;
using System.Text;

namespace Substrate.Generation.Core.Documents
{
    internal interface INodeBuilder
    {
        bool Accepts(SubstrateNode node);   // should this builder handle it?
        void Add(SubstrateNode node);       // collect nodes
        bool HasOutput { get; }             // does it produce anything?

        // New phased build hooks
        void BuildBeforeClass(StringBuilder sb);
        void BuildInsideClass(StringBuilder sb);
        void BuildAfterClass(StringBuilder sb);
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

        // New phased build hooks with sane defaults:
        // by default everything behaves like before and builds INSIDE the class.
        public virtual void BuildBeforeClass(StringBuilder sb)
        {
            // default: nothing
            Build(sb);
        }

        public virtual void BuildInsideClass(StringBuilder sb)
        {
            // default: call old-style Build for backwards compatibility
            Build(sb);
        }

        public virtual void BuildAfterClass(StringBuilder sb)
        {
            // default: nothing
        }

        // Existing abstract method used by current builders.
        // They don't need to change yet.
        public abstract void Build(StringBuilder sb);
    }
}
