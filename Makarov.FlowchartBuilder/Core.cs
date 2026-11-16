using System.Globalization;
using System.Security;
using System.Xml;
using Makarov.FlowchartBuilder.Forms;
using Makarov.FlowchartBuilder.Glyphs;
using Makarov.FlowchartBuilder.Sheets;
using Makarov.Framework.Core;
using Makarov.Framework.Core.Managers;
using Makarov.Framework.Core.SettingStorages;
using Makarov.Framework.Serialization;

namespace Makarov.FlowchartBuilder;

/// <summary>
/// Application root: owns the open documents and the shared services (translations, units, images, plugins,
/// settings) and drives redrawing of the main window. Rendering lives in Core_Rendering.cs.
/// </summary>
public sealed partial class Core : IDisposable
{
    private static Core? _instance;

    private readonly List<Document> _documents = [];
    private readonly WindowsManager _windows = new();
    private int _currentDocumentIndex = -1;
    private int _redrawSuspensions;
    private bool _disposed;
    private Type? _currentGlyphType;
    private readonly Dictionary<string, Commands.Command> _commands = new(StringComparer.Ordinal);
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Type, GlyphMetadata> _glyphMetadata = new();

    internal GlyphMetadata MetadataFor(Type type) => _glyphMetadata.GetOrAdd(type, static key => new GlyphMetadata(key));

    internal Commands.Command CommandFor(string name)
    {
        if (!_commands.TryGetValue(name, out var command))
        {
            command = Commands.Command.Create(name);
            _commands.Add(name, command);
        }
        return command;
    }

    private MainForm? _mainWindow;
    private RegistryStorage? _registryStorage;
    private ISettingsStorage? _settingsStorage;
    private TranslatorsManager? _translators;
    private UnitsManager? _units;
    private SerializerManager? _serializers;
    private PluginLoader? _plugins;
    private GraphicsManager? _icons;
    private IReadOnlyList<Type>? _glyphTypes;

    private Core(ISettingsStorage? storage)
    {
        _settingsStorage = storage;
        IsFirstRun = !SettingsStorage.Contains(Settings.Keys.FirstStart);

        string now = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        if (IsFirstRun)
            SettingsStorage[Settings.Keys.FirstStart] = now;

        SettingsStorage[Settings.Keys.LastStart] = now;
    }

    internal static Core Initialize(ISettingsStorage? storage = null) => _instance ??= new Core(storage);

    public static Core Instance => Initialize();

    /// <summary>Raised when the glyph type armed for placement changes, including when it is cleared.</summary>
    public event EventHandler? CurrentGlyphTypeChanged;

    public bool IsFirstRun { get; }

    public MainForm MainWindow => _mainWindow ??= _windows.GetWindow<MainForm>();

    public WindowsManager Windows => _windows;

    public ISettingsStorage SettingsStorage => _settingsStorage ??= new CachedStorage(CreateSettingsStorage());

    public TranslatorsManager Translators => _translators ??=
        TranslatorsManager.LoadFromFolder(Settings.Directories.Translations, Settings.Files.TranslationsSchema);

    /// <summary>The translator of the session language, falling back to English if that language is not installed.</summary>
    public Translator CurrentTranslator
    {
        get
        {
            string language = Settings.Environment.Languages.CurrentLanguage;
            return Translators.Contains(language)
                ? Translators[language]
                : Translators["English"];
        }
    }

    public UnitsManager Units => _units ??= new UnitsManager(Settings.Files.UnitsDefinition, Settings.Files.UnitsSchema);

    public SerializerManager Serializers => _serializers ??= new SerializerManager(typeof(BoolSerializer).Assembly);

    public PluginLoader Plugins => _plugins ??= new PluginLoader(Settings.Directories.Plugins);

    public GraphicsManager Icons => _icons ??= new GraphicsManager(Settings.Directories.Icons);

    /// <summary>Concrete glyph types found in the plugin assemblies.</summary>
    public IReadOnlyList<Type> GlyphTypes => _glyphTypes ??=
    [
        .. Plugins.Types.Where(type => type is { IsClass: true, IsAbstract: false } && type.IsSubclassOf(typeof(AbstractGlyph))),
    ];

    /// <summary>Glyph type chosen in the palette and waiting to be placed with a click on the sheet.</summary>
    public Type? CurrentGlyphType
    {
        get => _currentGlyphType;
        set
        {
            if (_currentGlyphType == value)
                return;

            _currentGlyphType = value;
            CurrentGlyphTypeChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool HasDocuments => _documents.Count > 0;

    public Document CurrentDocument =>
        HasDocuments ? _documents[_currentDocumentIndex] : throw new InvalidOperationException("No document is open.");

    /// <summary>Sheet of the current document, or <see langword="null"/> when no document is open.</summary>
    public Sheet? ActiveSheet => HasDocuments ? CurrentDocument.DocumentSheet : null;

    public void Run(IEnumerable<string> fileNames)
    {
        foreach (string fileName in fileNames.Where(name => !string.IsNullOrWhiteSpace(name)))
            TryOpenDocument(fileName);

        Application.Run(MainWindow);
    }

    public T GetWindowAsChild<T>()
        where T : Form, new()
    {
        if (typeof(T) == typeof(MainForm))
            throw new InvalidOperationException("The main window cannot be owned by itself.");

        T window = _windows.GetWindow<T>();
        if (window.TopLevel && window.Owner != MainWindow)
            window.Owner = MainWindow;

        return window;
    }

    /// <summary>Docks the palette to the left and the properties window to the right of the work area.</summary>
    public void SetDefaultWindowsPos() => MainWindow.ResetTools();

    public void ShowError(string message) =>
        MessageBox.Show(
            _mainWindow is { IsHandleCreated: true } ? _mainWindow : null,
            message,
            CurrentTranslator["Msg_Error"],
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _windows.Dispose();
        _icons?.Dispose();
        _registryStorage?.Dispose();
        _instance = null;
    }

    /// <summary>Settings go to the registry; if it is not writable, to an XML file; failing that, to memory.</summary>
    private ISettingsStorage CreateSettingsStorage()
    {
        try
        {
            return _registryStorage = new RegistryStorage(Settings.Keys.RegistryPath);
        }
        catch (Exception e) when (e is UnauthorizedAccessException or SecurityException or IOException)
        {
        }

        try
        {
            return new XmlStorage(Settings.Files.XmlSettingsFile);
        }
        catch (Exception e) when (e is UnauthorizedAccessException or SecurityException or IOException or XmlException)
        {
        }

        return new MemoryStorage();
    }
}
