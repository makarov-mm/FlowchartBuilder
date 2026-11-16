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

            if (Core.Instance.ActiveSheet is not { } sheet || GlyphType is not { } glyphType)
                return;

            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = Core.Instance.CurrentTranslator["ImageFiles"] + "|*.bmp;*.png;*.jpg;*.jpeg";
                dialog.Multiselect = false;

                if (dialog.ShowDialog(Core.Instance.MainWindow) == DialogResult.OK)
                {
                    var bmp = (Bitmap)Image.FromFile(dialog.FileName);
                    sheet.DeselectAllGlyphs();
                    var glyph = (AbstractGlyph) Activator.CreateInstance(glyphType, new object[] {bmp})!;
                    glyph.Selected = true;
                    sheet.Add(glyph);
                }
            }
        }
    }
}
