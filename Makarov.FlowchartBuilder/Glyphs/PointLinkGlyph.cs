using System.Drawing;

namespace Makarov.FlowchartBuilder.Glyphs
{

    public class PointLinkGlyph : LinkGlyph
    {
        #region Properties

        public LinkPoint FirstPoint
        {
            get; set;
        }

        public LinkPoint SecondPoint
        {
            get; set;
        }
        #endregion

        #region Draw methods

        protected override void DrawGlyph(Graphics gfx)
        {
            gfx.DrawLine(
                Pens.Black,
                UnitsConverter.MMToPx(FirstPoint.XInMM),
                UnitsConverter.MMToPx(FirstPoint.YInMM),
                UnitsConverter.MMToPx(SecondPoint.XInMM),
                UnitsConverter.MMToPx(SecondPoint.YInMM));
        }
        #endregion
    }
}
