using Makarov.Framework.Core;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using BaseSettings = Makarov.Framework.Core.Settings;

namespace Makarov.FlowchartBuilder
{

    public static class Settings
    {
        #region Files

        public static class Files
        {

            public static string MainMenuDefinition
            {
                get { return Path.Combine(Directories.Data, "MainMenu.xml"); }
            }

            public static string MainMenuSchema
            {
                get { return Path.Combine(Directories.Data, "MainMenu.xsd"); }
            }

            public static string SecondaryMenuDefinition
            {
                get { return Path.Combine(Directories.Data, "SecondaryMenu.xml"); }
            }

            public static string SecondaryMenuSchema
            {
                get { return Path.Combine(Directories.Data, "SecondaryMenu.xsd"); }
            }

            public static string UnitsDefinition
            {
                get { return Path.Combine(Directories.Data, "Units.xml"); }
            }

            public static string UnitsSchema
            {
                get { return Path.Combine(Directories.Data, "Units.xsd"); }
            }

            public static string TranslationsSchema
            {
                get { return Path.Combine(Directories.Translations, "Translations.xsd"); }
            }

            public static string SerializationDll
            {
                get { return Path.Combine(Directories.Main, @"Makarov.Framework.Serialization.dll"); }
            }

            public static string XmlSettingsFile
            {
                get { return @"FlowchartBuilderSettings.xml"; }
            }
        }
        #endregion

        #region Directories

        public static class Directories
        {

            public static string Main
            {
                get
                {
                    if (Application.StartupPath.ToLower().Contains("nunit") ||
                        Application.StartupPath.ToLower().Contains("sharper"))
                        return @"C:\MakarovProject\Makarov\FlowchartBuilder\FlowchartBuilder";

                    return Application.StartupPath;
                }
            }

            public static string TempDirectory
            {
                get
                {
                    return Path.Combine(System.Environment.GetFolderPath(
                      System.Environment.SpecialFolder.LocalApplicationData),
                      "FlowchartBuilderTemp");
                }
            }

            public static string AddFilesDirectory
            {
                get
                {
                    return Path.Combine(
                        TempDirectory,
                        "FilesToAdd");
                }
            }

            public static string Data
            {
                get { return Path.Combine(Main, "Data"); }
            }

            public static string Icons
            {
                get { return Path.Combine(Main, "Icons"); }
            }

            public static string Graphics
            {
                get { return Path.Combine(Main, "Graphics"); }
            }

            public static string Plugins
            {
                get { return Path.Combine(Main, "Plugins"); }
            }

            public static string Translations
            {
                get { return Path.Combine(Main, "Translations"); }
            }
        }
        #endregion

        #region Registry keys

        public static class RegistryKeys
        {

            public static string ProgramSettingsKey
            {
                get { return @"Software\MakarovFlowchartBuilder"; }
            }

            public static string FirstStart
            {
                get { return "FirstStart"; }
            }

            public static string LastStart
            {
                get { return "LastStart"; }
            }

            public static string Language
            {
                get { return "Language"; }
            }

            public static string NextLanguage
            {
                get { return "NextLanguage"; }
            }

            public static string Rulers
            {
                get { return "Rulers"; }
            }

            public static string Grid
            {
                get { return "Grid"; }
            }

            public static string Units
            {
                get { return "Units"; }
            }

            public static string SizeLocationLabel
            {
                get { return "SizeAndLocationLabel"; }
            }

            public static string SecondaryWindowParams
            {
                get { return "SecWndParams"; }
            }

            public static class MainWindow
            {

                public static string X
                {
                    get { return "X"; }
                }

                public static string Y
                {
                    get { return "Y"; }
                }

                public static string Width
                {
                    get { return "Width"; }
                }

                public static string Height
                {
                    get { return "Height"; }
                }

                public static string WindowState
                {
                    get { return "WindowState"; }
                }
            }

            public static string LimitedHistory
            {
                get { return "LimitedHistory"; }
            }

            public static string CacheResources
            {
                get { return "CacheResources"; }
            }

            public static string CacheImages
            {
                get { return "CacheImages"; }
            }

            public static string Antialiasing
            {
                get { return "UseAntialiasing"; }
            }
        }
        #endregion

        #region Brushes

        public static class Brushes
        {

            public static Brush ThumbnailText
            {
                get
                {
                    if (!Core.Instance.CachedBrushes.KeyExists("ThumbnailText"))
                        return Core.Instance.CachedBrushes["ThumbnailText"] = new SolidBrush(Colors.ThumbnailText);

                    return Core.Instance.CachedBrushes["ThumbnailText"];
                }
            }

            public static Brush Selection
            {
                get
                {
                    if (!Core.Instance.CachedBrushes.KeyExists("Selection"))
                        return Core.Instance.CachedBrushes["Selection"] = new SolidBrush(Colors.Selection);

                    return Core.Instance.CachedBrushes["Selection"];
                }
            }

            public static Brush GroupSelection
            {
                get
                {
                    if (!Core.Instance.CachedBrushes.KeyExists("GroupSelection"))
                        return Core.Instance.CachedBrushes["GroupSelection"] = new SolidBrush(Colors.GroupSelection);

                    return Core.Instance.CachedBrushes["GroupSelection"];
                }
            }
        }
        #endregion

        #region Pens

        public static class Pens
        {

            public static Pen SheetGrid
            {
                get
                {
                    if (!Core.Instance.CachedPens.KeyExists("SheetGrid"))
                        return Core.Instance.CachedPens["SheetGrid"] = new Pen(Colors.SheetGrid);

                    return Core.Instance.CachedPens["SheetGrid"];
                }
            }

            public static Pen RulersBorder
            {
                get
                {
                    if (!Core.Instance.CachedPens.KeyExists("RulersBorder"))
                        return Core.Instance.CachedPens["RulersBorder"] = new Pen(Colors.RulersBorder);

                    return Core.Instance.CachedPens["RulersBorder"];
                }
            }

            public static Pen ThumbnailBorder
            {
                get
                {
                    if (!Core.Instance.CachedPens.KeyExists("ThumbnailBorder"))
                    {
                        var pen = new Pen(Colors.ThumbnailBorder) {LineJoin = LineJoin.Bevel};
                        return Core.Instance.CachedPens["ThumbnailBorder"] = pen;
                    }

                    return Core.Instance.CachedPens["ThumbnailBorder"];
                }
            }

            public static Pen SelectionBorder
            {
                get
                {
                    if (!Core.Instance.CachedPens.KeyExists("SelectionBorder"))
                    {
                        var pen = new Pen(Colors.SelectionBorder) {LineJoin = LineJoin.Bevel};
                        return Core.Instance.CachedPens["SelectionBorder"] = pen;
                    }

                    return Core.Instance.CachedPens["SelectionBorder"];
                }
            }

            public static Pen GroupSelectionBorder
            {
                get
                {
                    if (!Core.Instance.CachedPens.KeyExists("GroupSelectionBorder"))
                        return Core.Instance.CachedPens["GroupSelectionBorder"] = new Pen(Colors.GroupSelectionBorder);

                    return Core.Instance.CachedPens["GroupSelectionBorder"];
                }
            }
        }
        #endregion

        #region Colors

        public static class Colors
        {

            public static Color WorkArea
            {
                get { return Color.FromArgb(255, 128, 128, 128); }
            }

            public static Color Sheet
            {
                get { return Color.White; }
            }

            public static Color SheetGrid
            {
                get
                {
                    return Color.FromArgb(24, SelectionBorder);
                }
            }

            public static Color RulersBackground
            {
                get { return Color.White; }
            }

            public static Color RulersBorder
            {
                get { return Color.Black; }
            }

            public static Color ThumbnailBorder
            {
                get { return Color.FromArgb(255, 60, 70, 160); }
            }

            public static Color ThumbnailGradientStart
            {
                get { return Color.FromArgb(255, 200, 200, 225); }
            }

            public static Color ThumbnailGradientEnd
            {
                get { return Color.White; }
            }

            public static Color ThumbnailObjectGradientStart
            {
                get { return Color.FromArgb(255, 228, 228, 240); }
            }

            public static Color ThumbnailObjectGradientEnd
            {
                get { return Color.FromArgb(255, 228, 228, 240); }
            }

            public static Color ThumbnailText
            {
                get { return ThumbnailBorder; }
            }

            public static Color Text
            {
                get { return Color.FromArgb(255, 60, 70, 160); }
            }

            public static Color SelectionBorder
            {
                get { return Color.FromArgb(128, 0, 0, 255); }
            }

            public static Color GroupSelectionBorder
            {
                get { return Color.FromArgb(128, 255, 0, 0); }
            }

            public static Color Selection
            {
                get { return Color.FromArgb(32, SelectionBorder); }
            }

            public static Color GroupSelection
            {
                get { return Color.FromArgb(32, GroupSelectionBorder); }
            }
        }
        #endregion

        #region Environment

        public static class Environment
        {

            public static bool IsDebug
            {
                get
                {
#if DEBUG
                    return true;
#else
                    return false;
#endif
                }
            }

            public static bool CanUseNative
            {
                get { return !Framework.Core.Settings.Environment.IsMono; }
            }

            public static bool InDesignTime
            {
                get { return !Core.IsInstanceExists && IsDebug; }
            }

            public static string DefaultUnitsName
            {
                get { return @"Millimeters"; }
            }

            public static Units CurrentUnits
            {
                get
                {
                    string unitsName = Core.Instance.CachedSettingsStorage[RegistryKeys.Units];

                    if (string.IsNullOrEmpty(unitsName))
                    {
                        Units units = Core.Instance.Units[DefaultUnitsName];
                        CurrentUnits = units;
                        return units;
                    }

                    return Core.Instance.Units[unitsName];
                }

                set
                {
                    Core.Instance.CachedSettingsStorage[RegistryKeys.Units] = value.Name;
                }
            }

            public static bool Rulers
            {
                get
                {
                    var rulers = Core.Instance.CachedSettingsStorage[RegistryKeys.Rulers];
                    bool result;
                    if (bool.TryParse(rulers, out result))
                        return result;
                    return Rulers = true;
                }

                set
                {
                    Core.Instance.CachedSettingsStorage[RegistryKeys.Rulers] = value.ToString();
                }
            }

            public static int RulersSize
            {
                get { return 20; }
            }

            public static bool Grid
            {
                get
                {
                    var grid = Core.Instance.CachedSettingsStorage[RegistryKeys.Grid];
                    bool result;
                    if (bool.TryParse(grid, out result))
                        return result;
                    return Grid = true;
                }

                set
                {
                    Core.Instance.CachedSettingsStorage[RegistryKeys.Grid] = value.ToString();
                }
            }

            public static int GridSize
            {
                get { return 20; }
            }

            public static float DPI
            {
                get { return 96f; }
            }

            public static bool SizeLocationLabel
            {
                get
                {
                    var sizeLocationLabel = Core.Instance.CachedSettingsStorage[RegistryKeys.SizeLocationLabel];
                    bool result;
                    if (bool.TryParse(sizeLocationLabel, out result))
                        return result;
                    return SizeLocationLabel = true;
                }

                set
                {
                    Core.Instance.CachedSettingsStorage[RegistryKeys.SizeLocationLabel] = value.ToString();
                }
            }

            public static string ApplicationName
            {
                get { return "MakarovFlowchartBuilder"; }
            }

            public static string ServerPort
            {
                get { return "MakarovFlowchartBuilderServer"; }
            }

            public static string ClientPort
            {
                get { return "MakarovFlowchartBuilderClient"; }
            }

            public static string Version
            {
                get { return "1.0 beta"; }
            }

            public static bool Registered
            {
                get { return true; }
            }

            public static string ErrorReportMail
            {
                get { return @"m.m.makarov@gmail.com"; }
            }

            public static string ErrorReportTitle
            {
                get { return @"FlowchartBuilder error report"; }
            }

            public static class Docking
            {

                public static int DockDistance
                {
                    get { return 10; }
                }

                public static int UndockDistance
                {
                    get { return 50; }
                }
            }


            public static bool LimitedHistory
            {
                get
                {
                    var limitedHistory = Core.Instance.CachedSettingsStorage[RegistryKeys.LimitedHistory];
                    bool result;
                    if (bool.TryParse(limitedHistory, out result))
                        return result;
                    return LimitedHistory = false;
                }

                set
                {
                    Core.Instance.CachedSettingsStorage[RegistryKeys.LimitedHistory] = value.ToString();
                }
            }

            public static bool CacheResources
            {
                get
                {
                    var cacheResources = Core.Instance.CachedSettingsStorage[RegistryKeys.CacheResources];
                    bool result;
                    if (bool.TryParse(cacheResources, out result))
                        return result;
                    return CacheResources = true;
                }

                set
                {
                    Core.Instance.CachedSettingsStorage[RegistryKeys.CacheResources] = value.ToString();
                }
            }

            public static bool CacheImages
            {
                get
                {
                    var cacheImages = Core.Instance.CachedSettingsStorage[RegistryKeys.CacheImages];
                    bool result;
                    if (bool.TryParse(cacheImages, out result))
                        return result;
                    return CacheImages = true;
                }

                set
                {
                    Core.Instance.CachedSettingsStorage[RegistryKeys.CacheImages] = value.ToString();
                }
            }

            public static bool Antialiasing
            {
                get
                {
                    var antialiasing = Core.Instance.CachedSettingsStorage[RegistryKeys.Antialiasing];
                    bool result;
                    if (bool.TryParse(antialiasing, out result))
                        return result;
                    return Antialiasing = true;
                }

                set
                {
                    Core.Instance.CachedSettingsStorage[RegistryKeys.Antialiasing] = value.ToString();
                }
            }

            public static string AppFileExtension
            {
                get { return "mfb"; }
            }

            public static string AppSettingsExtension
            {
                get { return "fbsettings"; }
            }

            public static class Languages
            {

                private static bool _currLangUpdated;

                public static string CurrentLanguage
                {
                    get
                    {
                        if (!_currLangUpdated)
                        {
                            var nextLang = Core.Instance.CachedSettingsStorage[RegistryKeys.NextLanguage];

                            if (!string.IsNullOrEmpty(nextLang))
                            {
                                Core.Instance.CachedSettingsStorage[RegistryKeys.NextLanguage] = "";
                                Core.Instance.CachedSettingsStorage[RegistryKeys.Language] = nextLang;
                            }

                            _currLangUpdated = true;
                        }

                        var currLang = Core.Instance.CachedSettingsStorage[RegistryKeys.Language];

                        if (string.IsNullOrEmpty(currLang))
                        {
                            CurrentLanguage = "English";
                            return "English";
                        }

                        return currLang;
                    }

                    set
                    {
                        Core.Instance.CachedSettingsStorage[RegistryKeys.NextLanguage] = value;
                    }
                }
            }
        }
        #endregion

        #region Xml

        public static class Xml
        {
            #region Xml Namespaces

            public static class Namespaces
            {

                public const string Base = @"http://flowchartbuilder.blogspot.com";
            }
            #endregion

            #region Xml Tags

            public static class Tags
            {

                public const string MainMenu = @"fb:MainMenu";

                public const string SecondaryMenu = @"fb:SecondaryMenu";

                public const string Menu = @"fb:Menu";

                public const string Command = @"fb:Command";

                public const string Separator = @"fb:Separator";

                public const string UnitsTypes = @"fb:UnitsTypes";

                public const string FlowchartBuilderDocument = @"fb:FlowchartBuilderDocument";

                public const string Sheet = @"fb:Sheet";

                public const string Glyph = @"fb:Glyph";

                public const string Property = @"fb:Property";

                public const string Settings = @"fb:Settings";
            }
            #endregion

            #region Xml Attributes

            public static class Attributes
            {

                public const string Caption = @"Caption";

                public const string Class = @"Class";

                public const string Icon = @"Icon";

                public const string Shortcut = @"Shortcut";

                public const string ToolTip = @"ToolTip";

                public const string Enabled = @"Enabled";

                public const string Checked = @"Checked";

                public const string Name = @"Name";

                public const string ShortName = @"ShortName";

                public const string InMM = @"InMM";

                public const string BigStep = @"BigStep";

                public const string SmallStep = @"SmallStep";

                public static string Word = @"Word";

                public static string Key = @"Key";

                public const string Value = @"fb:Value";

                public const string SheetClassName = @"fb:ClassName";

                public const string GlyphClassName = @"fb:ClassName";

                public static string ProgramVersion
                {
                    get { return "Version"; }
                }
            }
            #endregion
        }
        #endregion
    }
}
