using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Substrate.Generation.Core.Attributes;
using Substrate.Generation.Core.Documents;
using Substrate.Generation.Core.Nodes;
using Substrate.Generation.Core.Rules;
using System.Collections.Immutable;
using System.Data;
using static Microsoft.CodeAnalysis.CSharp.SyntaxTokenParser;

namespace Substrate.Generation.Core.Generator
{
    internal static class PipelineFactory
    {

        internal static void CreatePipelines(IncrementalGeneratorInitializationContext context)
        {
            AttributeRegistry.Register(new NotifyAttributeRule());
            AttributeRegistry.Register(new DependencyPropertyAttributeRule());


            var fieldsRaw =
                context.CreateDeclarationPipeline<VariableDeclaratorSyntax, IFieldSymbol>()
                       .FilterByRegisteredAttributes();

            var methodsRaw =
                context.CreateDeclarationPipeline<MethodDeclarationSyntax, IMethodSymbol>()
                       .FilterByRegisteredAttributes();

            // nodes only
            var fieldsPipe = fieldsRaw.Where(t => t.Node is not null).Select((t, _) => t.Node!);
            var methodsPipe = methodsRaw.Where(t => t.Node is not null).Select((t, _) => t.Node!);

            var collected = fieldsPipe
                .Collect()
                .Combine(methodsPipe.Collect());

            var documents = collected.SelectMany(static (pair, _) =>
            {
                var all = pair.Left.AddRange(pair.Right);

                var groups = all.GroupBy(n => new TypeKey(n.Namespace, n.TypeName));

                return groups
                    .Select(g => DocumentFactory.Create(
                        g.Key,
                        g.Cast<SubstrateNode>().ToList()))
                    .ToImmutableArray();
            });

            context.RegisterSourceOutput(
                documents,
                static (spc, doc) =>
                {
                    spc.AddSource(doc.HintName, doc.Build());
                });


            // diagnostics only
            var fieldDiagnostics =
                fieldsRaw.Select((r, _) => r.Diagnostic).Collect();

            var methodDiagnostics =
                methodsRaw.Select((r, _) => r.Diagnostic).Collect();

            context.RegisterSourceOutput(
                FlattenDiagnostics(fieldDiagnostics, methodDiagnostics),
                static (spc, diag) => spc.ReportDiagnostic(diag));
        }

        internal static IncrementalValuesProvider<TSymbol>
            CreateDeclarationPipeline<TSyntax, TSymbol>(
                this IncrementalGeneratorInitializationContext context)
            where TSyntax : SyntaxNode
            where TSymbol : class, ISymbol
        {
            return context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (node, _) => node is TSyntax,
                    transform: static (ctx, _) =>
                        ctx.SemanticModel.GetDeclaredSymbol((TSyntax)ctx.Node) as TSymbol)
                .Where(s => s is not null)!;
        }

        internal static IncrementalValuesProvider<(SubstrateNode? Node, Diagnostic? Diagnostic)>
            FilterByRegisteredAttributes<TSymbol>(this IncrementalValuesProvider<TSymbol> pipe)
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
                        Diagnostic? diag = null;

                        var node = rule.TryCreate(
                            symbol,
                            attr,
                            d => diag = d    // <- capture diagnostic, don’t report yet
                        );

                        results.Add((node, diag));
                    }
                }

                return results;
            });
        }

        internal static IncrementalValuesProvider<Diagnostic>
            FlattenDiagnostics(
                IncrementalValueProvider<ImmutableArray<Diagnostic?>> left,
                IncrementalValueProvider<ImmutableArray<Diagnostic?>> right)
        {
            return left.Combine(right)
                .SelectMany(static (pair, _) =>
                    pair.Left.Concat(pair.Right)
                             .Where(d => d is not null)!
                             .Cast<Diagnostic>());
        }
    }
}
