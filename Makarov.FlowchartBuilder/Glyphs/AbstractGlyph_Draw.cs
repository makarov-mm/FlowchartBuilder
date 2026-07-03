using System.Drawing;
using System.Drawing.Text;
using Makarov.Framework.Graphics;

namespace Makarov.FlowchartBuilder.Glyphs
{

    public abstract partial class AbstractGlyph
    {

        public void Draw(Graphics gfx)
        {

            if (BeginRedraw != null)
                BeginRedraw(this, gfx);

            using (new TempSmoothingMode(gfx, SmoothMode))
            {
                bool rotated = false;

                if (this is BlockGlyph)
                {
                    var blockGlyph = (BlockGlyph)this;

                    if (blockGlyph.CanRotate && blockGlyph.Angle != 0)
                    {
                        rotated = true;
                        gfx.TranslateTransform(blockGlyph.ScaledX, blockGlyph.ScaledY);
                        gfx.RotateTransform(blockGlyph.Angle);
                        gfx.TranslateTransform(-blockGlyph.ScaledX, -blockGlyph.ScaledY);
                    }
                }

                if (Selected)
                    DrawSelectedGlyph(gfx);
                else
                    DrawGlyph(gfx);

                if (rotated)
                    gfx.ResetTransform();
            }

            using (new TempTextRenderingHint(gfx, Settings.Environment.Antialiasing
                                                      ? TextRenderingHint.ClearTypeGridFit
                                                      : TextRenderingHint.SystemDefault))
            {

                DrawText(gfx);
            }

            if (Selected)
            {
                using (new TempSmoothingMode(gfx, SmoothMode))
                {

                    DrawDots(gfx);
                }

                DrawHelpers(gfx);
            }
            else
            {

                DrawLinkPoints(gfx);
            }

            if (Fixed && this is BlockGlyph)
            {
                Bitmap lockImg = Core.Instance.Images["Lock"];
                const int offset = 2;

                var blockGlyph = (BlockGlyph) this;
                gfx.DrawImageUnscaled(
                    lockImg,
                    blockGlyph.ScaledX + (blockGlyph.ScaledWidth >> 1) - lockImg.Width - offset,
                    blockGlyph.ScaledY - (blockGlyph.ScaledWidth >> 1) + offset);
            }

            if (EndRedraw != null)
                EndRedraw(this, gfx);
        }

        protected virtual void DrawGlyph(Graphics gfx) { }

        protected virtual void DrawSelectedGlyph(Graphics gfx)
        {
            DrawGlyph(gfx);
        }

        protected virtual void DrawText(Graphics gfx) { }

        protected virtual void DrawDots(Graphics gfx) { }

        protected virtual void DrawLinkPoints(Graphics gfx) { }

        protected virtual void DrawHelpers(Graphics gfx) { }
    }
}
