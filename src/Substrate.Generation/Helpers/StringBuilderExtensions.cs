using System.Text;

internal static class StringBuilderExtensions
{
    private const int SpacesPerIndent = 4;
    private const bool BlankLineAfterRegions = true;
    public static void AppendIndented(this StringBuilder sb, int tabs, string line)
        => sb.AppendLine(new string(' ', tabs * SpacesPerIndent) + line);
    public static void OpenBrace(this StringBuilder sb, int tabs)
        => sb.AppendIndented(tabs, "{");
    public static void CloseBrace(this StringBuilder sb, int tabs)
        => sb.AppendIndented(tabs, "}");
    public static void BlankLine(this StringBuilder sb)
        => sb.AppendLine();
    public static void Nullable(this StringBuilder sb, bool enable = true)
    {
        var ns = enable ? "enable" : "disable";
        sb.AppendLine($"#nullable {ns}");
    }
    public static void StartRegion(this StringBuilder sb, string region)
    {
        sb.AppendLine($"#region {region}");
        if (BlankLineAfterRegions) sb.BlankLine();
    }
    public static void EndRegion(this StringBuilder sb)
    {
        sb.AppendLine("#endregion");
        if (BlankLineAfterRegions) sb.BlankLine();
    }

}