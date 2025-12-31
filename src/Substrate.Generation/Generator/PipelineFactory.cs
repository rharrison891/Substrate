using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Substrate.Generation.Core.Attributes;
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
            //  FEATURE VALIDATION
            // =====================
            var (goodThemes, themeDiagnostics) =
                Validator.ValidatePipeline<ThemeNode>(
                    typesRaw,
                    DiagnosticDescriptors.MultipleThemeClassesDetected);

            var (goodIcons, iconDiagnostics) =
                Validator.ValidatePipeline<IconPackNode>(
                    typesRaw,
                    DiagnosticDescriptors.MultipleIconPackClassesDetected);

            // 👉 Add new features here:
            //
            // var (goodFoo, fooDiagnostics) =
            //     Validator.ValidatePipeline<FooNode>(typesRaw, DiagnosticDescriptors.MultipleFooDetected);

            var documents = DocumentBuilder.BuildDocuments(
                fieldsPipe,
                methodsPipe,
                typesPipe,
                goodThemes.ToSubstrateNodes(),
                goodIcons.ToSubstrateNodes()
            );

            context.RegisterSourceOutput(
                documents,
                static (spc, doc) =>
                {
                    spc.AddSource(doc.HintName, doc.Build());
                });

            context.RegisterSourceOutput(
                DiagnosticCollector.CollectDiagnostics(
                    fieldsRaw.Select((r, _) => r.Diagnostic),
                    methodsRaw.Select((r, _) => r.Diagnostic),
                    typesRaw.Select((r, _) => r.Diagnostic),
                    themeDiagnostics,
                    iconDiagnostics),
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
    }
}
