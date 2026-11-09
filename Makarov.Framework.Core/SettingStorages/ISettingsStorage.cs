namespace Makarov.Framework.Core.SettingStorages;

/// <summary>
/// Flat string key/value storage for user settings.
/// </summary>
public interface ISettingsStorage
{
    string? this[string key] { get; set; }

    /// <summary>Names of all stored settings.</summary>
    IEnumerable<string> Keys { get; }

    bool Contains(string key);
}
