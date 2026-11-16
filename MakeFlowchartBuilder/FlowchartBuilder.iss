; Installer for FlowchartBuilder. Requires Inno Setup 6.3 or later; run Make.cmd to build the
; application first. The installed application needs the .NET 11 Desktop Runtime.

#define AppName "FlowchartBuilder"
#define AppExe "Makarov.FlowchartBuilder.exe"
#define BuildDir "..\Build\Release"
#define AppVersion GetVersionNumbersString(AddBackslash(SourcePath) + BuildDir + "\" + AppExe)

[Setup]
AppId={#AppName}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Michael Makarov
AppCopyright=Copyright (c) 2008-2026 Mykhailo Makarov
DefaultDirName={autopf}\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
LicenseFile={#BuildDir}\License.txt
UninstallDisplayIcon={app}\{#AppExe}
OutputDir=Output
OutputBaseFilename=FlowchartBuilderSetup-{#AppVersion}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequiredOverridesAllowed=dialog
ChangesAssociations=yes

[Tasks]
Name: desktopicon; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#BuildDir}\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExe}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Registry]
; Opens *.mfb documents with FlowchartBuilder.
Root: HKA; Subkey: "Software\Classes\.mfb\OpenWithProgids"; ValueType: string; ValueName: "FlowchartBuilder.Document"; ValueData: ""; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\FlowchartBuilder.Document"; ValueType: string; ValueName: ""; ValueData: "FlowchartBuilder document"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\FlowchartBuilder.Document\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\FlowchartFile.ico"
Root: HKA; Subkey: "Software\Classes\FlowchartBuilder.Document\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#AppExe}"" ""%1"""

[Run]
Filename: "{app}\{#AppExe}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent
