namespace Makarov.Framework.Core.SettingStorages;

/// <summary>
/// Non-persistent storage, the last-resort fallback when neither the registry nor the file system is writable.
/// </summary>
public sealed class MemoryStorage : ISettingsStorage
{
    private readonly Dictionary<string, string?> _values = new(StringComparer.Ordinal);

    public string? this[string key]
    {
        get => _values.GetValueOrDefault(key);
        set => _values[key] = value;
    }

    public IEnumerable<string> Keys => _values.Where(pair => pair.Value is not null).Select(pair => pair.Key);

    public bool Contains(string key) => _values.ContainsKey(key);
}
