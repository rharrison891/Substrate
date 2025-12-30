using System;
using System.Collections.Generic;
using System.Text;

using Substrate.Generation.Core.Nodes;

namespace Substrate.Generation.Core.Documents
{
    internal sealed class ThemeNodeBuilder : NodeBuilder<ThemeNode>
    {
        public ThemeNodeBuilder(TypeDocument parent) : base(parent) { }

        public override void Build(StringBuilder sb)
        {
            // Only one ThemeNode should ever exist, so just take first
            var node = Nodes[0];

            sb.BlankLine();
            sb.AppendLine("public enum ThemeColor");
            sb.AppendLine("{");

            foreach (var c in node.Colors)
                sb.AppendLine($"    {c.Key},");

            sb.AppendLine("}");
        }
    }
}
