using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization;

public sealed class VersionSerializer : AbstractSerializer<Version>
{
    protected override string Format(Version value) => value.ToString();

    protected override Version Parse(string text) => Version.Parse(text);
}
