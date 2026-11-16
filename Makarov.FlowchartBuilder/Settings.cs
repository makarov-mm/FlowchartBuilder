using System.Reflection;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder;

/// <summary>
/// Application constants, file locations, shared drawing resources and the user settings
/// persisted through <see cref="Core.SettingsStorage"/>.
/// </summary>
public static class Settings
{
    public static class Files
    {
        public const string XmlSettingsFile = "FlowchartBuilderSettings.xml";

        public static string MainMenuDefinition => Path.Combine(Directories.Data, "MainMenu.xml");

        public static string MainMenuSchema => Path.Combine(Directories.Data, "MainMenu.xsd");

        public static string SecondaryMenuDefinition => Path.Combine(Directories.Data, "SecondaryMenu.xml");

        public static string SecondaryMenuSchema => Path.Combine(Directories.Data, "SecondaryMenu.xsd");

        public static string UnitsDefinition => Path.Combine(Directories.Data, "Units.xml");

        public static string UnitsSchema => Path.Combine(Directories.Data, "Units.xsd");

        public static string TranslationsSchema => Path.Combine(Directories.Translations, "Translations.xsd");
    }

    public static class Directories
    {
        public static string Main => AppContext.BaseDirectory;

        public static string Data => Path.Combine(Main, "Data");

        public static string Icons => Path.Combine(Main, "Icons");

        public static string Plugins => Path.Combine(Main, "Plugins");

        public static string Translations => Path.Combine(Main, "Translations");
    }

    /// <summary>Names of the persisted settings (registry values or XML entries).</summary>
    public static class Keys
    {
        public const string RegistryPath = @"Software\MakarovFlowchartBuilder";

        public const string FirstStart = "FirstStart";
        public const string LastStart = "LastStart";
        public const string Language = "Language";
        public const string NextLanguage = "NextLanguage";
        public const string Rulers = "Rulers";
        public const string Grid = "Grid";
        public const string Units = "Units";
        public const string SizeLocationLabel = "SizeAndLocationLabel";
        public const string Antialiasing = "UseAntialiasing";

        /// <summary>Prefix of the position settings of the docking tool windows.</summary>
        public const string SecondaryWindowParams = "SecWndParams";

        public static class MainWindow
        {
            public const string X = "X";
            public const string Y = "Y";
            public const string Width = "Width";
            public const string Height = "Height";
            public const string WindowState = "WindowState";
        }
    }

    public static class Colors
    {
        public static readonly Color WorkArea = Color.FromArgb(255, 128, 128, 128);
        public static readonly Color Sheet = Color.White;
        public static readonly Color SheetBorder = Color.Black;
        public static readonly Color SheetGradient = Color.FromArgb(255, 220, 220, 230);
        public static readonly Color RulersBackground = Color.White;
        public static readonly Color RulersBorder = Color.Black;
        public static readonly Color ThumbnailBorder = Color.FromArgb(255, 60, 70, 160);
        public static readonly Color ThumbnailGradientStart = Color.FromArgb(255, 200, 200, 225);
        public static readonly Color ThumbnailGradientEnd = Color.White;
        public static readonly Color ThumbnailObjectGradientStart = Color.FromArgb(255, 228, 228, 240);
        public static readonly Color ThumbnailObjectGradientEnd = Color.FromArgb(255, 228, 228, 240);
        public static readonly Color ThumbnailText = ThumbnailBorder;
        public static readonly Color SelectionBorder = Color.FromArgb(128, 0, 0, 255);
        public static readonly Color GroupSelectionBorder = Color.FromArgb(128, 255, 0, 0);
        public static readonly Color Selection = Color.FromArgb(32, SelectionBorder);
        public static readonly Color GroupSelection = Color.FromArgb(32, GroupSelectionBorder);
        public static readonly Color SheetGrid = Color.FromArgb(24, SelectionBorder);
        public static readonly Color SheetShadow = Color.FromArgb(128, 0, 0, 0);
        public static readonly Color RulersText = Color.Black;
        public static readonly Color Connector = Color.Black;
        public static readonly Color SelectedConnector = Color.DodgerBlue;
        public static readonly Color ConnectorCaptionBackground = Color.FromArgb(220, 255, 255, 255);
        public static readonly Color LinkPoint = Color.White;
        public static readonly Color LinkPointBorder = Color.SteelBlue;
    }

    /// <summary>Outline styles shared by the renderer.</summary>
    public static class Strokes
    {
        public static readonly Stroke SheetBorder = new(Colors.SheetBorder);
        public static readonly Stroke SheetGrid = new(Colors.SheetGrid) { Dash = new DashPattern(4f, 1f) };
        public static readonly Stroke RulersBorder = new(Colors.RulersBorder);
        public static readonly Stroke RulersTick = new(Colors.RulersText);
        public static readonly Stroke ThumbnailBorder = new(Colors.ThumbnailBorder) { Join = StrokeJoin.Bevel };
        public static readonly Stroke SelectionBorder = new(Colors.SelectionBorder) { Join = StrokeJoin.Bevel };
        public static readonly Stroke GroupSelectionBorder = new(Colors.GroupSelectionBorder);
    }

    public static class Environment
    {
        public const string ApplicationName = "MakarovFlowchartBuilder";
        public const string DefaultUnitsName = "Millimeters";
        public const string DocumentExtension = "mfb";
        public const string SettingsExtension = "fbsettings";
        public const int RulersSize = 20;
        public const float Dpi = 96f;

        private static Units? _currentUnits;

        /// <summary>Product version without the source revision suffix ("2.0.0+abc123" becomes "2.0.0").</summary>
        public static string Version { get; } =
            Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0]
            ?? string.Empty;

        public static string DocumentFilter => $"FlowchartBuilder Documents|*.{DocumentExtension}";

        public static Units CurrentUnits
        {
            get
            {
                if (_currentUnits is not null)
                    return _currentUnits;

                UnitsManager units = Core.Instance.Units;
                string? name = Core.Instance.SettingsStorage[Keys.Units];
                return _currentUnits = name is not null && units.Contains(name) ? units[name] : units[DefaultUnitsName];
            }

            set
            {
                _currentUnits = value;
                Core.Instance.SettingsStorage[Keys.Units] = value.Name;
            }
        }

        public static bool Rulers
        {
            get => GetFlag(Keys.Rulers, defaultValue: true);
            set => SetFlag(Keys.Rulers, value);
        }

        public static bool Grid
        {
            get => GetFlag(Keys.Grid, defaultValue: true);
            set => SetFlag(Keys.Grid, value);
        }

        public static bool SizeLocationLabel
        {
            get => GetFlag(Keys.SizeLocationLabel, defaultValue: true);
            set => SetFlag(Keys.SizeLocationLabel, value);
        }

        public static bool Antialiasing
        {
            get => GetFlag(Keys.Antialiasing, defaultValue: true);
            set => SetFlag(Keys.Antialiasing, value);
        }

        private static bool GetFlag(string key, bool defaultValue) =>
            bool.TryParse(Core.Instance.SettingsStorage[key], out bool value) ? value : defaultValue;

        private static void SetFlag(string key, bool value) =>
            Core.Instance.SettingsStorage[key] = value.ToString();

        public static class Docking
        {
            public const int DockDistance = 10;
            public const int UndockDistance = 50;
        }

        public static class Languages
        {
            public const string Default = "English";

            private static bool _pendingLanguageApplied;

            /// <summary>
            /// The language of this session. A newly chosen language is stored as pending and applied on the next start,
            /// because menus and windows are translated once when they are created.
            /// </summary>
            public static string CurrentLanguage
            {
                get
                {
                    var storage = Core.Instance.SettingsStorage;

                    if (!_pendingLanguageApplied)
                    {
                        string? next = storage[Keys.NextLanguage];
                        if (!string.IsNullOrEmpty(next))
                        {
                            storage[Keys.NextLanguage] = string.Empty;
                            storage[Keys.Language] = next;
                        }

                        _pendingLanguageApplied = true;
                    }

                    string? current = storage[Keys.Language];
                    return string.IsNullOrEmpty(current) ? Default : current;
                }

                set => Core.Instance.SettingsStorage[Keys.NextLanguage] = value;
            }
        }
    }

    public static class Xml
    {
        public const string Namespace = "http://flowchartbuilder.blogspot.com";

        public static class Tags
        {
            public const string MainMenu = "fb:MainMenu";
            public const string SecondaryMenu = "fb:SecondaryMenu";
            public const string Menu = "fb:Menu";
            public const string Command = "fb:Command";
            public const string Separator = "fb:Separator";
            public const string UnitsTypes = "fb:UnitsTypes";
            public const string FlowchartBuilderDocument = "fb:FlowchartBuilderDocument";
            public const string Sheet = "fb:Sheet";
            public const string Glyph = "fb:Glyph";
            public const string Property = "fb:Property";
            public const string Groups = "fb:Groups";
            public const string Group = "fb:Group";
            public const string GroupMember = "fb:Member";
        }

        public static class Attributes
        {
            public const string Caption = "Caption";
            public const string Class = "Class";
            public const string Icon = "Icon";
            public const string Shortcut = "Shortcut";
            public const string ToolTip = "ToolTip";
            public const string Enabled = "Enabled";
            public const string Checked = "Checked";
            public const string Name = "Name";
            public const string ShortName = "ShortName";
            public const string InMM = "InMM";
            public const string BigStep = "BigStep";
            public const string SmallStep = "SmallStep";
            public const string Value = "fb:Value";
            public const string ClassName = "fb:ClassName";
            public const string Id = "id";
            public const string Fixed = "fixed";
            public const string ProgramVersion = "Version";
        }
    }
}
