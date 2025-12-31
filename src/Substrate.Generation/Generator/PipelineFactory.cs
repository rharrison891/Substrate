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
            // 🔗 Attribute → Rule wiring
            AttributeRegistry.Register(new NotifyAttributeRule());
            AttributeRegistry.Register(new DependencyPropertyAttributeRule());
            AttributeRegistry.Register(new ThemeAttributeRule());
            AttributeRegistry.Register(new IconPackRule());

            // 🔹 Per-syntax pipelines
            var fieldsRaw =
                context.CreateDeclarationPipeline<VariableDeclaratorSyntax, IFieldSymbol>()
                       .FilterByRegisteredAttributes();

            var methodsRaw =
                context.CreateDeclarationPipeline<MethodDeclarationSyntax, IMethodSymbol>()
                       .FilterByRegisteredAttributes();

            var typesRaw =
                context.CreateDeclarationPipeline<ClassDeclarationSyntax, INamedTypeSymbol>()
                       .FilterByRegisteredAttributes();

            // 🔹 Node-only streams
            var fieldsPipe = fieldsRaw.Where(t => t.Node is not null).Select((t, _) => t.Node!);
            var methodsPipe = methodsRaw.Where(t => t.Node is not null).Select((t, _) => t.Node!);
            var typesPipe = typesRaw.Where(t => t.Node is not null).Select((t, _) => t.Node!);

            // =====================
            //  THEME PIPELINE
            // =====================

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
                    var groups = themes.GroupBy(t => new { t.Namespace, t.TypeName });

                    foreach (var g in groups)
                    {
                        // allow ONE theme per logical type
                        var first = g.First();
                        output.Add((first, null));

                        // extra theme classes with same name → diagnostic
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

            // =====================
            //  ICON PACK PIPELINE
            // =====================

            var iconNodes =
                typesRaw
                    .Where(t => t.Node is IconPackNode)
                    .Select((t, _) => (IconPackNode)t.Node!)
                    .Collect();

            var validatedIcons =
                iconNodes.SelectMany((icons, _) =>
                {
                    var output = new List<(IconPackNode? node, Diagnostic? diag)>();

                    var groups = icons.GroupBy(i => new { i.Namespace, i.TypeName });

                    foreach (var g in groups)
                    {
                        var first = g.First();
                        output.Add((first, null));

                        foreach (var extra in g.Skip(1))
                        {
                            output.Add((
                                null,
                                Diagnostic.Create(
                                    DiagnosticDescriptors.MultipleIconPackClassesDetected,
                                    extra.Location,
                                    extra.TypeName)));
                        }
                    }

                    return output;
                });

            var goodIconNodes =
                validatedIcons
                    .Where(t => t.node is not null)
                    .Select((t, _) => t.node!);

            var iconDiagnostics =
                validatedIcons
                    .Where(t => t.diag is not null)
                    .Select((t, _) => t.diag!);

            // =====================
            //  DOCUMENTS
            // =====================

            // fields + methods + type-level features + themes + icons
            var collected = fieldsPipe
                .Collect()
                .Combine(methodsPipe.Collect())
                .Combine(typesPipe.Collect())
                .Combine(goodThemeNodes.Collect())
                .Combine(goodIconNodes.Collect());

            var documents = collected.SelectMany(static (five, _) =>
            {
                // five = ((((fields, methods), types), themes), icons)
                var quad = five.Left;   // (((fields, methods), types), themes)
                var icons = five.Right;  // ImmutableArray<IconPackNode>

                var ((pair, types), themes) = quad;
                // pair = (fields, methods)

                var all = pair.Left      // fields
                    .AddRange(pair.Right) // methods
                    .AddRange(types)      // type-level nodes (Theme/IconPack/etc)
                    .AddRange(themes)     // validated themes
                    .AddRange(icons);     // validated icon packs

                var groups = all.GroupBy(n => new TypeKey(n.Namespace, n.TypeName));

                return groups
                    .SelectMany(g =>
                        DocumentFactory.CreateAll(
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

            // =====================
            //  DIAGNOSTICS
            // =====================

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
                    themeDiagnostics.Collect()!,
                    iconDiagnostics.Collect()!),
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
                            d => diag = d);

                        // 🔹 IMPORTANT — add one entry PER attribute
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
                IncrementalValueProvider<ImmutableArray<Diagnostic?>> d,
                IncrementalValueProvider<ImmutableArray<Diagnostic?>> e)
        {
            return a.Combine(b)
                    .Combine(c)
                    .Combine(d)
                    .Combine(e)
                    .SelectMany(static (five, _) =>
                        five.Left.Left.Left.Left
                            .Concat(five.Left.Left.Left.Right)
                            .Concat(five.Left.Left.Right)
                            .Concat(five.Left.Right)
                            .Concat(five.Right)
                            .Where(d => d is not null)!
                            .Cast<Diagnostic>());
        }
    }
}
