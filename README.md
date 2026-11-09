# FlowchartBuilder

A modular WinForms diagram editor built on a small custom .NET framework.

I wrote it in 2008-2011 to explore plugin architecture and declarative metadata, back when MEF,
modern DI containers and `System.Text.Json` were not part of .NET yet. In 2026 the code was moved
to .NET 10 and C# 14 and cleaned up (see `MIGRATION.md`).

![Screenshot](screenshot.png)

## Solution layout

`FlowchartBuilder.slnx` contains 11 projects.

### Framework

| Project | Role |
|---|---|
| `Makarov.Framework.Core` | Plugin loader, XML translations validated against an XML Schema, settings storages (registry, XML file, memory), image cache, window manager, serializer registry, critical error dialog |
| `Makarov.Framework.Serialization` | Culture-invariant string serializers for numbers, `bool`, `string`, `Color`, `Font` and `Version` |
| `Makarov.Framework.Graphics` | Geometric primitives that build `GraphicsPath` outlines: rhombus, star, trapezoid, rounded rectangle and others |
| `Makarov.Framework.Components` | `TabsPanel`, the strip of document tabs |
| `Makarov.Framework.Instance` | Single instance: a named mutex plus a named pipe that passes the command line to the running copy |

### FlowchartBuilder

| Project | Role |
|---|---|
| `Makarov.FlowchartBuilder` | The application: documents, 28 sheet formats (ISO, ANSI, other paper sizes and an infinite sheet), glyph base classes, 53 menu commands, windows |
| `Makarov.FlowchartBuilder.API` | Attributes that describe glyphs and sheets: `[GlyphName]`, `[GlyphFamily]`, `[GlyphDefaultWidth]`, `[VisibleGlyph]`, `[CommandGlyph]`, `[ActiveProperty]`, `[SheetName]`, `[SheetWidth]` and others |
| `Makarov.FlowchartBuilder.Glyphs.Flowchart` | Begin, End, If, In/Out, Operation, Subroutine |
| `Makarov.FlowchartBuilder.Glyphs.Shapes` | 14 geometric shapes |
| `Makarov.FlowchartBuilder.Glyphs.Containers` | Group, Panel, Stack, Table |
| `Makarov.FlowchartBuilder.Glyphs.Simple` | Arrow, Broken Arrow and Link (placeholders for now), Image and Text (hidden, not implemented) |

Glyph libraries are plugins. On startup the application loads every assembly from the `Plugins`
folder next to the executable and puts every non-abstract glyph type into the palette, so a new DLL
in that folder adds new glyphs without rebuilding the application.

## How it works

**Metadata in attributes.** A glyph describes itself with attributes and only draws its outline:

```csharp
[GlyphName("If")]
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(20)]
public sealed class IfGlyph : AbstractFlowchartGlyph
{
    protected override Primitive CreatePrimitive(int x, int y, int width, int height) => new Rhombus(x, y, width, height);
}
```

Properties marked with `[ActiveProperty]` are saved to the document, copied on paste and shown in
the Properties window; everything else stays out of the property grid.

**Documents.** A document is an XML file (`*.mfb`) with the sheet, the glyphs, the connectors and
the glyph groups. Glyphs are stored by class name, so documents keep loading as long as the class
names stay the same. Properties the current version does not know are skipped with a warning.

**Connectors.** Flowchart blocks show four link points. Drag from a link point to another block to
connect them; connectors follow the blocks when they move.

**Menus.** Menus and the toolbar are defined in `Data\MainMenu.xml` and `Data\SecondaryMenu.xml`.
Each item names a command class; commands are created on first use and shared by all items bound
to them.

**Settings.** Settings live in the registry under `HKCU\Software\MakarovFlowchartBuilder`. If the
registry is not available they go to `%AppData%\FlowchartBuilderSettings.xml`, and as a last resort
they are kept in memory. They can be exported to and imported from a `*.fbsettings` file.

**Translations.** English, Russian and Ukrainian dictionaries in `Translations\*.xml`. A new
language is used after a restart.

**Single instance.** Starting the application again (for example by opening a document from
Explorer) passes the file names to the running copy and brings it to the front.

## Building

You need Windows and Visual Studio 2026 or the .NET 10 SDK.

```
dotnet build FlowchartBuilder.slnx -c Release
```

The application is built into `Build\<Configuration>` and the glyph plugins into
`Build\<Configuration>\Plugins`. Menus, icons, images and translations are copied next to the
executable, so the build output runs as is.

`MakeFlowchartBuilder\Make.cmd` builds the Release configuration and, if Inno Setup is installed,
packs it into an installer in `MakeFlowchartBuilder\Output`. The installed application needs the
.NET 10 Desktop Runtime.

To move to a newer .NET, change `TargetFramework` in `Directory.Build.props`.

## Status

Working: documents (new, open, save, export to an image), all glyph families, connectors, grouping,
locking, alignment, cut, copy and paste, zoom, rulers, grid, units of measurement, languages,
settings import and export.

Not implemented yet, so the menu items stay disabled: Undo, Redo, Close, Close All, Save All,
Page Setup and Print. Open items are listed in `todo.txt`.

## License

Copyright © 2008-2026 Mykhailo Makarov. All rights reserved.

## Support

If you found this project interesting or useful, you can support my work:

[![GitHub Sponsors](https://img.shields.io/github/sponsors/makarov-mm?style=flat&logo=github)](https://github.com/sponsors/makarov-mm)
