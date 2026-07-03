using System;
using System.Drawing;
using System.Windows.Forms;
using Makarov.FlowchartBuilder.Glyphs;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class LoadImageCommand : CommandedGlyphCommand
    {

        public override void Run()
        {

            if (!Core.Instance.IsDocumentsExists)
                throw new InvalidContextException(@"Document not exists.");

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "All Picture Files|*.bmp;*.png;*.jpg;*.jpeg|Bitmap Files|*bmp|PNG|*.png|JPEG|*.jpg;*.jpeg|All Files|*.*";
                dialog.Multiselect = false;

                if (dialog.ShowDialog(Core.Instance.MainWindow) == DialogResult.OK)
                {
                    var bmp = (Bitmap)Image.FromFile(dialog.FileName);
                    Core.Instance.CurrentDocument.DocumentSheet.DeselectAllGlyphs();
                    var glyph = (AbstractGlyph) Activator.CreateInstance(GlyphType, new object[] {bmp});
                    glyph.Selected = true;
                    Core.Instance.CurrentDocument.DocumentSheet.Add(glyph);
                }
            }
        }
    }
}
