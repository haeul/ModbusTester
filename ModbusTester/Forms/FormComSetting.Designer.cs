namespace ModbusTester
{
    partial class FormComSetting
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
            chkSlaveMode = new CheckBox();
            cmbBaud = new ComboBox();
            label14 = new Label();
            cmbPort = new ComboBox();
            label13 = new Label();
            grpCom = new GroupBox();
            btnOpen = new Button();
            cmbStopBits = new ComboBox();
            label17 = new Label();
            cmbDataBits = new ComboBox();
            label16 = new Label();
            cmbParity = new ComboBox();
            label15 = new Label();
            grpCom.SuspendLayout();
            SuspendLayout();
            // 
            // chkSlaveMode
            // 
            chkSlaveMode.AutoSize = true;
            chkSlaveMode.Location = new Point(88, 13);
            chkSlaveMode.Name = "chkSlaveMode";
            chkSlaveMode.Size = new Size(89, 19);
            chkSlaveMode.TabIndex = 12;
            chkSlaveMode.Text = "Slave Mode";
            chkSlaveMode.UseVisualStyleBackColor = true;
            // 
            // cmbBaud
            // 
            cmbBaud.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBaud.FormattingEnabled = true;
            cmbBaud.Location = new Point(88, 68);
            cmbBaud.Name = "cmbBaud";
            cmbBaud.Size = new Size(150, 23);
            cmbBaud.TabIndex = 3;
            // 
            // label14
            // 
            label14.AutoSize = true;
            label14.Location = new Point(36, 71);
            label14.Name = "label14";
            label14.Size = new Size(34, 15);
            label14.TabIndex = 2;
            label14.Text = "Baud";
            // 
            // cmbPort
            // 
            cmbPort.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPort.FormattingEnabled = true;
            cmbPort.Location = new Point(88, 34);
            cmbPort.Name = "cmbPort";
            cmbPort.Size = new Size(150, 23);
            cmbPort.TabIndex = 1;
            // 
            // label13
            // 
            label13.AutoSize = true;
            label13.Location = new Point(41, 37);
            label13.Name = "label13";
            label13.Size = new Size(29, 15);
            label13.TabIndex = 0;
            label13.Text = "Port";
            // 예시: FormComSetting.Designer.cs 안 라벨 초기화 부분
            label13.DoubleClick += lblPort_DoubleClick;

            // 
            // grpCom
            // 
            grpCom.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpCom.Controls.Add(btnOpen);
            grpCom.Controls.Add(chkSlaveMode);
            grpCom.Controls.Add(cmbStopBits);
            grpCom.Controls.Add(label17);
            grpCom.Controls.Add(cmbDataBits);
            grpCom.Controls.Add(label16);
            grpCom.Controls.Add(cmbParity);
            grpCom.Controls.Add(label15);
            grpCom.Controls.Add(cmbBaud);
            grpCom.Controls.Add(label14);
            grpCom.Controls.Add(cmbPort);
            grpCom.Controls.Add(label13);
            grpCom.Location = new Point(12, 12);
            grpCom.Name = "grpCom";
            grpCom.Size = new Size(260, 260);
            grpCom.TabIndex = 3;
            grpCom.TabStop = false;
            grpCom.Text = "COM Setting";
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
            // cmbStopBits
            // 
            cmbStopBits.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStopBits.FormattingEnabled = true;
            cmbStopBits.Location = new Point(88, 170);
            cmbStopBits.Name = "cmbStopBits";
            cmbStopBits.Size = new Size(150, 23);
            cmbStopBits.TabIndex = 9;
            // 
            // label17
            // 
            label17.AutoSize = true;
            label17.Location = new Point(20, 173);
            label17.Name = "label17";
            label17.Size = new Size(51, 15);
            label17.TabIndex = 8;
            label17.Text = "StopBits";
            // 
            // cmbDataBits
            // 
            cmbDataBits.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDataBits.FormattingEnabled = true;
            cmbDataBits.Location = new Point(88, 136);
            cmbDataBits.Name = "cmbDataBits";
            cmbDataBits.Size = new Size(150, 23);
            cmbDataBits.TabIndex = 7;
            // 
            // label16
            // 
            label16.AutoSize = true;
            label16.Location = new Point(20, 139);
            label16.Name = "label16";
            label16.Size = new Size(51, 15);
            label16.TabIndex = 6;
            label16.Text = "DataBits";
            // 
            // cmbParity
            // 
            cmbParity.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbParity.FormattingEnabled = true;
            cmbParity.Location = new Point(88, 102);
            cmbParity.Name = "cmbParity";
            cmbParity.Size = new Size(150, 23);
            cmbParity.TabIndex = 5;
            // 
            // label15
            // 
            label15.AutoSize = true;
            label15.Location = new Point(33, 105);
            label15.Name = "label15";
            label15.Size = new Size(37, 15);
            label15.TabIndex = 4;
            label15.Text = "Parity";
            // 
            // FormComSetting
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(284, 281);
            Controls.Add(grpCom);
            Name = "FormComSetting";
            Text = "ModbusTester";
            grpCom.ResumeLayout(false);
            grpCom.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private CheckBox chkSlaveMode;
        private ComboBox cmbBaud;
        private Label label14;
        private ComboBox cmbPort;
        private Label label13;
        private GroupBox grpCom;
        private ComboBox cmbStopBits;
        private Label label17;
        private ComboBox cmbDataBits;
        private Label label16;
        private ComboBox cmbParity;
        private Label label15;
        private Button btnOpen;
    }
}