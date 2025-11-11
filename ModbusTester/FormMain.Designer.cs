using System;
using System.Windows.Forms;

namespace ModbusTester
{
    partial class FormMain : Form
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            grpTx = new GroupBox();
            txtCrc = new TextBox();
            txtDataCount = new TextBox();
            btnSend = new Button();
            btnCalcCrc = new Button();
            label6 = new Label();
            label5 = new Label();
            numStartRegister = new NumericUpDown();
            numCount = new NumericUpDown();
            label4 = new Label();
            label3 = new Label();
            cmbFunctionCode = new ComboBox();
            label2 = new Label();
            numSlave = new NumericUpDown();
            label1 = new Label();
            gridTx = new DataGridView();
            colTxReg = new DataGridViewTextBoxColumn();
            colTxHex = new DataGridViewTextBoxColumn();
            colTxDec = new DataGridViewTextBoxColumn();
            grpRx = new GroupBox();
            btnQuickView = new Button();
            txtRxDataCount = new TextBox();
            txtRxCrc = new TextBox();
            btnCopyToTx = new Button();
            btnRxClear = new Button();
            label7 = new Label();
            label8 = new Label();
            txtRxFc = new TextBox();
            txtRxStart = new TextBox();
            txtRxCount = new TextBox();
            txtRxSlave = new TextBox();
            label9 = new Label();
            label10 = new Label();
            label11 = new Label();
            label12 = new Label();
            gridRx = new DataGridView();
            colRxReg = new DataGridViewTextBoxColumn();
            colRxHex = new DataGridViewTextBoxColumn();
            colRxDec = new DataGridViewTextBoxColumn();
            grpCom = new GroupBox();
            chkSlaveMode = new CheckBox();
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
            ((System.ComponentModel.ISupportInitialize)numStartRegister).BeginInit();
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
            grpTx.Controls.Add(txtCrc);
            grpTx.Controls.Add(txtDataCount);
            grpTx.Controls.Add(btnSend);
            grpTx.Controls.Add(btnCalcCrc);
            grpTx.Controls.Add(label6);
            grpTx.Controls.Add(label5);
            grpTx.Controls.Add(numStartRegister);
            grpTx.Controls.Add(numCount);
            grpTx.Controls.Add(label4);
            grpTx.Controls.Add(label3);
            grpTx.Controls.Add(cmbFunctionCode);
            grpTx.Controls.Add(label2);
            grpTx.Controls.Add(numSlave);
            grpTx.Controls.Add(label1);
            grpTx.Controls.Add(gridTx);
            grpTx.Location = new Point(12, 12);
            grpTx.Name = "grpTx";
            grpTx.Size = new Size(448, 420);
            grpTx.TabIndex = 0;
            grpTx.TabStop = false;
            grpTx.Text = "Transmit (Send)";
            // 
            // txtCrc
            // 
            txtCrc.Location = new Point(104, 169);
            txtCrc.Name = "txtCrc";
            txtCrc.Size = new Size(108, 23);
            txtCrc.TabIndex = 49;
            // 
            // txtDataCount
            // 
            txtDataCount.Location = new Point(104, 140);
            txtDataCount.Name = "txtDataCount";
            txtDataCount.Size = new Size(108, 23);
            txtDataCount.TabIndex = 50;
            // 
            // btnSend
            // 
            btnSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSend.Location = new Point(115, 203);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(95, 44);
            btnSend.TabIndex = 28;
            btnSend.Text = "SEND";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // btnCalcCrc
            // 
            btnCalcCrc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCalcCrc.Location = new Point(11, 203);
            btnCalcCrc.Name = "btnCalcCrc";
            btnCalcCrc.Size = new Size(95, 44);
            btnCalcCrc.TabIndex = 27;
            btnCalcCrc.Text = "Calc CRC";
            btnCalcCrc.UseVisualStyleBackColor = true;
            btnCalcCrc.Click += btnCalcCrc_Click;
            // 
            // label6
            // 
            label6.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label6.AutoSize = true;
            label6.Location = new Point(66, 172);
            label6.Name = "label6";
            label6.Size = new Size(30, 15);
            label6.TabIndex = 25;
            label6.Text = "CRC";
            // 
            // label5
            // 
            label5.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label5.AutoSize = true;
            label5.Location = new Point(32, 144);
            label5.Name = "label5";
            label5.Size = new Size(64, 15);
            label5.TabIndex = 23;
            label5.Text = "DataCount";
            // 
            // numStartRegister
            // 
            numStartRegister.Location = new Point(104, 84);
            numStartRegister.Name = "numStartRegister";
            numStartRegister.Size = new Size(108, 23);
            numStartRegister.TabIndex = 16;
            // 
            // numCount
            // 
            numCount.Location = new Point(104, 113);
            numCount.Maximum = new decimal(new int[] { 125, 0, 0, 0 });
            numCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCount.Name = "numCount";
            numCount.Size = new Size(108, 23);
            numCount.TabIndex = 8;
            numCount.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numCount.ValueChanged += numCount_ValueChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(11, 115);
            label4.Name = "label4";
            label4.Size = new Size(85, 15);
            label4.TabIndex = 7;
            label4.Text = "Register Count";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(20, 87);
            label3.Name = "label3";
            label3.Size = new Size(76, 15);
            label3.TabIndex = 5;
            label3.Text = "Start Register";
            // 
            // cmbFunctionCode
            // 
            cmbFunctionCode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFunctionCode.FormattingEnabled = true;
            cmbFunctionCode.Items.AddRange(new object[] { "03h Read HR", "04h Read IR", "06h Write SR", "10h Write MR" });
            cmbFunctionCode.Location = new Point(104, 57);
            cmbFunctionCode.Name = "cmbFunctionCode";
            cmbFunctionCode.Size = new Size(108, 23);
            cmbFunctionCode.TabIndex = 4;
            cmbFunctionCode.SelectedIndexChanged += cmbFunctionCode_SelectedIndexChanged;
            cmbFunctionCode.TextChanged += cmbFunctionCode_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(11, 59);
            label2.Name = "label2";
            label2.Size = new Size(85, 15);
            label2.TabIndex = 3;
            label2.Text = "Function Code";
            // 
            // numSlave
            // 
            numSlave.Location = new Point(104, 30);
            numSlave.Maximum = new decimal(new int[] { 247, 0, 0, 0 });
            numSlave.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSlave.Name = "numSlave";
            numSlave.Size = new Size(108, 23);
            numSlave.TabIndex = 2;
            numSlave.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(17, 30);
            label1.Name = "label1";
            label1.Size = new Size(79, 15);
            label1.TabIndex = 1;
            label1.Text = "Slave Address";
            // 
            // gridTx
            // 
            gridTx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridTx.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridTx.Columns.AddRange(new DataGridViewColumn[] { colTxReg, colTxHex, colTxDec });
            gridTx.Location = new Point(229, 30);
            gridTx.Name = "gridTx";
            gridTx.RowHeadersVisible = false;
            gridTx.RowHeadersWidth = 62;
            gridTx.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridTx.Size = new Size(200, 375);
            gridTx.TabIndex = 15;
            gridTx.CellEndEdit += HexAutoFormat_OnEndEdit;
            // 
            // colTxReg
            // 
            colTxReg.HeaderText = "Register";
            colTxReg.MinimumWidth = 8;
            colTxReg.Name = "colTxReg";
            colTxReg.ReadOnly = true;
            colTxReg.Width = 120;
            // 
            // colTxHex
            // 
            colTxHex.HeaderText = "HEX";
            colTxHex.MinimumWidth = 8;
            colTxHex.Name = "colTxHex";
            colTxHex.Width = 120;
            // 
            // colTxDec
            // 
            colTxDec.HeaderText = "DEC";
            colTxDec.MinimumWidth = 8;
            colTxDec.Name = "colTxDec";
            colTxDec.Width = 120;
            // 
            // grpRx
            // 
            grpRx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            grpRx.Controls.Add(btnQuickView);
            grpRx.Controls.Add(txtRxDataCount);
            grpRx.Controls.Add(txtRxCrc);
            grpRx.Controls.Add(btnCopyToTx);
            grpRx.Controls.Add(btnRxClear);
            grpRx.Controls.Add(label7);
            grpRx.Controls.Add(label8);
            grpRx.Controls.Add(txtRxFc);
            grpRx.Controls.Add(txtRxStart);
            grpRx.Controls.Add(txtRxCount);
            grpRx.Controls.Add(txtRxSlave);
            grpRx.Controls.Add(label9);
            grpRx.Controls.Add(label10);
            grpRx.Controls.Add(label11);
            grpRx.Controls.Add(label12);
            grpRx.Controls.Add(gridRx);
            grpRx.Location = new Point(468, 12);
            grpRx.Name = "grpRx";
            grpRx.Size = new Size(447, 420);
            grpRx.TabIndex = 1;
            grpRx.TabStop = false;
            grpRx.Text = "Receive";
            // 
            // btnQuickView
            // 
            btnQuickView.Location = new Point(16, 252);
            btnQuickView.Name = "btnQuickView";
            btnQuickView.Size = new Size(95, 44);
            btnQuickView.TabIndex = 49;
            btnQuickView.Text = "QuickView";
            btnQuickView.UseVisualStyleBackColor = true;
            btnQuickView.Click += btnQuickView_Click;
            // 
            // txtRxDataCount
            // 
            txtRxDataCount.Location = new Point(107, 143);
            txtRxDataCount.Name = "txtRxDataCount";
            txtRxDataCount.ReadOnly = true;
            txtRxDataCount.Size = new Size(108, 23);
            txtRxDataCount.TabIndex = 48;
            // 
            // txtRxCrc
            // 
            txtRxCrc.Location = new Point(107, 170);
            txtRxCrc.Name = "txtRxCrc";
            txtRxCrc.ReadOnly = true;
            txtRxCrc.Size = new Size(108, 23);
            txtRxCrc.TabIndex = 47;
            // 
            // btnCopyToTx
            // 
            btnCopyToTx.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCopyToTx.Location = new Point(120, 202);
            btnCopyToTx.Name = "btnCopyToTx";
            btnCopyToTx.Size = new Size(95, 44);
            btnCopyToTx.TabIndex = 46;
            btnCopyToTx.Text = "Copy to TX";
            btnCopyToTx.UseVisualStyleBackColor = true;
            btnCopyToTx.Click += btnCopyToTx_Click;
            // 
            // btnRxClear
            // 
            btnRxClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRxClear.Location = new Point(16, 202);
            btnRxClear.Name = "btnRxClear";
            btnRxClear.Size = new Size(95, 44);
            btnRxClear.TabIndex = 45;
            btnRxClear.Text = "CLEAR";
            btnRxClear.UseVisualStyleBackColor = true;
            btnRxClear.Click += btnRxClear_Click;
            // 
            // label7
            // 
            label7.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label7.AutoSize = true;
            label7.Location = new Point(71, 171);
            label7.Name = "label7";
            label7.Size = new Size(30, 15);
            label7.TabIndex = 44;
            label7.Text = "CRC";
            // 
            // label8
            // 
            label8.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label8.AutoSize = true;
            label8.Location = new Point(37, 143);
            label8.Name = "label8";
            label8.Size = new Size(64, 15);
            label8.TabIndex = 43;
            label8.Text = "DataCount";
            // 
            // txtRxFc
            // 
            txtRxFc.Location = new Point(107, 55);
            txtRxFc.Name = "txtRxFc";
            txtRxFc.ReadOnly = true;
            txtRxFc.Size = new Size(108, 23);
            txtRxFc.TabIndex = 42;
            // 
            // txtRxStart
            // 
            txtRxStart.Location = new Point(107, 84);
            txtRxStart.Name = "txtRxStart";
            txtRxStart.ReadOnly = true;
            txtRxStart.Size = new Size(108, 23);
            txtRxStart.TabIndex = 41;
            // 
            // txtRxCount
            // 
            txtRxCount.Location = new Point(107, 112);
            txtRxCount.Name = "txtRxCount";
            txtRxCount.ReadOnly = true;
            txtRxCount.Size = new Size(108, 23);
            txtRxCount.TabIndex = 40;
            // 
            // txtRxSlave
            // 
            txtRxSlave.Location = new Point(107, 27);
            txtRxSlave.Name = "txtRxSlave";
            txtRxSlave.ReadOnly = true;
            txtRxSlave.Size = new Size(108, 23);
            txtRxSlave.TabIndex = 37;
            // 
            // label9
            // 
            label9.AutoSize = true;
            label9.Location = new Point(16, 113);
            label9.Name = "label9";
            label9.Size = new Size(85, 15);
            label9.TabIndex = 28;
            label9.Text = "Register Count";
            // 
            // label10
            // 
            label10.AutoSize = true;
            label10.Location = new Point(25, 85);
            label10.Name = "label10";
            label10.Size = new Size(76, 15);
            label10.TabIndex = 27;
            label10.Text = "Start Register";
            // 
            // label11
            // 
            label11.AutoSize = true;
            label11.Location = new Point(16, 57);
            label11.Name = "label11";
            label11.Size = new Size(85, 15);
            label11.TabIndex = 25;
            label11.Text = "Function Code";
            // 
            // label12
            // 
            label12.AutoSize = true;
            label12.Location = new Point(22, 28);
            label12.Name = "label12";
            label12.Size = new Size(79, 15);
            label12.TabIndex = 23;
            label12.Text = "Slave Address";
            // 
            // gridRx
            // 
            gridRx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridRx.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridRx.Columns.AddRange(new DataGridViewColumn[] { colRxReg, colRxHex, colRxDec });
            gridRx.Location = new Point(228, 28);
            gridRx.Name = "gridRx";
            gridRx.ReadOnly = true;
            gridRx.RowHeadersVisible = false;
            gridRx.RowHeadersWidth = 62;
            gridRx.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridRx.Size = new Size(200, 375);
            gridRx.TabIndex = 0;
            gridRx.CellEndEdit += HexAutoFormat_OnEndEdit;
            // 
            // colRxReg
            // 
            colRxReg.HeaderText = "Register";
            colRxReg.MinimumWidth = 8;
            colRxReg.Name = "colRxReg";
            colRxReg.ReadOnly = true;
            colRxReg.Width = 120;
            // 
            // colRxHex
            // 
            colRxHex.HeaderText = "HEX";
            colRxHex.MinimumWidth = 8;
            colRxHex.Name = "colRxHex";
            colRxHex.ReadOnly = true;
            colRxHex.Width = 120;
            // 
            // colRxDec
            // 
            colRxDec.HeaderText = "DEC";
            colRxDec.MinimumWidth = 8;
            colRxDec.Name = "colRxDec";
            colRxDec.ReadOnly = true;
            colRxDec.Width = 120;
            // 
            // grpCom
            // 
            grpCom.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpCom.Controls.Add(chkSlaveMode);
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
            // chkSlaveMode
            // 
            chkSlaveMode.AutoSize = true;
            chkSlaveMode.Location = new Point(88, 13);
            chkSlaveMode.Name = "chkSlaveMode";
            chkSlaveMode.Size = new Size(87, 19);
            chkSlaveMode.TabIndex = 12;
            chkSlaveMode.Text = "Slave Mode";
            chkSlaveMode.UseVisualStyleBackColor = true;
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
            label17.Size = new Size(50, 15);
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
            label16.Size = new Size(50, 15);
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
            label19.Size = new Size(46, 15);
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
            chkRecording.Size = new Size(80, 19);
            chkRecording.TabIndex = 1;
            chkRecording.Text = "Recording";
            chkRecording.UseVisualStyleBackColor = true;
            // 
            // label18
            // 
            label18.AutoSize = true;
            label18.Location = new Point(20, 37);
            label18.Name = "label18";
            label18.Size = new Size(58, 15);
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
            btnSaveLog.Location = new Point(1086, 17);
            btnSaveLog.Name = "btnSaveLog";
            btnSaveLog.Size = new Size(80, 32);
            btnSaveLog.TabIndex = 2;
            btnSaveLog.Text = "Save";
            btnSaveLog.UseVisualStyleBackColor = true;
            btnSaveLog.Click += btnSaveLog_Click;
            // 
            // btnLogClear
            // 
            btnLogClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLogClear.Location = new Point(1000, 17);
            btnLogClear.Name = "btnLogClear";
            btnLogClear.Size = new Size(80, 32);
            btnLogClear.TabIndex = 1;
            btnLogClear.Text = "Clear";
            btnLogClear.UseVisualStyleBackColor = true;
            btnLogClear.Click += btnLogClear_Click;
            // 
            // txtLog
            // 
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.Font = new Font("Consolas", 10F);
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
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1196, 672);
            Controls.Add(grpLog);
            Controls.Add(grpOpt);
            Controls.Add(grpCom);
            Controls.Add(grpRx);
            Controls.Add(grpTx);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(1212, 711);
            Name = "FormMain";
            Text = "Modbus Tester";
            Load += FormMain_Load;
            grpTx.ResumeLayout(false);
            grpTx.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numStartRegister).EndInit();
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
        private NumericUpDown numCount;
        private Label label4;
        private Label label3;
        private ComboBox cmbFunctionCode;
        private Label label2;
        private NumericUpDown numSlave;
        private Label label1;
        private DataGridView gridTx;
        private DataGridViewTextBoxColumn colTxReg;
        private DataGridViewTextBoxColumn colTxHex;
        private DataGridViewTextBoxColumn colTxDec;
        private GroupBox grpRx;
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
        private NumericUpDown numStartRegister;
        private Label label9;
        private Label label10;
        private Label label11;
        private Label label12;
        private TextBox txtRxFc;
        private TextBox txtRxStart;
        private TextBox txtRxCount;
        private TextBox txtRxSlave;
        private Button btnSend;
        private Button btnCalcCrc;
        private TextBox txtCrc;
        private Label label6;
        private TextBox txtDataCount;
        private Label label5;
        private TextBox txtRxDataCount;
        private TextBox txtRxCrc;
        private Button btnCopyToTx;
        private Button btnRxClear;
        private Label label7;
        private Label label8;
        private CheckBox chkSlaveMode;
        private Button btnQuickView;
    }
}
