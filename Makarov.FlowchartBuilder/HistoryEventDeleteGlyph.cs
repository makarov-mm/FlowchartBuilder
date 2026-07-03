using System.Collections.Generic;
using System.Linq;

namespace Makarov.FlowchartBuilder
{

    public sealed class HistoryEventDeleteGlyph : HistoryEvent
    {
        #region Exceptions

        public sealed class PropertyAlreadyExistsException : HistoryEventException
        {

            public PropertyAlreadyExistsException(string propertyName)
                : base($"Property '{propertyName ?? string.Empty}' already exists.")
            { }
        }
        #endregion

        #region Private members

        private readonly Dictionary<string, string> _props = new Dictionary<string, string>();
        #endregion

        #region Properties

        public string GlyphType { get; private set; }
        #endregion

        #region Constructors

        public HistoryEventDeleteGlyph(string glyphType, string glyphId,
            IEnumerable<KeyValuePair<string, string>> properties)
            : base(glyphId)
        {
            GlyphType = glyphType;

            if (properties != null)
                foreach (KeyValuePair<string, string> pair in properties)
                    AddProperty(pair.Key, pair.Value);
        }

        public HistoryEventDeleteGlyph(GlyphSnapshot snapshot)
            : this(snapshot.GlyphId, snapshot.GlyphType, snapshot.Pairs)
        { }
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
