# Modernization notes

FlowchartBuilder was written in 2008-2011 for .NET Framework and C# 3. In 2026 it was moved to
.NET 11, cleaned up and given a Direct2D renderer. This file lists what changed and why.

## Platform and project system

- All projects are SDK-style. `Makarov.FlowchartBuilder.API` and `Makarov.Framework.Instance`
  target `net11.0`; the rest target `net11.0-windows` with WinForms. The code uses C# 14 features
  and builds with the latest language version.
- Common settings live in `Directory.Build.props`: target framework, `LangVersion latest`, nullable
  reference types, implicit usings, version, copyright and the output layout. Moving to a newer
  .NET means changing `TargetFramework` there.
- The solution file is `FlowchartBuilder.slnx` (the XML solution format of Visual Studio 2026).
- Hand-written `Properties/AssemblyInfo.cs` files are gone. They duplicated the attributes the SDK
  generates, so the solution did not build at all.
- Everything builds into `Build\<Configuration>`, glyph plugins into `Build\<Configuration>\Plugins`.
  Plugin projects reference the application with `Private=false`, so the `Plugins` folder holds
  only plugin assemblies and the loader never sees a second copy of the host.
- `.editorconfig` describes the code style: file-scoped namespaces, naming, CRLF line endings.

## Replaced APIs and patterns

- .NET Remoting (single instance) was replaced with a named mutex plus a named pipe
  (`Makarov.Framework.Instance.SingleInstance`). The pipe is restricted to the current user, the
  listener stops with the application, messages reach the main window through `BeginInvoke`, and
  a bad message no longer kills the listener.
- The `Reflection.Emit` proxies that hid properties from the property grid were replaced with
  `PropertyGrid.BrowsableAttributes` and the `[ActiveProperty]` attribute.
- Plugins are loaded with `AssemblyLoadContext.Default.LoadFromAssemblyPath`.
- Painting: GDI+ is gone from the drawing code (see the next section). The sheet is drawn with a
  translate transform and a clip, and the mouse handlers use the same origin, so hit-testing
  matches the picture.
- Reflection metadata of glyphs (`GlyphMetadata`), unit factors, fonts, pens and command lookups
  are cached instead of being recomputed on every paint.
- Custom infrastructure that the BCL now covers (singleton base class, ranges, random generator,
  string dictionaries, caches, thread-safe console) was replaced with standard types.
- Numeric serializers are one generic class over `IParsable<T>` and always use the invariant
  culture. Decimals no longer lose digits, and fonts keep their style.
- C# 14: file-scoped namespaces, primary constructors, collection expressions, pattern matching,
  records, `extension` blocks, `ArgumentOutOfRangeException.ThrowIf*`, nullable annotations.

## Direct2D renderer

GDI+ drew everything on the CPU and became slow on large sheets, with antialiasing and at high zoom.
All drawing now goes through Direct2D and DirectWrite.

- `Makarov.Framework.Graphics` calls Direct2D, DirectWrite and WIC through their COM vtables
  (`Interop` folder, function pointers, no NuGet packages). Slot numbers and parameter counts were
  checked against the Windows SDK headers.
- `Canvas` is the drawing API used by glyphs and sheets: rectangles, ellipses, lines, geometries,
  gradients, text, transforms and clips, with brushes, stroke styles and text formats cached.
- `CanvasControl` hosts an `ID2D1HwndRenderTarget` and recreates it when the graphics device is
  lost. The main window draws in `SheetView`, a `CanvasControl` below the document tabs.
- `OffscreenRenderer` renders into a WIC bitmap for palette images, sheet previews, exported
  pictures and printed pages.
- Primitives build `Geometry` objects instead of `GraphicsPath`; arcs keep the GDI+ angle
  conventions, so all shapes look the same as before.
- Caption text now scales with the zoom, and the lock marker is drawn as a vector padlock.
- WinForms controls (menus, tabs, dialogs, the property grid) are still drawn by Windows.

## New features

- Undo and Redo for every change: mouse edits, menu commands and property edits. Each step is a
  snapshot of the document XML (`DocumentHistory`), so undo is exact for any operation.
- Close, Close All, Save As and Save All; unsaved changes are marked with an asterisk and confirmed
  when a document or the application is closed. Tabs have a close button (middle click works too).
- Page Setup and Print: one page, at real size when the sheet fits in the margins.
- Connectors: curved, straight or orthogonal routes that leave and enter the blocks at right angles
  to their sides, with an optional arrowhead, color, width and caption. The Arrow, Broken Arrow and
  Link glyphs of the Simple plugin are now link tools for these three styles.
- Flowchart blocks show their caption, centered. Flowchart blocks and containers can no longer be
  rotated, and their Angle property is hidden.
- The mouse wheel scrolls the sheet (Shift: horizontally), Ctrl+wheel zooms.
- The locked state of glyphs is saved in documents.

## Removed

- Projects: `Framework.Math`, `Framework.Collections`, `Framework.Launcher`,
  `Framework.VersionUpdater` (it rewrote version numbers in `AssemblyInfo.cs`),
  `FlowchartBuilder.Box` (its only helper became `GlyphMetadata`), and `Sheets.Ansi`, `Sheets.Iso`,
  `Sheets.Other`, whose classes were compiled into the application anyway and now live in
  `Makarov.FlowchartBuilder\Sheets`.
- Unused code: the empty rendering interfaces (fog, lights, textures, vertex buffers), GDI P/Invoke
  helpers and unsafe bitmap code, the old unfinished undo snapshots, most custom exception classes,
  the debug "force garbage collection" commands, the Help Topics and Load Image placeholders, and
  the custom sheet whose drawing threw `NotImplementedException`.
- Files: stale binaries, `app.config` and manifest files, the obfuscator configuration, the Mono
  launch script, the prebuilt installer and the unused images of the `Graphics` folder.

## Bugs fixed

- Starting a second copy without arguments crashed the running one.
- Opening a second document showed the first one; document tabs were not drawn.
- Menu commands threw when no document or nothing was selected, which closed the application
  through the error dialog. They are now disabled or do nothing in that state.
- Paste worked only once, and pasted glyphs lost their properties because `Clone` copied nothing.
- The same glyph could not be placed twice in a row from the palette.
- Infinite sheets crashed on paint.
- Center alignment divided by zero when only connectors were selected.
- Clicking a locked connector, or selecting a group with a locked member, threw.
- Groups were not saved, and deleted glyphs stayed in their groups.
- The z-order of glyphs flipped on every load; the "document saved by another version" message
  appeared once per glyph.
- Part of a sheet could not be scrolled into view, and the vertical scroll bar started at the
  bottom.
- The lock marker was drawn in the wrong place and leaked a bitmap on every frame; captions ignored
  the glyph font.
- Load Settings threw `NotImplementedException`; the settings dialog had options that did nothing.
- Two glyphs were both called "Arrow", and the ellipse was spelled "Ellypse".
- The settings window could be reopened from a disposed instance; the toolbar Open and Save buttons
  and the toggle check marks ignored the actual state.

## Compatibility

- The document format is unchanged. Glyph class names are kept (including `EllypseGlyph`), so old
  documents open as before. Groups are written to a new `fb:Groups` element and locked glyphs get a
  `fixed` attribute; older versions ignore both.
- Arrow, Broken Arrow and Link used to be empty placeholder blocks. Old documents that contain them
  open with a warning, and those placeholders are dropped.
- Registry keys and setting names are unchanged.

## Not done yet

Print preview, printing large sheets over several pages, and the Image and Text glyphs. See
`todo.txt` for the rest.
