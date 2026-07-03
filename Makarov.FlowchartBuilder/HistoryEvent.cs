namespace Makarov.FlowchartBuilder
{

    public abstract class HistoryEvent
    {
        #region Exceptions

        public abstract class HistoryEventException : FlowchartBuilderException
        {

            protected HistoryEventException(string message)
                : base(message)
            { }
        }

        public sealed class EmptyGlyphIdException : HistoryEventException
        {
            public EmptyGlyphIdException()
                : base(@"Empty glyph Id.")
            { }
        }

        public sealed class EmptyPropertyName : HistoryEventException
        {
            public EmptyPropertyName()
                : base(@"Empty property name.")
            { }
        }

        public sealed class PropertyNotExistsException : HistoryEventException
        {

            public PropertyNotExistsException(string propertyName)
                : base($"Property '{propertyName ?? string.Empty}' not found.")
            { }
        }
        #endregion

        #region Properties

        public string GlyphId { get; protected set; }
        #endregion

        #region Constructors

        protected HistoryEvent(string glyphId)
        {
            if (string.IsNullOrWhiteSpace(glyphId))
                throw new EmptyGlyphIdException();

            GlyphId = glyphId;
        }
        #endregion
    }
}
