using Makarov.Framework.Graphics;
using Makarov.FlowchartBuilder.Extensions;

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

        protected override void DrawGlyph(Canvas canvas)
        {
            canvas.DrawLine(
                new PointF(FirstPoint.XInMM.MMToPx().Scale(), FirstPoint.YInMM.MMToPx().Scale()),
                new PointF(SecondPoint.XInMM.MMToPx().Scale(), SecondPoint.YInMM.MMToPx().Scale()),
                new Stroke(Color.Black, 1f));
        }
        #endregion
    }
}
