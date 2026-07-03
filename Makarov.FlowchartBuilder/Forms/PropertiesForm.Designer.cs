namespace Makarov.FlowchartBuilder.Forms
{
    partial class PropertiesForm
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
            this.gridView = new System.Windows.Forms.PropertyGrid();
            this.SuspendLayout();

            this.gridView.Location = new System.Drawing.Point(12, 12);
            this.gridView.Name = "gridView";
            this.gridView.Size = new System.Drawing.Size(149, 160);
            this.gridView.TabIndex = 1;
            this.gridView.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.gridView_PropertyValueChanged);

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(234, 389);
            this.Controls.Add(this.gridView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "FBProperties";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "FBProperties";
            this.Load += new System.EventHandler(this.FBProperties_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FBProperties_FormClosing);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid gridView;

    }
}
