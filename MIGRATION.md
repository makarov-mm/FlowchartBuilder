# Migration to .NET 10 / latest C#

This document describes the modernization of FlowchartBuilder from
.NET Framework 4.8 / C# 3 to **.NET 10** and the **latest C# language version**.

## Target framework and language

- `LangVersion` = `latest`, applied to every project via `Directory.Build.props`.
- Windows desktop projects (WinForms / GDI+) target **`net10.0-windows`** with
  `<UseWindowsForms>true</UseWindowsForms>`.
- Pure library / tool projects target plain **`net10.0`**:
  `Makarov.Framework.Math`, `Makarov.Framework.Collections`,
  `Makarov.Framework.Instance`, `Makarov.Framework.Launcher`,
  `Makarov.Framework.VersionUpdater`.

## Project system

- All 19 projects converted from legacy MSBuild to **SDK-style** csproj.
- Common metadata (company, copyright, product, version, language version,
  nullable, assembly-info generation) centralized in **`Directory.Build.props`**.
- All hand-written `Properties/AssemblyInfo.cs` files removed; assembly
  attributes are now generated from `Directory.Build.props`
  (`GenerateAssemblyInfo=true`). Version bumped to `2.0.0`.
- The solution was regenerated (`FlowchartBuilder.sln`) and now contains only
  the real projects (the old solution folders and the never-present
  `Graphics.OpenGL` entry were dropped).
- Added `.gitignore` for `bin/`, `obj/`, `.vs/`, `*.user`.

## Breaking-API changes fixed

### 1. .NET Remoting -> Mutex + named pipes (single instance)

`System.Runtime.Remoting` was removed in modern .NET. The single-instance
mechanism was rewritten:

- `Makarov.Framework.Instance/InstanceHelper.cs` now uses a named `Mutex`
  (primary-instance detection) plus a background `NamedPipeServerStream`
  listener. Subsequent instances send their command-line arguments through a
  `NamedPipeClientStream`.
- `IInstance` keeps the same `Message(string[])` contract.
- `Program.cs` uses `InstanceHelper.TryStartPrimary` / `SendToPrimary` /
  `Stop` instead of Remoting registration.
- `AppInstance` no longer derives from `MarshalByRefObject`. Its `Message`
  handler now marshals back to the UI thread (`InvokeRequired` / `BeginInvoke`),
  which the Remoting version did not do safely.

### 2. `AppDomain.DefineDynamicAssembly` -> `AssemblyBuilder.DefineDynamicAssembly`

`Makarov.Framework.Core/Managers/ProxyManager.cs` used
`AppDomain.CurrentDomain.DefineDynamicAssembly`, which does not exist in modern
.NET. Switched to the static `AssemblyBuilder.DefineDynamicAssembly` with
`AssemblyBuilderAccess.Run` (fully supported on .NET 10).

### 3. Code Access Security attribute removed

`Makarov.Framework.Graphics/Gdi.cs` had a
`[SecurityPermission(SecurityAction.LinkDemand, ...)]` attribute. CAS is gone in
modern .NET; the attribute (and its `using`) were removed.

### 4. `VersionUpdater` decoupled from WinForms

`Application.StartupPath` replaced with `AppContext.BaseDirectory`, so the
console tool no longer needs WinForms and now targets plain `net10.0`.

## Comments and localization

- All comments were removed from the source (Russian XML-doc/inline commentary
  and generated designer noise), using a C#-aware stripper that preserves string
  and character literals. 297 `.cs` files processed.
- The only Russian string literal in code, `[GlyphName("Треугольник")]`, was
  corrected to `[GlyphName("Triangle")]` to match the English naming of every
  other glyph.
- `todo.txt` translated to English. `README.md` tech/build/status sections
  updated and mojibake dashes repaired.

## Runtime assets

The app loads menus, icons, translations and plugins from disk relative to the
executable. The main project now copies `Data`, `Icons`, `Graphics` and
`Translations` to the output directory on build (`CopyToOutputDirectory`), so a
fresh build is runnable without the installer. Stale committed binaries
(old `.dll` / `.exe` / `.pdb`, the pre-built installer and its staging folder)
were removed from the tree.

## Build verification

Built and verified with the **.NET 10.0.109 SDK** in this environment:

- `Makarov.Framework.Math` - OK
- `Makarov.Framework.Collections` - OK
- `Makarov.Framework.Instance` (rewritten IPC) - OK
- `Makarov.Framework.VersionUpdater` - OK
- `Makarov.Framework.Launcher` - OK

The 14 `net10.0-windows` (WinForms / GDI+) projects could **not** be compiled in
this Linux sandbox because the Windows Desktop reference packs are not
downloadable here. They were migrated by hand; the only framework-breaking APIs
they contained (Remoting, `AppDomain.DefineDynamicAssembly`, CAS) are fixed, and
everything else is standard WinForms/GDI+ that is fully supported on
`net10.0-windows`. Build them on Windows with Visual Studio 2022 or
`dotnet build`.

## Not yet done (deferred to the follow-up phase)

- The InnoSetup pipeline (`MakeFlowchartBuilder/*.iss`, `Make.cmd`) still
  reflects the old .NET Framework layout and needs reworking for a
  self-contained / framework-dependent .NET 10 publish.
- `VersionUpdater` bumped versions by rewriting `AssemblyInfo.cs`; since
  versioning moved to `Directory.Build.props`, it should be repointed there.
- Optional: enable nullable reference types and `ImplicitUsings` (left disabled
  to keep this migration behavior-preserving).
