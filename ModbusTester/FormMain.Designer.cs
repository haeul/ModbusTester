namespace ModbusTester
{
    partial class FormMain : Form
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

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            grpTx = new GroupBox();
            btnSend = new Button();
            btnCalcCrc = new Button();
            txtCrc = new TextBox();
            label6 = new Label();
            txtDataCount = new TextBox();
            label5 = new Label();
            numCount = new NumericUpDown();
            label4 = new Label();
            txtStart = new TextBox();
            label3 = new Label();
            cmbFc = new ComboBox();
            label2 = new Label();
            numSlave = new NumericUpDown();
            label1 = new Label();
            gridTx = new DataGridView();
            colTxReg = new DataGridViewTextBoxColumn();
            colTxHex = new DataGridViewTextBoxColumn();
            colTxDec = new DataGridViewTextBoxColumn();
            grpRx = new GroupBox();
            btnCopyToTx = new Button();
            btnRxClear = new Button();
            txtRxCrc = new TextBox();
            label12 = new Label();
            txtRxDataCount = new TextBox();
            label11 = new Label();
            txtRxCount = new TextBox();
            label10 = new Label();
            txtRxStart = new TextBox();
            label9 = new Label();
            txtRxFc = new TextBox();
            label8 = new Label();
            txtRxSlave = new TextBox();
            label7 = new Label();
            gridRx = new DataGridView();
            colRxReg = new DataGridViewTextBoxColumn();
            colRxHex = new DataGridViewTextBoxColumn();
            colRxDec = new DataGridViewTextBoxColumn();
            grpCom = new GroupBox();
            btnClose = new Button();
            btnOpen = new Button();
            cmbStopBits = new ComboBox();
            label17 = new Label();
            cmbDataBits = new ComboBox();
            label16 = new Label();
            cmbParity = new ComboBox();
            label15 = new Label();
            cmbBaud = new ComboBox();
            label14 = new Label();
            cmbPort = new ComboBox();
            label13 = new Label();
            grpOpt = new GroupBox();
            btnPollStop = new Button();
            btnPollStart = new Button();
            numInterval = new NumericUpDown();
            label19 = new Label();
            cmbRecordEvery = new ComboBox();
            chkRecording = new CheckBox();
            label18 = new Label();
            grpLog = new GroupBox();
            btnSaveLog = new Button();
            btnLogClear = new Button();
            txtLog = new RichTextBox();
            pollTimer = new System.Windows.Forms.Timer(components);
            grpTx.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSlave).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridTx).BeginInit();
            grpRx.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridRx).BeginInit();
            grpCom.SuspendLayout();
            grpOpt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numInterval).BeginInit();
            grpLog.SuspendLayout();
            SuspendLayout();
            // 
            // grpTx
            // 
            grpTx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            grpTx.Controls.Add(btnSend);
            grpTx.Controls.Add(btnCalcCrc);
            grpTx.Controls.Add(txtCrc);
            grpTx.Controls.Add(label6);
            grpTx.Controls.Add(txtDataCount);
            grpTx.Controls.Add(label5);
            grpTx.Controls.Add(numCount);
            grpTx.Controls.Add(label4);
            grpTx.Controls.Add(txtStart);
            grpTx.Controls.Add(label3);
            grpTx.Controls.Add(cmbFc);
            grpTx.Controls.Add(label2);
            grpTx.Controls.Add(numSlave);
            grpTx.Controls.Add(label1);
            grpTx.Controls.Add(gridTx);
            grpTx.Location = new Point(12, 12);
            grpTx.Name = "grpTx";
            grpTx.Size = new Size(520, 420);
            grpTx.TabIndex = 0;
            grpTx.TabStop = false;
            grpTx.Text = "Transmit (Send)";
            // 
            // btnSend
            // 
            btnSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSend.Location = new Point(440, 64);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(70, 26);
            btnSend.TabIndex = 14;
            btnSend.Text = "SEND";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // btnCalcCrc
            // 
            btnCalcCrc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCalcCrc.Location = new Point(440, 30);
            btnCalcCrc.Name = "btnCalcCrc";
            btnCalcCrc.Size = new Size(70, 26);
            btnCalcCrc.TabIndex = 13;
            btnCalcCrc.Text = "Calc CRC";
            btnCalcCrc.UseVisualStyleBackColor = true;
            btnCalcCrc.Click += btnCalcCrc_Click;
            // 
            // txtCrc
            // 
            txtCrc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtCrc.Location = new Point(360, 66);
            txtCrc.Name = "txtCrc";
            txtCrc.ReadOnly = true;
            txtCrc.Size = new Size(74, 23);
            txtCrc.TabIndex = 12;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Location = new Point(327, 69);
            label6.Name = "label6";
            label6.Size = new Size(27, 15);
            label6.TabIndex = 11;
            label6.Text = "CRC";
            // 
            // txtDataCount
            // 
            txtDataCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtDataCount.Location = new Point(360, 32);
            txtDataCount.Name = "txtDataCount";
            txtDataCount.ReadOnly = true;
            txtDataCount.Size = new Size(74, 23);
            txtDataCount.TabIndex = 10;
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label5.AutoSize = true;
            label5.Location = new Point(290, 35);
            label5.Name = "label5";
            label5.Size = new Size(64, 15);
            label5.TabIndex = 9;
            label5.Text = "DataCount";
            // 
            // numCount
            // 
            numCount.Location = new Point(226, 66);
            numCount.Maximum = new decimal(new int[] { 125, 0, 0, 0 });
            numCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCount.Name = "numCount";
            numCount.Size = new Size(58, 23);
            numCount.TabIndex = 8;
            numCount.Value = new decimal(new int[] { 2, 0, 0, 0 });
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(184, 69);
            label4.Name = "label4";
            label4.Size = new Size(39, 15);
            label4.TabIndex = 7;
            label4.Text = "Count";
            // 
            // txtStart
            // 
            txtStart.CharacterCasing = CharacterCasing.Upper;
            txtStart.Location = new Point(226, 32);
            txtStart.MaxLength = 4;
            txtStart.Name = "txtStart";
            txtStart.PlaceholderText = "0000";
            txtStart.Size = new Size(58, 23);
            txtStart.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(184, 35);
            label3.Name = "label3";
            label3.Size = new Size(33, 15);
            label3.TabIndex = 5;
            label3.Text = "Start";
            // 
            // cmbFc
            // 
            cmbFc.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFc.FormattingEnabled = true;
            cmbFc.Items.AddRange(new object[] { "03h Read HR", "04h Read IR", "06h Write SR", "10h Write MR" });
            cmbFc.Location = new Point(66, 66);
            cmbFc.Name = "cmbFc";
            cmbFc.Size = new Size(108, 23);
            cmbFc.TabIndex = 4;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(11, 69);
            label2.Name = "label2";
            label2.Size = new Size(53, 15);
            label2.TabIndex = 3;
            label2.Text = "Function";
            // 
            // numSlave
            // 
            numSlave.Location = new Point(66, 32);
            numSlave.Maximum = new decimal(new int[] { 247, 0, 0, 0 });
            numSlave.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSlave.Name = "numSlave";
            numSlave.Size = new Size(60, 23);
            numSlave.TabIndex = 2;
            numSlave.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(11, 35);
            label1.Name = "label1";
            label1.Size = new Size(36, 15);
            label1.TabIndex = 1;
            label1.Text = "Slave";
            // 
            // gridTx
            // 
            gridTx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridTx.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridTx.Columns.AddRange(new DataGridViewColumn[] { colTxReg, colTxHex, colTxDec });
            gridTx.Location = new Point(11, 105);
            gridTx.Name = "gridTx";
            gridTx.RowHeadersVisible = false;
            gridTx.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridTx.Size = new Size(499, 301);
            gridTx.TabIndex = 15;
            // 
            // colTxReg
            // 
            colTxReg.HeaderText = "Register";
            colTxReg.Name = "colTxReg";
            colTxReg.ReadOnly = true;
            colTxReg.Width = 120;
            // 
            // colTxHex
            // 
            colTxHex.HeaderText = "HEX";
            colTxHex.Name = "colTxHex";
            colTxHex.Width = 120;
            // 
            // colTxDec
            // 
            colTxDec.HeaderText = "DEC";
            colTxDec.Name = "colTxDec";
            colTxDec.Width = 120;
            // 
            // grpRx
            // 
            grpRx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            grpRx.Controls.Add(btnCopyToTx);
            grpRx.Controls.Add(btnRxClear);
            grpRx.Controls.Add(txtRxCrc);
            grpRx.Controls.Add(label12);
            grpRx.Controls.Add(txtRxDataCount);
            grpRx.Controls.Add(label11);
            grpRx.Controls.Add(txtRxCount);
            grpRx.Controls.Add(label10);
            grpRx.Controls.Add(txtRxStart);
            grpRx.Controls.Add(label9);
            grpRx.Controls.Add(txtRxFc);
            grpRx.Controls.Add(label8);
            grpRx.Controls.Add(txtRxSlave);
            grpRx.Controls.Add(label7);
            grpRx.Controls.Add(gridRx);
            grpRx.Location = new Point(538, 12);
            grpRx.Name = "grpRx";
            grpRx.Size = new Size(380, 420);
            grpRx.TabIndex = 1;
            grpRx.TabStop = false;
            grpRx.Text = "Receive";
            // 
            // btnCopyToTx
            // 
            btnCopyToTx.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCopyToTx.Location = new Point(294, 64);
            btnCopyToTx.Name = "btnCopyToTx";
            btnCopyToTx.Size = new Size(80, 26);
            btnCopyToTx.TabIndex = 14;
            btnCopyToTx.Text = "Copy to TX";
            btnCopyToTx.UseVisualStyleBackColor = true;
            btnCopyToTx.Click += btnCopyToTx_Click;
            // 
            // btnRxClear
            // 
            btnRxClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRxClear.Location = new Point(294, 30);
            btnRxClear.Name = "btnRxClear";
            btnRxClear.Size = new Size(80, 26);
            btnRxClear.TabIndex = 13;
            btnRxClear.Text = "CLEAR";
            btnRxClear.UseVisualStyleBackColor = true;
            btnRxClear.Click += btnRxClear_Click;
            // 
            // txtRxCrc
            // 
            txtRxCrc.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtRxCrc.Location = new Point(220, 66);
            txtRxCrc.Name = "txtRxCrc";
            txtRxCrc.ReadOnly = true;
            txtRxCrc.Size = new Size(68, 23);
            txtRxCrc.TabIndex = 12;
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(188, 69);
            label12.Name = "label12";
            label12.Size = new Size(27, 15);
            label12.TabIndex = 11;
            label12.Text = "CRC";
            // 
            // txtRxDataCount
            // 
            txtRxDataCount.Location = new Point(220, 32);
            txtRxDataCount.Name = "txtRxDataCount";
            txtRxDataCount.ReadOnly = true;
            txtRxDataCount.Size = new Size(68, 23);
            txtRxDataCount.TabIndex = 10;
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(151, 35);
            label11.Name = "label11";
            label11.Size = new Size(64, 15);
            label11.TabIndex = 9;
            label11.Text = "DataCount";
            // 
            // txtRxCount
            // 
            txtRxCount.Location = new Point(77, 66);
            txtRxCount.Name = "txtRxCount";
            txtRxCount.ReadOnly = true;
            txtRxCount.Size = new Size(68, 23);
            txtRxCount.TabIndex = 8;
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(34, 69);
            label10.Name = "label10";
            label10.Size = new Size(39, 15);
            label10.TabIndex = 7;
            label10.Text = "Count";
            // 
            // txtRxStart
            // 
            txtRxStart.Location = new Point(77, 32);
            txtRxStart.Name = "txtRxStart";
            txtRxStart.ReadOnly = true;
            txtRxStart.Size = new Size(68, 23);
            txtRxStart.TabIndex = 6;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(34, 35);
            label9.Name = "label9";
            label9.Size = new Size(33, 15);
            label9.TabIndex = 5;
            label9.Text = "Start";
            // 
            // txtRxFc
            // 
            txtRxFc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtRxFc.Location = new Point(294, 96);
            txtRxFc.Name = "txtRxFc";
            txtRxFc.ReadOnly = true;
            txtRxFc.Size = new Size(80, 23);
            txtRxFc.TabIndex = 4;
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label8.AutoSize = true;
            label8.Location = new Point(294, 78);
            label8.Name = "label8";
            label8.Size = new Size(53, 15);
            label8.TabIndex = 3;
            label8.Text = "Function";
            // 
            // txtRxSlave
            // 
            txtRxSlave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            txtRxSlave.Location = new Point(294, 140);
            txtRxSlave.Name = "txtRxSlave";
            txtRxSlave.ReadOnly = true;
            txtRxSlave.Size = new Size(80, 23);
            txtRxSlave.TabIndex = 2;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label7.AutoSize = true;
            label7.Location = new Point(294, 122);
            label7.Name = "label7";
            label7.Size = new Size(36, 15);
            label7.TabIndex = 1;
            label7.Text = "Slave";
            // 
            // gridRx
            // 
            gridRx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridRx.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridRx.Columns.AddRange(new DataGridViewColumn[] { colRxReg, colRxHex, colRxDec });
            gridRx.Location = new Point(11, 172);
            gridRx.Name = "gridRx";
            gridRx.ReadOnly = true;
            gridRx.RowHeadersVisible = false;
            gridRx.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridRx.Size = new Size(363, 234);
            gridRx.TabIndex = 0;
            // 
            // colRxReg
            // 
            colRxReg.HeaderText = "Register";
            colRxReg.Name = "colRxReg";
            colRxReg.ReadOnly = true;
            colRxReg.Width = 120;
            // 
            // colRxHex
            // 
            colRxHex.HeaderText = "HEX";
            colRxHex.Name = "colRxHex";
            colRxHex.ReadOnly = true;
            colRxHex.Width = 120;
            // 
            // colRxDec
            // 
            colRxDec.HeaderText = "DEC";
            colRxDec.Name = "colRxDec";
            colRxDec.ReadOnly = true;
            colRxDec.Width = 120;
            // 
            // grpCom
            // 
            grpCom.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpCom.Controls.Add(btnClose);
            grpCom.Controls.Add(btnOpen);
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
            grpCom.Location = new Point(924, 12);
            grpCom.Name = "grpCom";
            grpCom.Size = new Size(260, 260);
            grpCom.TabIndex = 2;
            grpCom.TabStop = false;
            grpCom.Text = "COM Setting";
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(138, 215);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(100, 30);
            btnClose.TabIndex = 11;
            btnClose.Text = "CLOSE";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnOpen
            // 
            btnOpen.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnOpen.Location = new Point(20, 215);
            btnOpen.Name = "btnOpen";
            btnOpen.Size = new Size(100, 30);
            btnOpen.TabIndex = 10;
            btnOpen.Text = "OPEN";
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
            label17.Size = new Size(54, 15);
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
            label16.Size = new Size(56, 15);
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
            label15.Location = new Point(20, 105);
            label15.Name = "label15";
            label15.Size = new Size(38, 15);
            label15.TabIndex = 4;
            label15.Text = "Parity";
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
            label14.Location = new Point(20, 71);
            label14.Name = "label14";
            label14.Size = new Size(36, 15);
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
            label13.Location = new Point(20, 37);
            label13.Name = "label13";
            label13.Size = new Size(29, 15);
            label13.TabIndex = 0;
            label13.Text = "Port";
            // 
            // grpOpt
            // 
            grpOpt.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpOpt.Controls.Add(btnPollStop);
            grpOpt.Controls.Add(btnPollStart);
            grpOpt.Controls.Add(numInterval);
            grpOpt.Controls.Add(label19);
            grpOpt.Controls.Add(cmbRecordEvery);
            grpOpt.Controls.Add(chkRecording);
            grpOpt.Controls.Add(label18);
            grpOpt.Location = new Point(924, 278);
            grpOpt.Name = "grpOpt";
            grpOpt.Size = new Size(260, 154);
            grpOpt.TabIndex = 3;
            grpOpt.TabStop = false;
            grpOpt.Text = "Recording / Polling";
            // 
            // btnPollStop
            // 
            btnPollStop.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPollStop.Location = new Point(138, 110);
            btnPollStop.Name = "btnPollStop";
            btnPollStop.Size = new Size(100, 30);
            btnPollStop.TabIndex = 6;
            btnPollStop.Text = "STOP";
            btnPollStop.UseVisualStyleBackColor = true;
            btnPollStop.Click += btnPollStop_Click;
            // 
            // btnPollStart
            // 
            btnPollStart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPollStart.Location = new Point(20, 110);
            btnPollStart.Name = "btnPollStart";
            btnPollStart.Size = new Size(100, 30);
            btnPollStart.TabIndex = 5;
            btnPollStart.Text = "START";
            btnPollStart.UseVisualStyleBackColor = true;
            btnPollStart.Click += btnPollStart_Click;
            // 
            // numInterval
            // 
            numInterval.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            numInterval.Location = new Point(88, 73);
            numInterval.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numInterval.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numInterval.Name = "numInterval";
            numInterval.Size = new Size(150, 23);
            numInterval.TabIndex = 4;
            numInterval.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(20, 75);
            label19.Name = "label19";
            label19.Size = new Size(53, 15);
            label19.TabIndex = 3;
            label19.Text = "Interval";
            // 
            // cmbRecordEvery
            // 
            cmbRecordEvery.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRecordEvery.FormattingEnabled = true;
            cmbRecordEvery.Items.AddRange(new object[] { "1 sec", "5 sec", "10 sec", "30 sec", "60 sec" });
            cmbRecordEvery.Location = new Point(163, 34);
            cmbRecordEvery.Name = "cmbRecordEvery";
            cmbRecordEvery.Size = new Size(75, 23);
            cmbRecordEvery.TabIndex = 2;
            // 
            // chkRecording
            // 
            chkRecording.AutoSize = true;
            chkRecording.Location = new Point(88, 36);
            chkRecording.Name = "chkRecording";
            chkRecording.Size = new Size(78, 19);
            chkRecording.TabIndex = 1;
            chkRecording.Text = "Recording";
            chkRecording.UseVisualStyleBackColor = true;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(20, 37);
            label18.Name = "label18";
            label18.Size = new Size(62, 15);
            label18.TabIndex = 0;
            label18.Text = "Log every";
            // 
            // grpLog
            // 
            grpLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpLog.Controls.Add(btnSaveLog);
            grpLog.Controls.Add(btnLogClear);
            grpLog.Controls.Add(txtLog);
            grpLog.Location = new Point(12, 438);
            grpLog.Name = "grpLog";
            grpLog.Size = new Size(1172, 222);
            grpLog.TabIndex = 4;
            grpLog.TabStop = false;
            grpLog.Text = "Message (HEX Log)";
            // 
            // btnSaveLog
            // 
            btnSaveLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveLog.Location = new Point(1086, 22);
            btnSaveLog.Name = "btnSaveLog";
            btnSaveLog.Size = new Size(80, 26);
            btnSaveLog.TabIndex = 2;
            btnSaveLog.Text = "Save";
            btnSaveLog.UseVisualStyleBackColor = true;
            btnSaveLog.Click += btnSaveLog_Click;
            // 
            // btnLogClear
            // 
            btnLogClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogClear.Location = new Point(1000, 22);
            btnLogClear.Name = "btnLogClear";
            btnLogClear.Size = new Size(80, 26);
            btnLogClear.TabIndex = 1;
            btnLogClear.Text = "Clear";
            btnLogClear.UseVisualStyleBackColor = true;
            btnLogClear.Click += btnLogClear_Click;
            // 
            // txtLog
            // 
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.Font = new Font("Consolas", 10F, FontStyle.Regular, GraphicsUnit.Point);
            txtLog.Location = new Point(11, 54);
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.Size = new Size(1155, 160);
            txtLog.TabIndex = 0;
            txtLog.Text = "";
            txtLog.WordWrap = false;
            // 
            // pollTimer
            // 
            pollTimer.Tick += pollTimer_Tick;
            // 
            // FormMain
            // 
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1196, 672);
            Controls.Add(grpLog);
            Controls.Add(grpOpt);
            Controls.Add(grpCom);
            Controls.Add(grpRx);
            Controls.Add(grpTx);
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            MinimumSize = new Size(1212, 711);
            Name = "FormMain";
            Text = "Modbus Tester";
            Load += FormMain_Load;
            grpTx.ResumeLayout(false);
            grpTx.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSlave).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridTx).EndInit();
            grpRx.ResumeLayout(false);
            grpRx.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridRx).EndInit();
            grpCom.ResumeLayout(false);
            grpCom.PerformLayout();
            grpOpt.ResumeLayout(false);
            grpOpt.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numInterval).EndInit();
            grpLog.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private GroupBox grpTx;
        private Button btnSend;
        private Button btnCalcCrc;
        private TextBox txtCrc;
        private Label label6;
        private TextBox txtDataCount;
        private Label label5;
        private NumericUpDown numCount;
        private Label label4;
        private TextBox txtStart;
        private Label label3;
        private ComboBox cmbFc;
        private Label label2;
        private NumericUpDown numSlave;
        private Label label1;
        private DataGridView gridTx;
        private DataGridViewTextBoxColumn colTxReg;
        private DataGridViewTextBoxColumn colTxHex;
        private DataGridViewTextBoxColumn colTxDec;
        private GroupBox grpRx;
        private Button btnCopyToTx;
        private Button btnRxClear;
        private TextBox txtRxCrc;
        private Label label12;
        private TextBox txtRxDataCount;
        private Label label11;
        private TextBox txtRxCount;
        private Label label10;
        private TextBox txtRxStart;
        private Label label9;
        private TextBox txtRxFc;
        private Label label8;
        private TextBox txtRxSlave;
        private Label label7;
        private DataGridView gridRx;
        private DataGridViewTextBoxColumn colRxReg;
        private DataGridViewTextBoxColumn colRxHex;
        private DataGridViewTextBoxColumn colRxDec;
        private GroupBox grpCom;
        private Button btnClose;
        private Button btnOpen;
        private ComboBox cmbStopBits;
        private Label label17;
        private ComboBox cmbDataBits;
        private Label label16;
        private ComboBox cmbParity;
        private Label label15;
        private ComboBox cmbBaud;
        private Label label14;
        private ComboBox cmbPort;
        private Label label13;
        private GroupBox grpOpt;
        private Button btnPollStop;
        private Button btnPollStart;
        private NumericUpDown numInterval;
        private Label label19;
        private ComboBox cmbRecordEvery;
        private CheckBox chkRecording;
        private Label label18;
        private GroupBox grpLog;
        private Button btnSaveLog;
        private Button btnLogClear;
        private RichTextBox txtLog;
        private System.Windows.Forms.Timer pollTimer;
    }
}
