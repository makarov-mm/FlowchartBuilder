using Makarov.FlowchartBuilder.Extensions;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class ZoomToWindowCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            var sizedSheet = (ISize)Core.Instance.CurrentDocument.DocumentSheet;
            float h = sizedSheet.Width.MMToPx() /
                      (float) Core.Instance.MainWindow.InnerRectangle.Width;
            float v = sizedSheet.Height.MMToPx() /
                      (float) Core.Instance.MainWindow.InnerRectangle.Height;

            Core.Instance.CurrentDocument.DocumentSheet.ScaleFactor = h > v ? 1 / h : 1 / v;

            Core.Instance.Redraw();
        }
    }
}
