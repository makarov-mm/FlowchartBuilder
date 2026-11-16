namespace Makarov.Framework.Core.SettingStorages;

/// <summary>
/// Read-through, write-through cache in front of a slower storage (registry reads happen while painting).
/// </summary>
public sealed class CachedStorage(ISettingsStorage storage) : ISettingsStorage
{
    private readonly Dictionary<string, string?> _cache = new(StringComparer.Ordinal);

    public string? this[string key]
    {
        get
        {
            if (!_cache.TryGetValue(key, out string? value))
            {
                value = storage[key];
                _cache.Add(key, value);
            }

            return value;
        }

        set
        {
            if (_cache.TryGetValue(key, out string? cached) && cached == value)
                return;

            _cache[key] = value;
            storage[key] = value;
        }
    }

    /// <summary>Writes go through to the underlying storage, so it knows every key.</summary>
    public IEnumerable<string> Keys => storage.Keys;

    public bool Contains(string key) =>
        _cache.TryGetValue(key, out string? value) ? value is not null : storage.Contains(key);
}
