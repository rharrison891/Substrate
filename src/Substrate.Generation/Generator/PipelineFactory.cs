using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Substrate.Generation.Core.Attributes;
using Substrate.Generation.Core.Documents;
using Substrate.Generation.Core.Nodes;
using Substrate.Generation.Core.Rules;

namespace Substrate.Generation.Core.Generator
{
    internal static class PipelineFactory
    {

        internal static void CreatePipelines(IncrementalGeneratorInitializationContext context)
        {
            AttributeRegistry.Register(new NotifyAttributeRule());
            AttributeRegistry.Register(new DependencyPropertyAttributeRule());
            AttributeRegistry.Register(new ThemeAttributeRule());

            var fieldsRaw =
                context.CreateDeclarationPipeline<VariableDeclaratorSyntax, IFieldSymbol>()
                       .FilterByRegisteredAttributes();

            var methodsRaw =
                context.CreateDeclarationPipeline<MethodDeclarationSyntax, IMethodSymbol>()
                       .FilterByRegisteredAttributes();

            var typesRaw =
                context.CreateDeclarationPipeline<ClassDeclarationSyntax, INamedTypeSymbol>()
                       .FilterByRegisteredAttributes();

            // nodes only
            var fieldsPipe = fieldsRaw.Where(t => t.Node is not null).Select((t, _) => t.Node!);
            var methodsPipe = methodsRaw.Where(t => t.Node is not null).Select((t, _) => t.Node!);
            var typesPipe = typesRaw.Where(t => t.Node is not null).Select((t, _) => t.Node!);

            var themeNodes =
                typesRaw
                    .Where(t => t.Node is ThemeNode)
                    .Select((t, _) => (ThemeNode)t.Node!)
                    .Collect();

            var validatedThemes =
                themeNodes.SelectMany((themes, _) =>
                {
                    var output = new List<(ThemeNode? node, Diagnostic? diag)>();

                    // Group by logical type
                    var groups = themes
                        .GroupBy(t => new { t.Namespace, t.TypeName });

                    foreach (var g in groups)
                    {
                        // allow ONE theme per logical type
                        var first = g.First();
                        output.Add((first, null));

                        // any other theme class with the SAME name → diagnostic
                        foreach (var extra in g.Skip(1))
                        {
                            output.Add((
                                null,
                                Diagnostic.Create(
                                    DiagnosticDescriptors.MultipleThemeClassesDetected,
                                    extra.Location,
                                    extra.TypeName)));
                        }
                    }

                    return output;
                });

            var goodThemeNodes =
                validatedThemes
                    .Where(t => t.node is not null)
                    .Select((t, _) => t.node!);

            var themeDiagnostics =
                validatedThemes
                    .Where(t => t.diag is not null)
                    .Select((t, _) => t.diag!);

            var collected = fieldsPipe
                .Collect()
                .Combine(methodsPipe.Collect())
                .Combine(typesPipe.Collect())
                .Combine(goodThemeNodes.Collect());

            var documents = collected.SelectMany(static (quad, _) =>
            {
                var ((pair, types), themes) = quad;

                var all = pair.Left
                    .AddRange(pair.Right)
                    .AddRange(types)
                    .AddRange(themes);

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

            var typeDiagnostics =
                typesRaw.Select((r, _) => r.Diagnostic).Collect();

            context.RegisterSourceOutput(
                FlattenDiagnostics(
                    fieldDiagnostics,
                    methodDiagnostics,
                    typeDiagnostics,
                    themeDiagnostics.Collect()!),
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
                IncrementalValueProvider<ImmutableArray<Diagnostic?>> a,
                IncrementalValueProvider<ImmutableArray<Diagnostic?>> b,
                IncrementalValueProvider<ImmutableArray<Diagnostic?>> c,
                IncrementalValueProvider<ImmutableArray<Diagnostic?>> d)
                {
                    return a.Combine(b)
                            .Combine(c)
                            .Combine(d)
                            .SelectMany(static (quad, _) =>
                                quad.Left.Left.Left
                                    .Concat(quad.Left.Left.Right)
                                    .Concat(quad.Left.Right)
                                    .Concat(quad.Right)
                                    .Where(d => d is not null)!
                                    .Cast<Diagnostic>());
                }
    }
}
