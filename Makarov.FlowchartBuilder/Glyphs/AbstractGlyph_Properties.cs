using System;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using Makarov.FlowchartBuilder.API.Attributes;

namespace Makarov.FlowchartBuilder.Glyphs
{

    public abstract partial class AbstractGlyph
    {

        public bool Selected
        {
            get { return _selected; }

            set
            {
                if (value && _fixed)
                    throw new InvalidContextException();

                _selected = value;
            }
        }

        public bool Fixed
        {
            get { return _fixed; }

            set
            {
                _fixed = value;

                if (value)
                    Selected = false;
            }
        }

        [Description(@"Caption.")]
        [Category(@"Layout")]
        [DisplayName(@"Caption")]
        [ActiveProperty]
        public string Caption
        {
            get;
            set;
        }

        public bool OrderExists
        {
            get
            {
                return GetType().GetCustomAttributes(typeof(GlyphOrderAttribute), true).Length > 0;
            }
        }

        public int Order
        {
            get
            {
                Type type = GetType();
                object[] attribs = type.GetCustomAttributes(typeof(GlyphOrderAttribute), true);

                if (attribs.Length == 0)
                {
                    if (Settings.Environment.IsDebug)
                        throw new UnknownOrderException(type);
                }

                return ((GlyphOrderAttribute)attribs[attribs.Length - 1]).Order;
            }
        }

        public string Name
        {
            get
            {
                Type type = GetType();
                object[] attribs = type.GetCustomAttributes(typeof (GlyphNameAttribute), true);

                if (attribs.Length == 0)
                {
                    if (Settings.Environment.IsDebug)
                        throw new UnknownNameException(type);

                    return Core.Instance.CurrentTranslator["UndefinedGlyphName"];
                }

                return ((GlyphNameAttribute)attribs[attribs.Length - 1]).Name;
            }
        }

        public string Family
        {
            get
            {
                Type type = GetType();
                object[] attribs = type.GetCustomAttributes(typeof(GlyphFamilyAttribute), true);

                if (attribs.Length == 0)
                {
                    if (Settings.Environment.IsDebug)
                        throw new UnknownFamilyException(type);

                    return Core.Instance.CurrentTranslator["UndefinedGlyphFamily"];
                }

                return ((GlyphFamilyAttribute)attribs[attribs.Length - 1]).Family;
            }
        }

        protected static SmoothingMode SmoothMode
        {
            get
            {
                return Settings.Environment.Antialiasing
                           ? SmoothingMode.HighQuality
                           : SmoothingMode.HighSpeed;
            }
        }
    }
}
