namespace Makarov.Framework.Core.Managers;

public sealed class TranslatorsManager
{
    private readonly Dictionary<string, Translator> _translators = new(StringComparer.Ordinal);

    public IEnumerable<string> Languages => _translators.Keys;

    public int Count => _translators.Count;

    public Translator this[string language] =>
        _translators.TryGetValue(language, out Translator? translator)
            ? translator
            : throw new KeyNotFoundException($"Language '{language}' is not loaded.");

    public static TranslatorsManager LoadFromFolder(string folder, string schemaFile)
    {
        var manager = new TranslatorsManager();

        foreach (string file in Directory.EnumerateFiles(folder, "*.xml"))
            manager.Add(Translator.Load(file, schemaFile));

        return manager;
    }

    public bool Contains(string language) => _translators.ContainsKey(language);

    public void Add(Translator translator)
    {
        if (!_translators.TryAdd(translator.Language, translator))
            throw new InvalidOperationException($"Language '{translator.Language}' is already loaded.");
    }
}
