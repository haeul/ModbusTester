namespace ModbusTester
{
    partial class FormGridZoom
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.DataGridView gridZoom;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.gridZoom = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.gridZoom)).BeginInit();
            this.SuspendLayout();
            // 
            // gridZoom
            // 
            this.gridZoom.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridZoom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridZoom.Location = new System.Drawing.Point(0, 0);
            this.gridZoom.Name = "gridZoom";
            this.gridZoom.RowTemplate.Height = 23;
            this.gridZoom.Size = new System.Drawing.Size(800, 450);
            this.gridZoom.TabIndex = 0;
            // 
            // FormGridZoom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.gridZoom);
            this.Name = "FormGridZoom";
            this.Text = "Grid Zoom";
            ((System.ComponentModel.ISupportInitialize)(this.gridZoom)).EndInit();
            this.ResumeLayout(false);
        }
    }
}
