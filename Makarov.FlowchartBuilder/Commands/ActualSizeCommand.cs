using Makarov.FlowchartBuilder.Extensions;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class ActualSizeCommand : Command
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            var sizedSheet = (ISize)Core.Instance.CurrentDocument.DocumentSheet;
            float h = sizedSheet.Width.MMToPx() /
                      (Core.Instance.MainWindow.InnerRectangle.Width * 0.9f);
            float v = sizedSheet.Height.MMToPx() /
                      (Core.Instance.MainWindow.InnerRectangle.Height * 0.9f);

            Core.Instance.CurrentDocument.DocumentSheet.ScaleFactor = h > v ? 1 / h : 1 / v;

            Core.Instance.Redraw();
        }
    }
}
