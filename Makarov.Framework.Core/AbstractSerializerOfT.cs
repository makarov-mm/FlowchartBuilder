namespace Makarov.Framework.Core;

/// <summary>
/// Strongly typed base for serializers. A <see langword="null"/> value serializes to an empty string.
/// </summary>
public abstract class AbstractSerializer<T> : AbstractSerializer
    where T : notnull
{
    public sealed override Type SerializerType => typeof(T);

    public sealed override string Serialize(object? value) => value is null ? string.Empty : Format((T)value);

    public sealed override object Deserialize(string text) => Parse(text);

    protected abstract string Format(T value);

    protected abstract T Parse(string text);
}
