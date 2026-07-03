using Makarov.Framework.Core;

namespace Makarov.FlowchartBuilder.Box
{

    public static partial class GlyphsHelper
    {

        public abstract class GlyphException : MakarovFrameworkException
        {

            protected GlyphException(string message)
                : base(message)
            { }
        }

        public sealed class TooManyActivePropertyAttributesException : GlyphException
        {

            public TooManyActivePropertyAttributesException(string glyphType, string propertyName)
                : base($"Too many ActivePropertyAttributes for property. Glyph '{glyphType ?? string.Empty}', property '{propertyName ?? string.Empty}'.")
            { }
        }
    }
}
