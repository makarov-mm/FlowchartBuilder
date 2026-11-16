namespace Makarov.Framework.Instance;

/// <summary>
/// Receives the command-line arguments that secondary instances forward to the primary one.
/// </summary>
public interface IInstance
{
    /// <summary>Called on a background thread; implementations must marshal to the UI thread themselves.</summary>
    void Message(string[] messages);
}
