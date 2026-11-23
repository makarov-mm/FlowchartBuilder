# FlowchartBuilder

A modular WinForms diagram editor with an infinite canvas. Start drawing immediately or press Ctrl+N for a new canvas. Drag with the middle mouse button to pan in any direction. Older paper-sized documents open on the same canvas.

I wrote it in 2008-2011 to explore plugin architecture and declarative metadata, back when MEF,
modern DI containers and `System.Text.Json` were not part of .NET yet. In 2026 the code was moved
to .NET 11, cleaned up and given a Direct2D renderer.

![Screenshot](screenshot.png)

Drag the title of the glyph palette or the Properties window toward any edge of the work area.
Release over the highlighted area to dock, or away from the edges to float the window.
Drag the divider to resize a docked panel. Escape cancels a drag. The layout is restored on startup.

## Solution layout

`FlowchartBuilder.slnx` contains the editor, its libraries and plugins, and the FlowchartBuilder.Tests project.

### Framework

| Project | Role |
|---|---|
| `Makarov.Framework.Core` | Plugin loader, XML translations validated against an XML Schema, settings storages (registry, XML file, memory), image cache, window manager, serializer registry, critical error dialog |
| `Makarov.Framework.Serialization` | Culture-invariant string serializers for numbers, `bool`, `string`, `Color`, `Font` and `Version` |
| `Makarov.Framework.Graphics` | Direct2D renderer: a `Canvas` drawing API, the `CanvasControl` window, offscreen rendering to bitmaps, geometries and the geometric primitives (rhombus, star, trapezoid, rounded rectangle and others) |
| `Makarov.Framework.Components` | `TabsPanel`, the strip of document tabs |
| `Makarov.Framework.Instance` | Single instance: a named mutex plus a named pipe that passes the command line to the running copy |

### FlowchartBuilder

| Project | Role |
|---|---|
| `Makarov.FlowchartBuilder` | The application: documents, an infinite canvas, glyph base classes, editor commands, windows |
| `Makarov.FlowchartBuilder.API` | Attributes that describe glyphs: `[GlyphName]`, `[GlyphFamily]`, `[GlyphDefaultWidth]`, `[VisibleGlyph]`, `[CommandGlyph]`, `[ActiveProperty]` and others |
| `Makarov.FlowchartBuilder.Glyphs.Flowchart` | Begin, End, If, In/Out, Operation, Subroutine |
| `Makarov.FlowchartBuilder.Glyphs.States` | States, initial/final markers and named transitions |
| `Makarov.FlowchartBuilder.Tools.StateMachine` | Interactive transition testing and Python state machine export |
| `Makarov.FlowchartBuilder.Glyphs.Uml` | UML classes, inheritance and directed associations |
| `Makarov.FlowchartBuilder.Tools.ClassPython` | Python class module generation and diagram diagnostics |
| `Makarov.FlowchartBuilder.Glyphs.Electrical` | Electrical components, wire junctions and orthogonal wires |
| `Makarov.FlowchartBuilder.Glyphs.Shapes` | 14 geometric shapes |
| `Makarov.FlowchartBuilder.Glyphs.Containers` | Group, Panel, Stack, Table |
| `Makarov.FlowchartBuilder.Glyphs.Simple` | Link tools: Arrow (curved), Broken Arrow (horizontal and vertical segments) and Link (straight, no arrowhead); embedded images and editable text |

Glyph libraries are plugins. On startup the application loads every assembly from the `Plugins`
folder next to the executable and puts every non-abstract glyph type into the palette, so a new DLL
in that folder adds new glyphs without rebuilding the application.

## How it works

**Rendering.** Everything on the sheet is drawn with Direct2D and DirectWrite through a small COM
interop layer in `Makarov.Framework.Graphics` (no third-party packages). The sheet view is a
hardware-accelerated `ID2D1HwndRenderTarget`; palette images, exported pictures and printed pages
are rendered into WIC bitmaps. Menus, tabs and dialogs are ordinary WinForms controls.

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
the Properties window; everything else stays out of the property grid. A glyph can also hide
properties that do not apply to it: blocks that cannot rotate (flowchart blocks, containers) hide
their angle.

**Documents.** A document is an XML file (`*.mfb`) with the sheet, the glyphs, the connectors and
the glyph groups. Glyphs are stored by class name, so documents keep loading as long as the class
names stay the same. Properties the current version does not know are skipped with a warning.

**Connectors.** Flowchart blocks show four link points. Drag from a link point to another block to
connect them with a curved arrow, or pick a link tool in the palette (Arrow, Broken Arrow, Link) and
drag from block to block. The route leaves and enters the blocks at right angles to their sides and
follows them when they move. To move an end, select the connector and drag the square handle at
that end to another port of the same block or to another block. Routing, arrowhead, color, width
and a caption shown in the middle of the line can be changed in the Properties window.

**Electrical schematics.** Pick a component in the Electrical palette. Drag from a terminal to another component to draw a wire, or select the Wire tool first. Use Wire junction for branches. Terminals follow a component when moved or rotated. Set its reference in Caption and its rating or part number in Component value. Switch contacts can be drawn open or closed.

**Undo.** Every change (mouse edits, menu commands, property edits) is one undo step. A step is a
snapshot of the document XML, so undo is exact for any operation; the last 100 steps are kept.

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

## Simulation and code export

The **Plugins** menu lists commands from optional `IDocumentTool` assemblies in the executable's `Plugins` directory. Each DLL can be distributed or removed separately. Restart the editor after changing the contents of this directory. **Plugins → Examples** opens a fresh copy of a diagram in its own tab. The first save asks for a destination. Opening a regular `.mfb` file still edits that file.

- **Circuit simulation.** Open **RC-low-pass**, then **Plugins → Circuit simulation**. Select the voltage at C1 or a component current and press **Run**. The oscilloscope shows time in seconds and the selected quantity in volts or amperes. Change the integration step in microseconds, pause, or reset to zero initial energy. **DC operating point** starts at DC equilibrium. **AC sweep** plots the selected node's voltage magnitude from 1 Hz to 100 kHz for a circuit with an AC source. With 1 kΩ and 1 µF the example's cutoff is about 159 Hz.
- The simulator joins explicit wire endpoints and junctions. Connect both terminals of each component and provide ground and a source. Set **Component value** to e.g. `1k`, `100nF`, `10mH`, `9V`, `5V 50Hz`, `12V 5W` or `2A`. The AC amplitude is peak voltage. The model supports R, L, C, batteries, DC/AC sources, piecewise-linear diodes, resistive lamps, switches and fuses. Switch controls change the simulation session. Reset clears a blown fuse. NPN simulation is unsupported and produces a diagnostic. The adapted MNA solver and SI units parser are from Schematic Editor; its MIT notice ships with the plugin.
- **Flowchart → Python.** Open **Countdown**, choose the export command and save the preview as `countdown.py`. Running `python countdown.py` prints 3, 2 and 1. A diagram starts at one Begin and reaches its blocks through directed connectors. Operation takes a single Python statement, Subroutine a call, If a Python expression, and In/Out takes `input name` (a string) or `output expression`. Label the two outgoing If connections `true` and `false` (also yes/no, да/нет or так/ні). Simple sequences, single-entry decisions and pre-test loops produce Python statements, `if` / `else` and `while`. Other control-flow graphs use a dispatch loop. Python snippets in captions are copied into the exported source. The preview stays open while you edit the diagram. Click a code line to select its block, or select a block to highlight its code. The preview lets you save the program for execution in Python.

### State diagrams — try a book loan

Open **Plugins → Examples → Book-loan**, then **Plugins → State diagram…**. The **Transitions** tab starts at **Активна** (active). Click **Книгу вернули** (book returned) for an on-time return. Click **Restart**, choose **Истёк срок** (due date passed), then **Книгу вернули** to return an overdue loan. Both paths end at **Закрыта** (closed). The canvas highlights the current state, and the history records each event and its destination. You can change the diagram with the window open. The runner keeps a path that still exists and restarts when an edit invalidates it.

To draw an automaton, place **State** blocks from the **States** palette. Give each a unique caption. The first state placed is initial. Use **Initial state** and **Final state** in Properties to edit these markers. Drag from a port to another state to create a **Transition**, then set its caption to the event name. To create a self-transition, drag between two different ports of the same state. Event names must be unique within a source state. The tool links validation messages to the affected elements for corrections.

On the **Python** tab, save the module as `book_loan.py`. It uses the standard library and the same transitions as the editor.

```python
from book_loan import StateMachine

loan = StateMachine()
assert loan.state.value == "Активна"
assert loan.handle("Истёк срок").value == "Просрочена"
assert loan.handle("Книгу вернули").value == "Закрыта"
assert loan.is_final and loan.available_events() == ()
loan.reset()
```

`handle(event)` raises `ValueError` if the event is unavailable. The machine stays in its current state.

### Class diagrams and Python

Open **Plugins → Examples → Library-classes**, then **Plugins → Class diagram → Python**. The example models a library member and a loan. Save the preview as `models.py`. The generated module contains typed dataclasses with constructors and method stubs. For example, in Python:

```python
from models import Loan, Member

reader = Member(name="Ada", member_id=7)
loan = Loan(due_day=14, borrower=reader)
assert loan.borrower.name == "Ada"
```

To create a diagram, place a **Class** from the **UML classes** palette. Set **Caption** to the Python class name. Enter one field per line in **Attributes** (e.g. `name: str`) and one signature per line in **Methods** (e.g. `can_borrow(active_loans: int) -> bool`). The generator adds `self` and leaves the method body for you to implement.

Draw **Inheritance** from a child to its base class. Draw **Association** from the class that owns a reference to its target, then set the arrow's **Caption** to the field name (e.g. `borrower`). The exporter supports single inheritance and any number of named associations. Use ASCII Python identifiers for class, field and method names. Field and method types can use class names, scalar types, typed collections such as `list[str]` and unions such as `Member | None`. Use Python 3.10 or newer for these annotations. The preview updates after edits and undo. Click a diagnostic to select the class or relationship that needs attention.

### First lesson — fix the countdown

1. Open **Plugins → Examples → Countdown-exercise**. Predict what happens when `n` reaches zero.
2. Choose **Plugins → Check flowchart**. The two exits of `n > 0` have identical labels. Select each issue to find its arrow on the canvas. Select the arrow leading to End and change its **Caption** to `false` in Properties. The issue list updates when you finish the edit.
3. Open **Plugins → Flowchart → Python**. Select the line `while n > 0:` to find the decision. Select the block labelled `n -= 1` to find the decrement in Python.
4. Predict the output, then save the preview as `countdown.py` and run `python countdown.py`. The output is 3, 2, 1 on separate lines. Explain why zero is skipped. Change the starting value to 5 and check your prediction again.

### Filter lab — find the cutoff

1. Open **Plugins → Examples → RC-low-pass**, then **Plugins → Circuit simulation → Gain and phase…**. Select the source terminal as input and the resistor-capacitor junction as output. The RC example selects this pair by default.
2. Click either plot to move the shared cursor, or enter **159.15 Hz**. With 1 kΩ and 1 µF, the output/input voltage ratio is about **−3.01 dB** and the phase is **−45°**.
3. Close both measurement and simulation windows. Set C1’s **Component value** to `2uF`, reopen the measurement, and find the same gain and phase at **79.58 Hz**. The time constant doubled, so the cutoff frequency halved.

## Building

You need Windows and Visual Studio 2026 or the .NET 11 SDK.

```
dotnet build FlowchartBuilder.slnx -c Release
```

The application is built into `Build\<Configuration>` and the glyph plugins into
`Build\<Configuration>\Plugins`. Menus, icons, images and translations are copied next to the
executable, so the build output runs as is.

`MakeFlowchartBuilder\Make.cmd` builds the Release configuration and, if Inno Setup is installed,
packs it into an installer in `MakeFlowchartBuilder\Output`. The installed application needs the
.NET 11 Desktop Runtime.

To move to a newer .NET, change `TargetFramework` in `Directory.Build.props`.

## Status

The editor opens an infinite canvas. Older paper formats load on that canvas. Document tabs,
keyboard shortcuts, pan and zoom, editable connector ends, grouping, locks, undo/redo and bitmap
export are implemented. Save As, Save All and close commands use the document's dirty state.
WindowsManager manages tool windows. Core owns editor services and the command and glyph
metadata caches. Direct2D render targets manage their drawing resources.

| Feature | How to use it |
|---|---|
| Image and Text | Pick a tool in the Common group. Image embeds the selected file in the document. The display bitmap is sampled to a maximum of 4096 pixels on its longer side. Edit a text block's caption, font and colour in Properties. |
| Placement and styling | Double-click a palette icon repeatedly to place blocks in free positions around the viewport centre. Drag a resize or rotation handle to snap the pointer to it. Blocks have shadows and palette icons have a uniform background. |
| Print and preview | File → Page Setup sets the paper and margins. Print Preview and Print tile the content at actual size, from left to right and then top to bottom. |
| Clipboard | Copy or cut selected blocks, then paste into the same or another document. Groups and connections between copied blocks travel with them. Other applications can read the selection as XML text. Repeated pastes are offset by 5 mm. |
| Languages and units | On first use the editor selects English, Russian or Ukrainian from the Windows UI culture, and millimetres or inches from regional settings. Explicit preferences take precedence. |
| About | Help → About shows the editor version, copyright and full MIT license. |

Run the Windows test suite with `dotnet test FlowchartBuilder.slnx -c Release`. It checks history,
XML settings and clipboard fragments, translations, connector routing, Direct2D bitmap drawing
and tiled pagination.

## License

Copyright © 2008-2026 Mykhailo Makarov. Distributed under the [MIT License](LICENSE).

## Support

If you found this project interesting or useful, you can support my work:

[![GitHub Sponsors](https://img.shields.io/github/sponsors/makarov-mm?style=flat&logo=github)](https://github.com/sponsors/makarov-mm)
