using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using BaseSettings = Makarov.Framework.Core.Settings;

namespace Makarov.FlowchartBuilder.Commands
{

    public sealed class ExportImageCommand : Command
    {

        public static string _oldFileName = "Flowchart.png";

        public override void Run()
        {
            if (Settings.Environment.Registered)
            {

                if (!Core.Instance.IsDocumentsExists)
                    throw new Core.CurrentDocumentNotExistsException();

                using (var dialog = new SaveFileDialog())
                {
                    dialog.Filter = @"*.png|*.png|*.bmp|*.bmp";

                    if (string.IsNullOrEmpty(dialog.FileName))
                        dialog.FileName = _oldFileName;

                    DialogResult result = dialog.ShowDialog(Core.Instance.MainWindow);
                    if (result == DialogResult.OK || result == DialogResult.Yes)
                    {
                        string fileName = dialog.FileName;
                        string ext = Path.GetExtension(fileName).ToLower();
                        ImageFormat format = null;

                        if (!string.IsNullOrEmpty(ext))
                        {
                            switch (ext)
                            {
                                case ".bmp":
                                    format = ImageFormat.Bmp;
                                    break;

                                case ".png":
                                    format = ImageFormat.Png;
                                    break;
                            }
                        }

                        if (format != null)
                        {
                            Core.Instance.CurrentDocument.DocumentSheet.DeselectAllGlyphs();
                            Core.Instance.Redraw();

                            using (Bitmap bmp = Core.Instance.CurrentDocument.DocumentSheet.DrawUnscaled())
                                bmp.Save(fileName, format);

                            _oldFileName = dialog.FileName;
                        }
                        else
                            MessageBox.Show(Core.Instance.CurrentTranslator["Msg_InvalidImageExtension"],
                                            Core.Instance.CurrentTranslator["Msg_Error"],
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show(Core.Instance.CurrentTranslator["Msg_FunctionDisabledInNonRegisteredProgram"],
                                Core.Instance.CurrentTranslator["Msg_Warning"],
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
            }
        }
    }
}
