using System.Globalization;
using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization;

/// <summary>
/// Culture-independent serializer for any parsable and formattable type (numbers, characters).
/// Documents written on a machine with one regional setting must load on any other.
/// </summary>
public abstract class InvariantSerializer<T> : AbstractSerializer<T>
    where T : IParsable<T>, IFormattable
{
    protected override string Format(T value) => value.ToString(null, CultureInfo.InvariantCulture);

    protected override T Parse(string text) => T.Parse(text, CultureInfo.InvariantCulture);
}
