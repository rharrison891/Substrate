# Theme

## Simple, code first theming

You can easily theme your entire app from a simple dictionTheme your entire app from a simple dictionary - no XAML resoource dictionaries to manage.

Everything is generated and stays in code.

## Getting started

Create a public partial class, and add the `[Theme]` attribute and define your color dictionary.

```csharp
[Theme]
public partial class AppTheme
{
    private static readonly Dictionary<string, string> _baseColors = new()
    {
        { "Background", "#101010" },
        { "Accent", "#505050" },
    };
}
```

This generates

- `ThemeColor` enum
- `GetBrush()` 
- `GetColor()` 
- `SetColor()` 
- `ThemeBrush` XAML markup extension

## Defaults

You don't have to define everything. A built-in palette is generated automatically - override only what you need:

```csharp
public static readonly List<(string Key, int A, int R, int G, int B)> 
    BasePalette = new()
    {
        ("Background", 255, 30, 30, 30),
        ("Foreground", 255, 220, 220, 220),
        ("Primary",    255, 0, 120, 215),
        ("Secondary",  255, 45, 45, 48),
        ("Accent",     255, 0, 153, 204),
        ("Border",     255, 90, 90, 90),
        ("Error",      255, 232, 17, 35),
        ("Warning",    255, 255, 185, 0),
        ("Success",    255, 16, 124, 16)
    };
```

## Basic usage

``` xaml
<Border Background="{local:ThemeBrush Base=Background}"
        BorderBrush="{local:ThemeBrush Base=Accent}" />
```

## Advanced usage

You can adjust alpha and brightness directly in XAML:

``` xaml
<Border Background="{local:ThemeBrush Base=Background, Alpha=0.5}"
        BorderBrush="{local:ThemeBrush Base=Accent, Brightness=-50}" />
```

- Alpha - 0 (transparent) -> 1 (opaque)
- Brightness - negative = darker, positive = lighter (clamped 0-255)

## Runtime theme manipulation

Change colors at runtime - everything updates automatically:

```csharp
AppTheme.SetColor(ThemeColor.Background, Colors.Blue);
```
No resource dictionaries. No XAML updates. 

Just one line.