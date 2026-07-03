using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Makarov.FlowchartBuilder.API;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder
{

    public sealed class GlyphSnapshotsDiff : AbstractGlyphSnapshot
    {
        #region Exceptions
        public abstract class GlyphSnapshotsDiffException : FlowchartBuilderException
        {

            protected GlyphSnapshotsDiffException(string message)
                : base(message)
            { }
        }

        public sealed class IncompatibleGlyphsException : GlyphSnapshotsDiffException
        {

            public IncompatibleGlyphsException(string type1, string type2)
                : base(string.Format("Incompatible glyphs: '{0}' and '{1}'.",
                type1 ?? string.Empty, type2 ?? string.Empty))
            { }

            public IncompatibleGlyphsException(Type type1, Type type2)
                : this(type1.Name, type2.Name)
            { }
        }

        public sealed class DifferentIdsException : GlyphSnapshotsDiffException
        {

            public DifferentIdsException(string id1, string id2)
                : base(string.Format("Differect glyphs ids: '{0}' and '{1}'.",
                id1 ?? string.Empty, id2 ?? string.Empty))
            { }
        }

        public sealed class IncompatibleGlyphType : GlyphSnapshotsDiffException
        {

            public IncompatibleGlyphType(string requiredType, string existsType)
                : base(string.Format("Incompatible glyph type '{0}', but required '{1}'.",
                requiredType ?? string.Empty, existsType ?? string.Empty))
            { }

            public IncompatibleGlyphType(Type requiredType, Type existsType)
                : this(requiredType.Name, existsType.Name)
            { }
        }
        #endregion

        #region Private members

        private readonly Dictionary<string, KeyValuePair<string, string>> _props =
            new Dictionary<string, KeyValuePair<string, string>>();
        #endregion

        #region Constructors
        public GlyphSnapshotsDiff(GlyphSnapshot snapshot1, GlyphSnapshot snapshot2)
        {

            if (snapshot1.GlyphType != snapshot2.GlyphType)
                throw new IncompatibleGlyphsException(
                    snapshot1.GlyphType,
                    snapshot2.GlyphType);

            GlyphType = snapshot1.GlyphType;

            if (snapshot1.GlyphId != snapshot2.GlyphId)
                throw new DifferentIdsException(snapshot1.GlyphId, snapshot2.GlyphId);

            GlyphId = snapshot1.GlyphId;

            foreach (string prop in snapshot1.Properties)
            {
                string prop1 = snapshot1[prop];
                string prop2 = snapshot2[prop];

                if (prop1 != prop2)
                    _props.Add(prop, new KeyValuePair<string, string>(prop1, prop2));
            }
        }
        #endregion

        #region Properties

        public IEnumerable<string> Properties
        {
            get { return _props.Select(kvp => kvp.Key); }
        }

        public IEnumerable<KeyValuePair<string, KeyValuePair<string, string>>> Pairs
        {
            get { return _props; }
        }

        public IEnumerable<KeyValuePair<string, string>> NewPairs
        {
            get { return _props.Select(prop => new KeyValuePair<string, string>(prop.Key, prop.Value.Value)); }
        }

        public IEnumerable<KeyValuePair<string, string>> OldPairs
        {
            get { return _props.Select(prop => new KeyValuePair<string, string>(prop.Key, prop.Value.Key)); }
        }

        public int Count
        {
            get { return _props.Count; }
        }

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

        public IEnumerable<KeyValuePair<string, KeyValuePair<string, string>>> Apply(AbstractGlyph glyph)
        {
            return Apply(glyph, false);
        }

        public IEnumerable<KeyValuePair<string, KeyValuePair<string, string>>> ApplyOld(AbstractGlyph glyph)
        {
            return Apply(glyph, true);
        }

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

        #region Private methods

        public IEnumerable<KeyValuePair<string, KeyValuePair<string, string>>> Apply(AbstractGlyph glyph, bool oldValues)
        {
            Type type = glyph.GetType();
            string typeName = type.FullName;

            if (typeName != GlyphType)
                throw new IncompatibleGlyphType(GlyphType, typeName);

            type.GetProperty("Id").SetValue(glyph, GlyphId, null);

            foreach (KeyValuePair<string, KeyValuePair<string, string>> prop in _props)
            {
                PropertyInfo property = type.GetProperty(prop.Key);

                if (property == null)
                    yield return prop;
                else
                {
                    object[] attribs = property.GetCustomAttributes(typeof (ActivePropertyAttribute), true);

                    if (attribs.Length == 0)
                        yield return prop;
                    else
                        property.SetValue(glyph, prop.Value.Value, null);
                }
            }
        }
        #endregion
    }
}
