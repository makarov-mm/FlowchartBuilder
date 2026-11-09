using Microsoft.Win32;

namespace Makarov.Framework.Core.SettingStorages;

/// <summary>
/// Stores settings as string values under a key in HKEY_CURRENT_USER.
/// </summary>
public sealed class RegistryStorage : ISettingsStorage, IDisposable
{
    private readonly RegistryKey _key;

    /// <exception cref="UnauthorizedAccessException">The key cannot be opened for writing.</exception>
    public RegistryStorage(string keyPath)
    {
        _key = Registry.CurrentUser.CreateSubKey(keyPath, writable: true)
            ?? throw new UnauthorizedAccessException($@"Registry key 'HKCU\{keyPath}' is not accessible.");
    }

    public string? this[string key]
    {
        get => _key.GetValue(key) as string;
        set
        {
            if (value is null)
                _key.DeleteValue(key, throwOnMissingValue: false);
            else
                _key.SetValue(key, value);
        }
    }

    public IEnumerable<string> Keys => _key.GetValueNames();

    public bool Contains(string key) => !string.IsNullOrEmpty(this[key]);

    public void Dispose() => _key.Dispose();
}
