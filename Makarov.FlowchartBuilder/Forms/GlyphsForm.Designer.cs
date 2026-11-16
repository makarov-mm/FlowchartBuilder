using System.Windows.Forms;

namespace Makarov.FlowchartBuilder.Forms
{
    partial class GlyphsForm
    {

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lstView = new System.Windows.Forms.ListView();
            this.SuspendLayout();

            this.lstView.BackColor = System.Drawing.SystemColors.Control;
            this.lstView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstView.Location = new System.Drawing.Point(0, 0);
            this.lstView.MultiSelect = false;
            this.lstView.Name = "lstView";
            this.lstView.Size = new System.Drawing.Size(234, 417);
            this.lstView.TabIndex = 0;
            this.lstView.UseCompatibleStateImageBehavior = false;
            this.lstView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lstView_ItemSelectionChanged);
            this.lstView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lstView_MouseDoubleClick);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(234, 417);
            this.Controls.Add(this.lstView);
            this.Name = "GlyphsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Glyphs";
            this.Load += new System.EventHandler(this.GlyphsForm_Load);
            this.Shown += new System.EventHandler(this.GlyphsForm_Shown);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.ListView lstView;
    }
}
