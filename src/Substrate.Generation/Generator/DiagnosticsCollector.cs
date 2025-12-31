using Microsoft.CodeAnalysis;

namespace Substrate.Generation.Core.Generator
{
    internal static class DiagnosticCollector
    {
        public static IncrementalValuesProvider<Diagnostic>
            CollectDiagnostics(params IncrementalValuesProvider<Diagnostic?>[] sources)
        {
            var combined = sources[0];

            for (int i = 1; i < sources.Length; i++)
                combined = combined.Concat(sources[i]);

            return combined
                .Where(d => d is not null)!
                .Select((d, _) => d!);
        }
    }
}
