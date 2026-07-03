using System.Collections.Generic;
using System.Linq;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder
{

    public sealed class GlyphSnapshot : AbstractGlyphSnapshot
    {
        #region Exceptions

        public sealed class PropertyAlreadyExistsException : GlyphSnapshotException
        {

            public PropertyAlreadyExistsException(string propertyName)
                : base(string.Format("Property '{0}' already exists.", propertyName ?? string.Empty))
            { }
        }
        #endregion

        #region Constructors

        public GlyphSnapshot(AbstractGlyph glyph)
        {

            GlyphId = glyph.Id;

            GlyphType = glyph.GetType().FullName;

            foreach (KeyValuePair<string, string> kvp in glyph.SerializeProperties())
                AddProperty(kvp.Key, kvp.Value);
        }
        #endregion

        #region Private members

        private readonly Dictionary<string, string> _props = new Dictionary<string, string>();
        #endregion

        #region Properties

        public IEnumerable<string> Properties
        {
            get { return _props.Select(kvp => kvp.Key); }
        }

        public IEnumerable<KeyValuePair<string, string>> Pairs
        {
            get { return _props; }
        }

        public int Count
        {
            get { return _props.Count; }
        }

        public string this[string propertyName]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(propertyName))
                    throw new EmptyPropertyName();

                if (!IsPropertyExists(propertyName))
                    throw new PropertyNotExistsException(propertyName);

                return _props[propertyName];
            }
        }
        #endregion

        #region Public methods

        public GlyphSnapshotsDiff Diff(GlyphSnapshot otherSnapshot)
        {
            return new GlyphSnapshotsDiff(this, otherSnapshot);
        }
        #endregion

        #region Operators

        public static GlyphSnapshotsDiff operator -(GlyphSnapshot glyphSnapshot1, GlyphSnapshot glyphSnapshot2)
        {
            return new GlyphSnapshotsDiff(glyphSnapshot1, glyphSnapshot2);
        }
        #endregion

        #region Public methods

        public bool IsPropertyExists(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new EmptyPropertyName();

            return _props.ContainsKey(propertyName);
        }

        public string GetProperty(string propertyName)
        {
            return this[propertyName];
        }
        #endregion

        #region Private methods

        private void AddProperty(string propertyName, string propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new EmptyPropertyName();

            if (_props.ContainsKey(propertyName))
                throw new PropertyAlreadyExistsException(propertyName);

            _props.Add(propertyName, propertyValue);
        }
        #endregion
    }
}
