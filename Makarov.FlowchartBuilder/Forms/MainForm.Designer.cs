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
            this.sheetView = new Makarov.FlowchartBuilder.Forms.SheetView();
            this.SuspendLayout();

            this.documentsTabsBar.BackColor = System.Drawing.SystemColors.Control;
            this.documentsTabsBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.documentsTabsBar.Location = new System.Drawing.Point(0, 49);
            this.documentsTabsBar.MaximumSize = new System.Drawing.Size(10000, 24);
            this.documentsTabsBar.Name = "documentsTabsBar";
            this.documentsTabsBar.Size = new System.Drawing.Size(501, 24);
            this.documentsTabsBar.TabIndex = 5;

            this.sheetView.Location = new System.Drawing.Point(0, 73);
            this.sheetView.Name = "sheetView";
            this.sheetView.Size = new System.Drawing.Size(501, 251);
            this.sheetView.TabIndex = 6;
            this.sheetView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.sheetView_MouseDown);
            this.sheetView.MouseMove += new System.Windows.Forms.MouseEventHandler(this.sheetView_MouseMove);
            this.sheetView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.sheetView_MouseUp);
            this.sheetView.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.sheetView_MouseWheel);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(501, 346);
            this.Controls.Add(this.sheetView);
            this.Controls.Add(this.documentsTabsBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "FlowchartBuilder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.Controls.SetChildIndex(this.documentsTabsBar, 0);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Framework.Components.TabsPanel documentsTabsBar;
        private SheetView sheetView;

    }
}
