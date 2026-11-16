using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization;

public sealed class StringSerializer : AbstractSerializer<string>
{
    protected override string Format(string value) => value;

    protected override string Parse(string text) => text;
}
