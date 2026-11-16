namespace Makarov.FlowchartBuilder.Forms
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.documentsTabsBar = new Makarov.Framework.Components.TabsPanel();
            this.SuspendLayout();

            this.documentsTabsBar.BackColor = System.Drawing.SystemColors.Control;
            this.documentsTabsBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.documentsTabsBar.Location = new System.Drawing.Point(0, 49);
            this.documentsTabsBar.MaximumSize = new System.Drawing.Size(10000, 24);
            this.documentsTabsBar.Name = "documentsTabsBar";
            this.documentsTabsBar.Size = new System.Drawing.Size(501, 24);
            this.documentsTabsBar.TabIndex = 5;

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 346);
            this.Controls.Add(this.documentsTabsBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "FBMainWindow";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FBMainWindow_FormClosing);
            this.Load += new System.EventHandler(this.FBMainWindow_Load);
            this.Shown += new System.EventHandler(this.FBMainWindow_Shown);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.FBMainWindow_Paint);
            this.Enter += new System.EventHandler(this.FBMainWindow_Enter);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FBMainWindow_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.FBMainWindow_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.FBMainWindow_MouseUp);
            this.Move += new System.EventHandler(this.FBMainWindow_Move);
            this.Resize += new System.EventHandler(this.FBMainWindow_Resize);
            this.Controls.SetChildIndex(this.documentsTabsBar, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Framework.Components.TabsPanel documentsTabsBar;

    }
}
