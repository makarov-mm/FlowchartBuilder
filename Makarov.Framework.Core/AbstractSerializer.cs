namespace Makarov.Framework.Core;

/// <summary>
/// Converts values of <see cref="SerializerType"/> to and from their string representation.
/// </summary>
public abstract class AbstractSerializer
{
    public abstract Type SerializerType { get; }

    public abstract string Serialize(object? value);

    public abstract object Deserialize(string text);
}
