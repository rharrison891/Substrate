using Microsoft.CodeAnalysis;

namespace Substrate.Generation.Core.Generator
{
    [Generator(LanguageNames.CSharp)]
    public class Generator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
            => PipelineFactory.CreatePipelines(context);
    }
}
