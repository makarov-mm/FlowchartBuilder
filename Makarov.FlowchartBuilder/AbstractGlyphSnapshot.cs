using System;

namespace Makarov.FlowchartBuilder
{

    public abstract class AbstractGlyphSnapshot
    {
        #region Exceptions

        public abstract class GlyphSnapshotException : FlowchartBuilderException
        {

            protected GlyphSnapshotException(string message)
                : base(message)
            { }
        }

        public sealed class EmptyPropertyName : GlyphSnapshotException
        {
            public EmptyPropertyName()
                : base(@"Empty property name.")
            { }
        }

        public sealed class PropertyNotExistsException : GlyphSnapshotException
        {

            public PropertyNotExistsException(string propertyName)
                : base(string.Format("Property '{0}' not found.", propertyName ?? string.Empty))
            { }
        }
        #endregion

        #region Properties

        public string GlyphId { get; protected set; }

        public string GlyphType { get; protected set; }
        #endregion

        #region Public methods

        Type GetGlyphType()
        {
            return Type.GetType(GlyphType);
        }

        public bool IsGlyphTypeExists()
        {
            return GetGlyphType() != null;
        }
        #endregion
    }
}
