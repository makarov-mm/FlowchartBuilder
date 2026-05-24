# FlowchartBuilder

A modular WinForms diagram editor with a custom .NET framework underneath.

Developed 2008–2011 as a deep exploration of plugin architecture, declarative
metadata, and pluggable rendering — well before MEF, modern DI containers,
or `System.Text.Json` were available in the .NET ecosystem.

![Screenshot](screenshot.png)

## Context

This was a personal multi-year project written entirely in .NET Framework /
WinForms / GDI+, with optional OpenGL rendering. Most of the patterns it
explores — attribute-based plugin discovery, pluggable graphics backends,
declarative metadata for extensions — were not yet standard practice in
.NET tooling at the time. The codebase reflects the conventions of its era
(`System.Runtime.Remoting` for IPC, manual reflection-based loaders, custom
serialization) and is preserved as-is.

## Solution Layout

The repository contains **21 projects** split into two layers:

### Makarov Framework (10 projects) — a small custom application framework

| Project | Role |
|---|---|
| `Makarov.Framework.Core` | Reflection-based assembly loader, i18n translator with XML Schema, settings (Registry + XML fallback), managers, caching |
| `Makarov.Framework.Serialization` | 16 typed serializers (`Bool`, `Int`, `Float`, `Color`, `Font`, `Version`, …) over an extensible `AbstractSerializer` base |
| `Makarov.Framework.Graphics` | Abstract rendering layer — `IFog`, `ILight`, `ILighting`, `ITexture`, `VertexBuffer`, `CompiledVertexBuffer`, primitives |
| `Makarov.Framework.Graphics.OpenGL` | OpenGL backend implementation |
| `Makarov.Framework.Math` | Math utilities |
| `Makarov.Framework.Components` | Custom UI components |
| `Makarov.Framework.Collections` | Collection extensions |
| `Makarov.Framework.Instance` | Single-instance application pattern via `System.Runtime.Remoting` IPC channels |
| `Makarov.Framework.Launcher` | Generic launcher |
| `Makarov.Framework.VersionUpdater` | Auto-updater (separate process to bypass file-locks on the main executable) |

### FlowchartBuilder (11 projects on top of the framework)

| Project | Role |
|---|---|
| `Makarov.FlowchartBuilder.API` | Declarative attributes for extensions: `[GlyphName]`, `[GlyphDefaultWidth]`, `[GlyphFamilyAttribute]`, `[GlyphGroupOrderAttribute]`, `[CommandGlyphAttribute]`, `[VisibleGlyphAttribute]`, `[SheetName]`, `[SheetWidth]`, … |
| `Makarov.FlowchartBuilder` | Application core with `Core` split across partial classes by concern; **60 command classes** in a clean Command pattern |
| `Makarov.FlowchartBuilder.Box` | Glyph palette helper |
| `Makarov.FlowchartBuilder.Glyphs.Flowchart` | If, Operation, Begin, End, InOut, Subroutine |
| `Makarov.FlowchartBuilder.Glyphs.Shapes` | 16 geometric shapes (Rhombus, Ellipse, Trapezoid, Star, Hourglass, Pie, Triangle, Rounded Rectangle, …) |
| `Makarov.FlowchartBuilder.Glyphs.Simple` | Arrow, BrokenArrow, Link, Image, Text |
| `Makarov.FlowchartBuilder.Glyphs.Containers` | Group, Panel, Stack, Table |
| `Makarov.FlowchartBuilder.Sheets.Iso` | ISO paper formats |
| `Makarov.FlowchartBuilder.Sheets.Ansi` | ANSI paper formats |
| `Makarov.FlowchartBuilder.Sheets.Other` | Other paper formats |
| `MakeFlowchartBuilder` | Build pipeline + InnoSetup installer script + Mono compatibility script |

Glyphs and sheets are independent plugin assemblies — drop a new DLL into the
plugins folder and it becomes a new shape or paper format. No host
recompilation required.

## Architecture Highlights

**Attribute-based plugin discovery.** Each extension declares its metadata
through attributes:

```csharp
[GlyphName("If")]
[GlyphDefaultWidth(30)]
[GlyphDefaultHeight(20)]
public class IfGlyph : AbstractFlowchartGlyph { ... }
```

The host enumerates plugin assemblies, reflects over their types, reads the
attributes, and builds the palette dynamically. The same mechanism applies
to sheet formats. Conceptually similar to MEF — predates the team's adoption
of it.

**Pluggable graphics backends.** The `Makarov.Framework.Graphics` project
defines a backend-agnostic rendering interface; `Graphics.OpenGL` provides
one concrete implementation, with GDI+ available through the WinForms host.
The architectural split anticipates patterns later standardized in graphics
abstraction layers.

**Custom serialization framework.** 16 typed serializers over an extensible
`AbstractSerializer`, written years before `System.Text.Json` (2019) became
part of the runtime. Registered through a `SerializerManager`.

**Command pattern at full scale.** 60 command classes — Undo/Redo, Cut/Copy/
Paste, alignment (6 directions × {glyph, sheet}), Group/Ungroup, Fix/Unfix,
Zoom, PageSetup, Print, ExportImage, language switching, units of
measurement — all sharing one `Command` base, all decoupled from UI.

**Single-instance pattern via .NET Remoting.** `InstanceHelper` registers
an IPC channel and exposes the running instance through `RemotingConfiguration.
RegisterWellKnownServiceType` — the canonical 2011 approach.

**Settings with graceful fallback.** Primary storage is the Windows Registry;
if access is denied, the layer falls back to an XML file transparently
(`RegistryStorage`, `XmlStorage`, `FakeStorage`).

**Internationalization.** Custom `Translator` with XML-based dictionaries
validated against XML Schema, runtime language switching, typed exceptions.

**Mono compatibility target.** `runOnMono.cmd` in the build pipeline — the
project was already conscious of cross-platform .NET five years before
.NET Core 1.0.

**Auto-updater.** Separate `VersionUpdater` executable invoked to bypass
file-locks on the main process during updates.

**Code conventions.** XML doc comments on public APIs, copyright headers with
authorship and date, partial classes for separation of concerns, named
regions, typed exceptions per domain.

## Tech

- C# 3 / .NET Framework
- WinForms + GDI+
- OpenGL (optional rendering backend)
- `System.Runtime.Remoting` (IPC)
- `System.Reflection` (plugin discovery)
- InnoSetup (installer)
- Mono (cross-platform target)

## Building

Open `FlowchartBuilder.sln` in Visual Studio 2010+ and build. The installer
is produced through `MakeFlowchartBuilder/Make.cmd` and the InnoSetup script
`FlowchartBuilder.iss`.

## Project Status

Feature-complete for its original scope. Preserved as a snapshot of the
2008–2011 era of .NET application design.

## License

Copyright © 2008–2011 Mykhailo Makarov. All rights reserved.
