using System;
using System.Drawing;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class ZoomToWindowCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            var sheet = Core.Instance.CurrentDocument.DocumentSheet;
            var bounds = sheet.GetContentBounds();
            if (bounds.IsEmpty)
            {
                sheet.ScaleFactor = 1f;
                sheet.ViewOrigin = PointF.Empty;
            }
            else
            {
                var size = Core.Instance.MainWindow.InnerRectangle.Size;
                bounds.Inflate(16, 16);
                sheet.ScaleFactor = Math.Min(size.Width / bounds.Width, size.Height / bounds.Height);
                sheet.ViewOrigin = new PointF(
                    bounds.Left + (bounds.Width - size.Width / sheet.ScaleFactor) / 2,
                    bounds.Top + (bounds.Height - size.Height / sheet.ScaleFactor) / 2);
            }

            Core.Instance.Redraw();
        }
    }
}
