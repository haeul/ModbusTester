namespace ModbusTester.Forms
{
    partial class FormStartMode
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            panel1 = new Panel();
            tableLayoutPanel1 = new TableLayoutPanel();
            btnNext = new Button();
            rbTcp = new RadioButton();
            rbRtu = new RadioButton();
            label1 = new Label();
            panel1.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // panel1
            // 
            panel1.BackColor = Color.White;
            panel1.Controls.Add(tableLayoutPanel1);
            panel1.Location = new Point(1, 1);
            panel1.Name = "panel1";
            panel1.Size = new Size(222, 210);
            panel1.TabIndex = 0;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayoutPanel1.Controls.Add(btnNext, 0, 3);
            tableLayoutPanel1.Controls.Add(rbTcp, 0, 2);
            tableLayoutPanel1.Controls.Add(rbRtu, 0, 1);
            tableLayoutPanel1.Controls.Add(label1, 0, 0);
            tableLayoutPanel1.Location = new Point(11, 11);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 4;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 25F));
            tableLayoutPanel1.Size = new Size(200, 185);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // btnNext
            // 
            btnNext.Font = new Font("Segoe UI Semibold", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnNext.Location = new Point(4, 142);
            btnNext.Name = "btnNext";
            btnNext.Size = new Size(192, 39);
            btnNext.TabIndex = 1;
            btnNext.Text = "Next";
            btnNext.UseVisualStyleBackColor = true;
            // 
            // rbTcp
            // 
            rbTcp.BackColor = SystemColors.Control;
            rbTcp.Font = new Font("Segoe UI Semibold", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            rbTcp.Location = new Point(4, 96);
            rbTcp.Name = "rbTcp";
            rbTcp.Size = new Size(192, 39);
            rbTcp.TabIndex = 1;
            rbTcp.TabStop = true;
            rbTcp.Text = "Modbus TCP";
            rbTcp.TextAlign = ContentAlignment.MiddleCenter;
            rbTcp.UseVisualStyleBackColor = false;
            // 
            // rbRtu
            // 
            rbRtu.BackColor = SystemColors.Control;
            rbRtu.Font = new Font("Segoe UI Semibold", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            rbRtu.Location = new Point(4, 50);
            rbRtu.Name = "rbRtu";
            rbRtu.Size = new Size(192, 39);
            rbRtu.TabIndex = 0;
            rbRtu.TabStop = true;
            rbRtu.Text = "Modbus RTU";
            rbRtu.TextAlign = ContentAlignment.MiddleCenter;
            rbRtu.UseVisualStyleBackColor = false;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.None;
            label1.BackColor = Color.LightGray;
            label1.Font = new Font("Segoe UI Semibold", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(4, 3);
            label1.Name = "label1";
            label1.Size = new Size(192, 40);
            label1.TabIndex = 2;
            label1.Text = "통신 모드 설정";
            label1.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // FormStartMode
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(226, 213);
            Controls.Add(panel1);
            Name = "FormStartMode";
            Text = "FormStartMode";
            panel1.ResumeLayout(false);
            tableLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panel1;
        private Button btnNext;
        private TableLayoutPanel tableLayoutPanel1;
        private RadioButton rbRtu;
        private RadioButton rbTcp;
        private Label label1;
    }
}