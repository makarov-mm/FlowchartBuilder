using Makarov.Framework.Core;

namespace Makarov.Framework.Serialization;

public sealed class BoolSerializer : AbstractSerializer<bool>
{
    protected override string Format(bool value) => value ? bool.TrueString : bool.FalseString;

    /// <remarks><see cref="bool.Parse(string)"/> ignores case and surrounding white space.</remarks>
    protected override bool Parse(string text) => bool.Parse(text);
}
