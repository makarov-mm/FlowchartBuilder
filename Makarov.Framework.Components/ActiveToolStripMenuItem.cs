using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Makarov.Framework.Components
{
    public sealed class ActiveToolStripMenuItem : ToolStripMenuItem, IActiveControl
    {

        public ActiveToolStripMenuItem(string text, string command)
            : base(text)
        {
            Tag = command;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ActiveCaption
        {
            get => Text ?? string.Empty;
            set => Text = value ?? string.Empty;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ActiveBalloon
        {
            get => ToolTipText ?? string.Empty;
            set => ToolTipText = value ?? string.Empty;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Bitmap ActiveIcon
        {
            get => Image == null ? null : (Bitmap)Image;
            set => Image = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ActiveEnabled
        {
            get => Enabled;
            set => Enabled = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ActiveChecked
        {
            get => Checked;
            set => Checked = value;
        }
    }
}
