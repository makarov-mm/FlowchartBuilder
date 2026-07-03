using System;
using System.Reflection;

namespace Makarov.FlowchartBuilder.Glyphs
{

    public abstract partial class AbstractGlyph
    {

        public abstract class GlyphException : FlowchartBuilderException
        {

            protected GlyphException(string message)
                : base(message)
            { }
        }

        public sealed class TooManyActivePropertyAttributesException : GlyphException
        {

            public TooManyActivePropertyAttributesException(string glyphType, string propertyName)
                : base(string.Format("Too many ActivePropertyAttributes for property. " +
                "Glyph '{0}', property '{1}'.", glyphType ?? string.Empty, propertyName ?? string.Empty))
            { }

            public TooManyActivePropertyAttributesException(AbstractGlyph glyph, PropertyInfo property)
                : this(glyph.GetType().FullName, property.Name)
            { }
        }

        public sealed class UnknownNameException : GlyphException
        {

            public UnknownNameException(Type type)
                : base(string.Format(@"Unknown glyph '{0}' name.", type.Name))
            { }
        }

        public sealed class UnknownFamilyException : GlyphException
        {

            public UnknownFamilyException(Type type)
                : base(string.Format(@"Unknown glyph '{0}' family.", type.Name))
            { }
        }

        public sealed class UnknownOrderException : GlyphException
        {

            public UnknownOrderException(Type type)
                : base(string.Format(@"Unknown glyph '{0}' order.", type.Name))
            { }
        }
    }
}
