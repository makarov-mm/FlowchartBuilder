using System.Collections.Generic;
using System.Linq;

namespace Makarov.FlowchartBuilder
{

    public sealed class HistoryEventChangeGlyph : HistoryEvent
    {
        #region Private members

        private readonly Dictionary<string, KeyValuePair<string, string>> _props =
            new Dictionary<string, KeyValuePair<string, string>>();
        #endregion

        #region Constructors

        public HistoryEventChangeGlyph(string glyphId,
            IEnumerable<KeyValuePair<string, KeyValuePair<string, string>>> properties)
            : base(glyphId)
        {
            if (properties != null)
                foreach (KeyValuePair<string, KeyValuePair<string, string>> prop in properties)
                    _props.Add(prop.Key, prop.Value);
        }

        public HistoryEventChangeGlyph(GlyphSnapshotsDiff diff)
            : this(diff.GlyphId, diff.Pairs)
        { }
        #endregion

        #region Properties

        public IEnumerable<string> Properties => _props.Select(kvp => kvp.Key);

        public IEnumerable<KeyValuePair<string, string>> NewPairs => _props.Select(prop => new KeyValuePair<string, string>(prop.Key, prop.Value.Value));

        public IEnumerable<KeyValuePair<string, string>> OldPairs => _props.Select(prop => new KeyValuePair<string, string>(prop.Key, prop.Value.Key));

        public int Count => _props.Count;

        public KeyValuePair<string, string> this[string propertyName]
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

        public KeyValuePair<string, string> GetProperty(string propertyName)
        {
            return this[propertyName];
        }
        #endregion
    }
}
