using Substrate.Generation.Attributes;

namespace Substrate.Generation.Test
{
    [Theme]
    public partial class AppTheme
    {
        private static readonly Dictionary<string, string> _baseColors = new()
        {
            { "Background", "#101010" },
            { "Accent", "#505050" },
            { "Blue", "#0000ff" }
        };
    }
}
