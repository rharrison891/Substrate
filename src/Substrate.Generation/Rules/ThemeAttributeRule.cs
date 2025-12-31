using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Substrate.Generation.Core.Nodes;

namespace Substrate.Generation.Core.Rules
{
    internal sealed class ThemeAttributeRule : IAttributeRule
    {
        public string AttributeName => "Theme";

        public SubstrateNode? TryCreate(
            ISymbol symbol,
            AttributeData attribute,
            ReportDiagnostic report)
        {
            if (symbol is not INamedTypeSymbol type)
                return null;

            var ns = type.ContainingNamespace.ToDisplayString();

            var colors = ExtractColors(type, report);
            bool usesFallback = colors.Count == 0;

            //
            // 👇 build the usings the ThemeDocument will need
            //
            var usings = new HashSet<string>
            {
                ns,
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "System.Windows.Markup",
                "System.Windows.Media"
            };

            return new ThemeNode(
                Namespace: ns,
                TypeName: type.Name,
                Colors: colors,
                UsesFallbackPalette: usesFallback,
                Location: symbol.Locations.FirstOrDefault(),
                Usings: usings
            );
        }

        private static IFieldSymbol? FindThemeField(INamedTypeSymbol type)
        {
            foreach (var member in type.GetMembers())
            {
                if (member is IFieldSymbol f && f.Name == "_baseColors")
                    return f;
            }

            // fallback: walk partials manually
            foreach (var decl in type.DeclaringSyntaxReferences)
            {
                if (decl.GetSyntax() is not ClassDeclarationSyntax syntax)
                    continue;

                foreach (var field in syntax.Members.OfType<FieldDeclarationSyntax>())
                {
                    foreach (var variable in field.Declaration.Variables)
                    {
                        if (variable.Identifier.Text == "_baseColors")
                        {
                            return (IFieldSymbol?)type
                                .GetMembers()
                                .FirstOrDefault(m =>
                                    m is IFieldSymbol fs &&
                                    fs.Name == "_baseColors");
                        }
                    }
                }
            }

            return null;
        }

        private static List<(string Key, int A, int R, int G, int B)> ExtractColors(
            INamedTypeSymbol type,
            ReportDiagnostic report)
        {
            var result = new List<(string Key, int A, int R, int G, int B)>(ThemeDefaults.BasePalette);

            var field = FindThemeField(type);
            if (field is null)
                return result;

            var declarator = field.DeclaringSyntaxReferences
                .Select(r => r.GetSyntax())
                .OfType<VariableDeclaratorSyntax>()
                .FirstOrDefault();

            InitializerExpressionSyntax? initializer = null;

            if (declarator?.Initializer?.Value is ObjectCreationExpressionSyntax oce)
                initializer = oce.Initializer;
            else if (declarator?.Initializer?.Value is ImplicitObjectCreationExpressionSyntax ioce)
                initializer = ioce.Initializer;

            if (initializer is null)
                return result;

            foreach (var expr in initializer.Expressions)
            {
                if (expr is not InitializerExpressionSyntax pair ||
                    pair.Expressions.Count != 2)
                    continue;

                var keyExpr = pair.Expressions[0];
                var valueExpr = pair.Expressions[1];

                var keyText = keyExpr.ToString().Trim().Trim('"');
                var valueText = valueExpr.ToString().Trim();

                var (ok, key, a, r, g, b) = ThemeColorParser.Parse(keyText, valueText);

                if (!ok)
                {
                    report(Diagnostic.Create(
                        DiagnosticDescriptors.InvalidColorEntry,
                        valueExpr.GetLocation(),
                        keyText));

                    continue;
                }

                var i = result.FindIndex(c => c.Key == key);
                if (i >= 0)
                    result[i] = (key, a, r, g, b);
                else
                    result.Add((key, a, r, g, b));
            }

            return result;
        }
    }
}
