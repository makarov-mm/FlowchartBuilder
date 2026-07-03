using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using Makarov.FlowchartBuilder.API.Attributes;
using Makarov.Framework.Core.Managers;

namespace Makarov.FlowchartBuilder.Glyphs
{

    public abstract partial class AbstractGlyph : ICloneable
    {
        #region Private members

        private bool _selected;

        private bool _fixed;
        #endregion

        #region Constructors
        protected AbstractGlyph()
        {
            Id = Guid.NewGuid().ToString();
        }
        #endregion

        #region Thumbnails

        protected void DrawThumbnailBorder(Graphics gfx)
        {
            gfx.DrawRectangle(Settings.Pens.ThumbnailBorder, 0, 0, ThumbnailWidth - 1, ThumbnailHeight - 1);
        }

        public virtual int ThumbnailWidth
        {
            get { return 48; }
        }

        public virtual int ThumbnailHeight
        {
            get { return 48; }
        }

        public virtual Bitmap Thumbnail
        {
            get
            {
                var bmp = new Bitmap(ThumbnailWidth, ThumbnailHeight);

                using (Graphics gfx = Graphics.FromImage(bmp))
                {
                    gfx.Clear(Color.White);

                    using (Brush brush = new LinearGradientBrush(
                        new Point(ThumbnailWidth >> 1, 0),
                        new Point(ThumbnailWidth >> 1, ThumbnailHeight),
                        Settings.Colors.ThumbnailGradientStart,
                        Settings.Colors.ThumbnailGradientEnd))
                    {
                        gfx.FillRectangle(brush, 0, 0, ThumbnailWidth, ThumbnailHeight);
                    }
                }

                return bmp;
            }
        }

        public string Id { get; private set; }
        #endregion

        #region Delegates and events
        public delegate void GlyphDelegate(AbstractGlyph glyph, Graphics gfx);

        public event GlyphDelegate BeginRedraw;

        public event GlyphDelegate EndRedraw;
        #endregion

        #region Public members

        public object Clone()
        {

            Type t = GetType();

            var tmp = Activator.CreateInstance(t);

            foreach (var prop in t.GetProperties(BindingFlags.Public))
            {

                if (prop.GetCustomAttributes(true).OfType<ActivePropertyAttribute>().Any())
                    prop.SetValue(tmp, prop.GetValue(this, null), null);
            }

            return tmp;
        }

        public object GetProxy()
        {
            return ProxyManager.CreateProxy<ActivePropertyAttribute>(this);
        }
        #endregion
    }
}
