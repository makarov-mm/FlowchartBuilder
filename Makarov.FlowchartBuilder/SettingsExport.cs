using System;
using System.Windows.Forms;
using System.Xml;

namespace Makarov.FlowchartBuilder
{

    public static class SettingsExport
    {

        public static void ShowSaveDialog()
        {
            var dlg = new SaveFileDialog
                          {
                              DefaultExt = Settings.Environment.AppSettingsExtension,
                              CheckFileExists = false,
                              CheckPathExists = true,
                              Filter = string.Format(@"(*.{0})|*.{0}", Settings.Environment.AppSettingsExtension),
                              FileName = "FlowchartBuilder." + Settings.Environment.AppSettingsExtension
                          };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Save(dlg.FileName);
            }
        }

        public static void ShowLoadDialog()
        {
            var dlg = new OpenFileDialog
                          {
                              DefaultExt = Settings.Environment.AppSettingsExtension,
                              CheckFileExists = true,
                              CheckPathExists = true,
                              Filter = string.Format("(*.{0})|*.{0}", Settings.Environment.AppSettingsExtension)
                          };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Load(dlg.FileName);
            }
        }

        public static void Save(string fileName)
        {
            var doc = new XmlDocument();

            var mainNode = (XmlElement)doc.CreateNode(
                XmlNodeType.Element,
                Settings.Xml.Tags.Settings,
                Settings.Xml.Namespaces.Base);

            doc.AppendChild(mainNode);

            doc.Save(fileName);
        }

        public static void Load(string fileName)
        {
            throw new NotImplementedException();
        }
    }
}
