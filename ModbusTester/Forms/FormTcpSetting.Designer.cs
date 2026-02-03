namespace ModbusTester
{
    partial class FormTcpSetting
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
            labelHost = new Label();
            txtHost = new TextBox();
            labelPort = new Label();
            numPort = new NumericUpDown();
            labelUnitId = new Label();
            numUnitId = new NumericUpDown();
            grpTcp = new GroupBox();
            btnOpen = new Button();
            ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numUnitId).BeginInit();
            grpTcp.SuspendLayout();
            SuspendLayout();
            // 
            // labelHost
            // 
            labelHost.AutoSize = true;
            labelHost.Location = new Point(33, 37);
            labelHost.Name = "labelHost";
            labelHost.Size = new Size(32, 15);
            labelHost.TabIndex = 0;
            labelHost.Text = "Host";
            labelHost.DoubleClick += lblHost_DoubleClick;
            // 
            // txtHost
            // 
            txtHost.Location = new Point(88, 34);
            txtHost.Name = "txtHost";
            txtHost.Size = new Size(150, 23);
            txtHost.TabIndex = 1;
            // 
            // labelPort
            // 
            labelPort.AutoSize = true;
            labelPort.Location = new Point(36, 71);
            labelPort.Name = "labelPort";
            labelPort.Size = new Size(29, 15);
            labelPort.TabIndex = 2;
            labelPort.Text = "Port";
            // 
            // numPort
            // 
            numPort.Location = new Point(88, 68);
            numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPort.Name = "numPort";
            numPort.Size = new Size(150, 23);
            numPort.TabIndex = 3;
            numPort.Value = new decimal(new int[] { 13000, 0, 0, 0 });
            // 
            // labelUnitId
            // 
            labelUnitId.AutoSize = true;
            labelUnitId.Location = new Point(23, 105);
            labelUnitId.Name = "labelUnitId";
            labelUnitId.Size = new Size(45, 15);
            labelUnitId.TabIndex = 4;
            labelUnitId.Text = "Unit ID";
            // 
            // numUnitId
            // 
            numUnitId.Location = new Point(88, 102);
            numUnitId.Maximum = new decimal(new int[] { 247, 0, 0, 0 });
            numUnitId.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numUnitId.Name = "numUnitId";
            numUnitId.Size = new Size(150, 23);
            numUnitId.TabIndex = 5;
            numUnitId.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // grpTcp
            // 
            grpTcp.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpTcp.Controls.Add(btnOpen);
            grpTcp.Controls.Add(numUnitId);
            grpTcp.Controls.Add(labelUnitId);
            grpTcp.Controls.Add(numPort);
            grpTcp.Controls.Add(labelPort);
            grpTcp.Controls.Add(txtHost);
            grpTcp.Controls.Add(labelHost);
            grpTcp.Location = new Point(12, 12);
            grpTcp.Name = "grpTcp";
            grpTcp.Size = new Size(260, 260);
            grpTcp.TabIndex = 3;
            grpTcp.TabStop = false;
            grpTcp.Text = "TCP Setting";
            // 
            // btnOpen
            // 
            btnOpen.Location = new Point(20, 206);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(218, 39);
            btnOpen.TabIndex = 13;
            btnOpen.Text = "Open";
            btnOpen.UseVisualStyleBackColor = true;
            btnOpen.Click += btnOpen_Click;
            // 
            // FormTcpSetting
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(284, 281);
            Controls.Add(grpTcp);
            Name = "FormTcpSetting";
            ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
            ((System.ComponentModel.ISupportInitialize)numUnitId).EndInit();
            grpTcp.ResumeLayout(false);
            grpTcp.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label labelHost;
        private TextBox txtHost;
        private Label labelPort;
        private NumericUpDown numPort;
        private Label labelUnitId;
        private NumericUpDown numUnitId;
        private GroupBox grpTcp;
        private Button btnOpen;
    }
}
