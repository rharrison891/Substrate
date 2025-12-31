# IconPack

## A complete set of Fluent Icons

All of the icons are available through the source generator and can be included
with a simple **`[IconPack]`** attribute.

This generates:

- `Icons` enum with ≈ 1,500 icons  
- `IconMap` dictionary to convert enum → glyph  
- `AsGlyph()` extension method  
- `Get()` accessor

To activate, create a blank partial class:

```csharp
[IconPack]
public partial class AppIcons { }
```

That's it - The generator will do the rest, and Icons are now available globally.

## Using an icon

```xaml
<TextBlock FontFamily="Segoe MDL2 Assets"
           Text="{x:Static local:Icons.Save.AsGlyph}" />
```

or in C#:

```csharp
var glyph = Icons.Save.AsGlyph();
```