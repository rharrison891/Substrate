# IconPack

## A complete set of Icons from FluentIcons.

All of the icons are available through the source generator and can be easily included in your project with a simple `[IconPack]` attribute.

This will give you 

- `Icons` enum with ≈ 1,500 icons
- `IconMap` dictionary to convert enm to glyph
- `AsGlyph()` extension method
- `Get()` accessor

To activate the icon pack just simply create a blank class with the `[IconPack]` attribute.

```csharp
[IconPack]
public partial class AppIcons { }
```

That's it...The generator will do everything for you, and Icons are available globally in your app.

## Using an icon

```xaml
<TextBlock FontFamily="Segoe MDL2 Assets"
           Text="{x:Static local:Icons.Save.AsGlyph}" />
```

or in C#:

```csharp
var glyph = Icons.Save.AsGlyph();
```