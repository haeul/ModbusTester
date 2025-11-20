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
            grpRx = new GroupBox();
            btnQuickView = new Button();
            txtRxDataCount = new TextBox();
            txtRxCrc = new TextBox();
            btnCopyToTx = new Button();
            btnRxClear = new Button();
            lblRxCrc = new Label();
            lblRxDataCount = new Label();
            txtRxFc = new TextBox();
            txtRxStart = new TextBox();
            txtRxCount = new TextBox();
            txtRxSlave = new TextBox();
            lblRxRegisterCount = new Label();
            lblRxStartRegister = new Label();
            lblRxFunctionCode = new Label();
            lblRxSlaveAddress = new Label();
            gridRx = new DataGridView();
            colRxReg = new DataGridViewTextBoxColumn();
            colRxNick = new DataGridViewTextBoxColumn();
            colRxHex = new DataGridViewTextBoxColumn();
            colRxDec = new DataGridViewTextBoxColumn();
            colRxBit = new DataGridViewTextBoxColumn();
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
            gridTx = new DataGridView();
            colTxReg = new DataGridViewTextBoxColumn();
            colTxNick = new DataGridViewTextBoxColumn();
            colTxHex = new DataGridViewTextBoxColumn();
            colTxDec = new DataGridViewTextBoxColumn();
            colTxBit = new DataGridViewTextBoxColumn();
            lblTxSlaveAddress = new Label();
            numSlave = new NumericUpDown();
            lblTxFunctionCode = new Label();
            cmbFunctionCode = new ComboBox();
            lblTxStartRegister = new Label();
            lblTxRegisterCount = new Label();
            numCount = new NumericUpDown();
            numStartRegister = new NumericUpDown();
            lblTxDataCount = new Label();
            lblTxCrc = new Label();
            btnCalcCrc = new Button();
            btnSend = new Button();
            txtDataCount = new TextBox();
            txtCrc = new TextBox();
            lblPreset = new Label();
            cmbPreset = new ComboBox();
            btnPresetDelete = new Button();
            btnPresetSave = new Button();
            btnTxClear = new Button();
            btnRestore = new Button();
            grpTx = new GroupBox();
            grpRx.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridRx).BeginInit();
            grpOpt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numInterval).BeginInit();
            grpLog.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)gridTx).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numSlave).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCount).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numStartRegister).BeginInit();
            grpTx.SuspendLayout();
            SuspendLayout();
            // 
            // grpRx
            // 
            grpRx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            grpRx.Controls.Add(btnQuickView);
            grpRx.Controls.Add(txtRxDataCount);
            grpRx.Controls.Add(txtRxCrc);
            grpRx.Controls.Add(btnCopyToTx);
            grpRx.Controls.Add(btnRxClear);
            grpRx.Controls.Add(lblRxCrc);
            grpRx.Controls.Add(lblRxDataCount);
            grpRx.Controls.Add(txtRxFc);
            grpRx.Controls.Add(txtRxStart);
            grpRx.Controls.Add(txtRxCount);
            grpRx.Controls.Add(txtRxSlave);
            grpRx.Controls.Add(lblRxRegisterCount);
            grpRx.Controls.Add(lblRxStartRegister);
            grpRx.Controls.Add(lblRxFunctionCode);
            grpRx.Controls.Add(lblRxSlaveAddress);
            grpRx.Controls.Add(gridRx);
            grpRx.Location = new Point(672, 12);
            grpRx.Name = "grpRx";
            grpRx.Size = new Size(643, 420);
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
            // lblRxCrc
            // 
            lblRxCrc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRxCrc.AutoSize = true;
            lblRxCrc.Location = new Point(71, 171);
            lblRxCrc.Name = "lblRxCrc";
            lblRxCrc.Size = new Size(30, 15);
            lblRxCrc.TabIndex = 44;
            lblRxCrc.Text = "CRC";
            // 
            // lblRxDataCount
            // 
            lblRxDataCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRxDataCount.AutoSize = true;
            lblRxDataCount.Location = new Point(37, 143);
            lblRxDataCount.Name = "lblRxDataCount";
            lblRxDataCount.Size = new Size(64, 15);
            lblRxDataCount.TabIndex = 43;
            lblRxDataCount.Text = "DataCount";
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
            // lblRxRegisterCount
            // 
            lblRxRegisterCount.AutoSize = true;
            lblRxRegisterCount.Location = new Point(16, 113);
            lblRxRegisterCount.Name = "lblRxRegisterCount";
            lblRxRegisterCount.Size = new Size(85, 15);
            lblRxRegisterCount.TabIndex = 28;
            lblRxRegisterCount.Text = "Register Count";
            // 
            // lblRxStartRegister
            // 
            lblRxStartRegister.AutoSize = true;
            lblRxStartRegister.Location = new Point(25, 85);
            lblRxStartRegister.Name = "lblRxStartRegister";
            lblRxStartRegister.Size = new Size(76, 15);
            lblRxStartRegister.TabIndex = 27;
            lblRxStartRegister.Text = "Start Register";
            // 
            // lblRxFunctionCode
            // 
            lblRxFunctionCode.AutoSize = true;
            lblRxFunctionCode.Location = new Point(16, 57);
            lblRxFunctionCode.Name = "lblRxFunctionCode";
            lblRxFunctionCode.Size = new Size(85, 15);
            lblRxFunctionCode.TabIndex = 25;
            lblRxFunctionCode.Text = "Function Code";
            // 
            // lblRxSlaveAddress
            // 
            lblRxSlaveAddress.AutoSize = true;
            lblRxSlaveAddress.Location = new Point(22, 28);
            lblRxSlaveAddress.Name = "lblRxSlaveAddress";
            lblRxSlaveAddress.Size = new Size(79, 15);
            lblRxSlaveAddress.TabIndex = 23;
            lblRxSlaveAddress.Text = "Slave Address";
            // 
            // gridRx
            // 
            gridRx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridRx.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridRx.Columns.AddRange(new DataGridViewColumn[] { colRxReg, colRxNick, colRxHex, colRxDec, colRxBit });
            gridRx.Location = new Point(228, 28);
            gridRx.Name = "gridRx";
            gridRx.RowHeadersVisible = false;
            gridRx.RowHeadersWidth = 62;
            gridRx.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridRx.Size = new Size(406, 375);
            gridRx.TabIndex = 0;
            gridRx.CellBeginEdit += Grid_CellBeginEdit;
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
            // colRxNick
            // 
            colRxNick.HeaderText = "Name";
            colRxNick.Name = "colRxNick";
            colRxNick.ReadOnly = true;
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
            // colRxBit
            // 
            colRxBit.HeaderText = "BIT";
            colRxBit.Name = "colRxBit";
            colRxBit.ReadOnly = true;
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
            grpOpt.Location = new Point(1055, 438);
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
            chkRecording.CheckedChanged += chkRecording_CheckedChanged;
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
            grpLog.Size = new Size(1034, 222);
            grpLog.TabIndex = 4;
            grpLog.TabStop = false;
            grpLog.Text = "Message (HEX Log)";
            // 
            // btnSaveLog
            // 
            btnSaveLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveLog.Location = new Point(948, 17);
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
            btnLogClear.Location = new Point(862, 17);
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
            txtLog.Size = new Size(1017, 160);
            txtLog.TabIndex = 0;
            txtLog.Text = "";
            txtLog.WordWrap = false;
            // 
            // pollTimer
            // 
            pollTimer.Tick += pollTimer_Tick;
            // 
            // gridTx
            // 
            gridTx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridTx.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridTx.Columns.AddRange(new DataGridViewColumn[] { colTxReg, colTxNick, colTxHex, colTxDec, colTxBit });
            gridTx.Location = new Point(234, 23);
            gridTx.Name = "gridTx";
            gridTx.RowHeadersVisible = false;
            gridTx.RowHeadersWidth = 62;
            gridTx.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridTx.Size = new Size(405, 375);
            gridTx.TabIndex = 105;
            // 
            // colTxReg
            // 
            colTxReg.HeaderText = "Register";
            colTxReg.MinimumWidth = 8;
            colTxReg.Name = "colTxReg";
            colTxReg.ReadOnly = true;
            colTxReg.Width = 120;
            // 
            // colTxNick
            // 
            colTxNick.HeaderText = "Name";
            colTxNick.Name = "colTxNick";
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
            // colTxBit
            // 
            colTxBit.HeaderText = "BIT";
            colTxBit.Name = "colTxBit";
            // 
            // lblTxSlaveAddress
            // 
            lblTxSlaveAddress.AutoSize = true;
            lblTxSlaveAddress.Location = new Point(22, 23);
            lblTxSlaveAddress.Name = "lblTxSlaveAddress";
            lblTxSlaveAddress.Size = new Size(79, 15);
            lblTxSlaveAddress.TabIndex = 98;
            lblTxSlaveAddress.Text = "Slave Address";
            // 
            // numSlave
            // 
            numSlave.Location = new Point(109, 23);
            numSlave.Maximum = new decimal(new int[] { 247, 0, 0, 0 });
            numSlave.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSlave.Name = "numSlave";
            numSlave.Size = new Size(108, 23);
            numSlave.TabIndex = 99;
            numSlave.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblTxFunctionCode
            // 
            lblTxFunctionCode.AutoSize = true;
            lblTxFunctionCode.Location = new Point(16, 52);
            lblTxFunctionCode.Name = "lblTxFunctionCode";
            lblTxFunctionCode.Size = new Size(85, 15);
            lblTxFunctionCode.TabIndex = 100;
            lblTxFunctionCode.Text = "Function Code";
            // 
            // cmbFunctionCode
            // 
            cmbFunctionCode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFunctionCode.FormattingEnabled = true;
            cmbFunctionCode.Items.AddRange(new object[] { "03h Read HR", "04h Read IR", "06h Write SR", "10h Write MR" });
            cmbFunctionCode.Location = new Point(109, 50);
            cmbFunctionCode.Name = "cmbFunctionCode";
            cmbFunctionCode.Size = new Size(108, 23);
            cmbFunctionCode.TabIndex = 101;
            // 
            // lblTxStartRegister
            // 
            lblTxStartRegister.AutoSize = true;
            lblTxStartRegister.Location = new Point(25, 80);
            lblTxStartRegister.Name = "lblTxStartRegister";
            lblTxStartRegister.Size = new Size(76, 15);
            lblTxStartRegister.TabIndex = 102;
            lblTxStartRegister.Text = "Start Register";
            // 
            // lblTxRegisterCount
            // 
            lblTxRegisterCount.AutoSize = true;
            lblTxRegisterCount.Location = new Point(16, 108);
            lblTxRegisterCount.Name = "lblTxRegisterCount";
            lblTxRegisterCount.Size = new Size(85, 15);
            lblTxRegisterCount.TabIndex = 103;
            lblTxRegisterCount.Text = "Register Count";
            // 
            // numCount
            // 
            numCount.Location = new Point(109, 106);
            numCount.Maximum = new decimal(new int[] { 125, 0, 0, 0 });
            numCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCount.Name = "numCount";
            numCount.Size = new Size(108, 23);
            numCount.TabIndex = 104;
            numCount.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // numStartRegister
            // 
            numStartRegister.Location = new Point(109, 77);
            numStartRegister.Name = "numStartRegister";
            numStartRegister.Size = new Size(108, 23);
            numStartRegister.TabIndex = 106;
            // 
            // lblTxDataCount
            // 
            lblTxDataCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTxDataCount.AutoSize = true;
            lblTxDataCount.Location = new Point(37, 132);
            lblTxDataCount.Name = "lblTxDataCount";
            lblTxDataCount.Size = new Size(64, 15);
            lblTxDataCount.TabIndex = 107;
            lblTxDataCount.Text = "DataCount";
            // 
            // lblTxCrc
            // 
            lblTxCrc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTxCrc.AutoSize = true;
            lblTxCrc.Location = new Point(71, 160);
            lblTxCrc.Name = "lblTxCrc";
            lblTxCrc.Size = new Size(30, 15);
            lblTxCrc.TabIndex = 108;
            lblTxCrc.Text = "CRC";
            // 
            // btnCalcCrc
            // 
            btnCalcCrc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCalcCrc.Location = new Point(16, 191);
            btnCalcCrc.Name = "btnCalcCrc";
            btnCalcCrc.Size = new Size(95, 44);
            btnCalcCrc.TabIndex = 109;
            btnCalcCrc.Text = "Calc CRC";
            btnCalcCrc.UseVisualStyleBackColor = true;
            // 
            // btnSend
            // 
            btnSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSend.Location = new Point(120, 191);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(95, 44);
            btnSend.TabIndex = 110;
            btnSend.Text = "SEND";
            btnSend.UseVisualStyleBackColor = true;
            // 
            // txtDataCount
            // 
            txtDataCount.Location = new Point(109, 133);
            txtDataCount.Name = "txtDataCount";
            txtDataCount.Size = new Size(108, 23);
            txtDataCount.TabIndex = 112;
            // 
            // txtCrc
            // 
            txtCrc.Location = new Point(109, 162);
            txtCrc.Name = "txtCrc";
            txtCrc.Size = new Size(108, 23);
            txtCrc.TabIndex = 111;
            // 
            // lblPreset
            // 
            lblPreset.AutoSize = true;
            lblPreset.Location = new Point(16, 260);
            lblPreset.Name = "lblPreset";
            lblPreset.Size = new Size(39, 15);
            lblPreset.TabIndex = 114;
            lblPreset.Text = "Preset";
            // 
            // cmbPreset
            // 
            cmbPreset.FormattingEnabled = true;
            cmbPreset.Location = new Point(61, 257);
            cmbPreset.Name = "cmbPreset";
            cmbPreset.Size = new Size(156, 23);
            cmbPreset.TabIndex = 115;
            // 
            // btnPresetDelete
            // 
            btnPresetDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPresetDelete.Location = new Point(122, 288);
            btnPresetDelete.Name = "btnPresetDelete";
            btnPresetDelete.Size = new Size(95, 44);
            btnPresetDelete.TabIndex = 116;
            btnPresetDelete.Text = "Delete";
            btnPresetDelete.UseVisualStyleBackColor = true;
            // 
            // btnPresetSave
            // 
            btnPresetSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPresetSave.Location = new Point(16, 288);
            btnPresetSave.Name = "btnPresetSave";
            btnPresetSave.Size = new Size(95, 44);
            btnPresetSave.TabIndex = 117;
            btnPresetSave.Text = "Save";
            btnPresetSave.UseVisualStyleBackColor = true;
            // 
            // btnTxClear
            // 
            btnTxClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTxClear.Location = new Point(16, 338);
            btnTxClear.Name = "btnTxClear";
            btnTxClear.Size = new Size(95, 44);
            btnTxClear.TabIndex = 113;
            btnTxClear.Text = "CLEAR";
            btnTxClear.UseVisualStyleBackColor = true;
            // 
            // btnRestore
            // 
            btnRestore.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRestore.Location = new Point(122, 338);
            btnRestore.Name = "btnRestore";
            btnRestore.Size = new Size(95, 44);
            btnRestore.TabIndex = 118;
            btnRestore.Text = "Restore";
            btnRestore.UseVisualStyleBackColor = true;
            // 
            // grpTx
            // 
            grpTx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            grpTx.Controls.Add(btnRestore);
            grpTx.Controls.Add(btnTxClear);
            grpTx.Controls.Add(btnPresetSave);
            grpTx.Controls.Add(btnPresetDelete);
            grpTx.Controls.Add(cmbPreset);
            grpTx.Controls.Add(lblPreset);
            grpTx.Controls.Add(txtCrc);
            grpTx.Controls.Add(txtDataCount);
            grpTx.Controls.Add(btnSend);
            grpTx.Controls.Add(btnCalcCrc);
            grpTx.Controls.Add(lblTxCrc);
            grpTx.Controls.Add(lblTxDataCount);
            grpTx.Controls.Add(numStartRegister);
            grpTx.Controls.Add(numCount);
            grpTx.Controls.Add(lblTxRegisterCount);
            grpTx.Controls.Add(lblTxStartRegister);
            grpTx.Controls.Add(cmbFunctionCode);
            grpTx.Controls.Add(lblTxFunctionCode);
            grpTx.Controls.Add(numSlave);
            grpTx.Controls.Add(lblTxSlaveAddress);
            grpTx.Controls.Add(gridTx);
            grpTx.Location = new Point(0, 0);
            grpTx.Name = "grpTx";
            grpTx.Size = new Size(654, 420);
            grpTx.TabIndex = 0;
            grpTx.TabStop = false;
            grpTx.Text = "Transmit (Send)";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1327, 672);
            Controls.Add(grpTx);
            Controls.Add(grpLog);
            Controls.Add(grpOpt);
            Controls.Add(grpRx);
            Font = new Font("Segoe UI", 9F);
            MinimumSize = new Size(1212, 711);
            Name = "FormMain";
            Text = "Modbus Tester";
            Load += FormMain_Load;
            grpRx.ResumeLayout(false);
            grpRx.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)gridRx).EndInit();
            grpOpt.ResumeLayout(false);
            grpOpt.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numInterval).EndInit();
            grpLog.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)gridTx).EndInit();
            ((System.ComponentModel.ISupportInitialize)numSlave).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCount).EndInit();
            ((System.ComponentModel.ISupportInitialize)numStartRegister).EndInit();
            grpTx.ResumeLayout(false);
            grpTx.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private GroupBox grpRx;
        private DataGridView gridRx;
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
        private Label lblRxRegisterCount;
        private Label lblRxStartRegister;
        private Label lblRxFunctionCode;
        private Label lblRxSlaveAddress;
        private TextBox txtRxFc;
        private TextBox txtRxStart;
        private TextBox txtRxCount;
        private TextBox txtRxSlave;
        private TextBox txtRxDataCount;
        private TextBox txtRxCrc;
        private Button btnCopyToTx;
        private Button btnRxClear;
        private Label lblRxCrc;
        private Label lblRxDataCount;
        private Button btnQuickView;
        private DataGridViewTextBoxColumn colRxReg;
        private DataGridViewTextBoxColumn colRxNick;
        private DataGridViewTextBoxColumn colRxHex;
        private DataGridViewTextBoxColumn colRxDec;
        private DataGridViewTextBoxColumn colRxBit;
        private GroupBox grpTx;
        private Button btnRestore;
        private Button btnTxClear;
        private Button btnPresetSave;
        private Button btnPresetDelete;
        private ComboBox cmbPreset;
        private Label lblPreset;
        private TextBox txtCrc;
        private TextBox txtDataCount;
        private Button btnSend;
        private Button btnCalcCrc;
        private Label lblTxCrc;
        private Label lblTxDataCount;
        private NumericUpDown numStartRegister;
        private NumericUpDown numCount;
        private Label lblTxRegisterCount;
        private Label lblTxStartRegister;
        private ComboBox cmbFunctionCode;
        private Label lblTxFunctionCode;
        private NumericUpDown numSlave;
        private Label lblTxSlaveAddress;
        public DataGridView gridTx;
        private DataGridViewTextBoxColumn colTxReg;
        private DataGridViewTextBoxColumn colTxNick;
        private DataGridViewTextBoxColumn colTxHex;
        private DataGridViewTextBoxColumn colTxDec;
        private DataGridViewTextBoxColumn colTxBit;
    }
}
