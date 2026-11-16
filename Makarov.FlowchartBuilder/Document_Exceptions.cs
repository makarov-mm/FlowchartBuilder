namespace Makarov.FlowchartBuilder
{

    public sealed partial class Document
    {

        public abstract class DocumentException : FlowchartBuilderException
        {

            protected DocumentException(string message)
                : base(message)
            { }
        }

        public abstract class IncorrectSavedDocumentException : DocumentException
        {

            protected IncorrectSavedDocumentException(string reason)
                : base($@"Incorrect saved document: {reason ?? string.Empty}.")
            { }
        }

        public sealed class MainDocumentNodeNotFoundException : IncorrectSavedDocumentException
        {
            public MainDocumentNodeNotFoundException()
                : base(@"Main document node not found")
            { }
        }

        public sealed class SheetNodeNotFoundException : IncorrectSavedDocumentException
        {
            public SheetNodeNotFoundException()
                : base(@"Sheet node not found")
            { }
        }

        public sealed class GlyphTypeNotFoundException : IncorrectSavedDocumentException
        {

            public GlyphTypeNotFoundException(string glyphType)
                : base($@"Glyph type '{glyphType ?? string.Empty}' not found")
            { }
        }
    }
}
