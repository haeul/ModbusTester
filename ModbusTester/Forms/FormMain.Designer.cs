using ModbusTester.Controls;
using System;
using System.Drawing;
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
            colRxQuickView = new DataGridViewCheckBoxColumn();
            grpOpt = new GroupBox();
            lblRecordingInterval = new Label();
            btnPollStop = new Button();
            btnPollStart = new Button();
            numInterval = new NumericUpDown();
            lblPollingInterval = new Label();
            cmbRecordEvery = new ComboBox();
            chkRecording = new CheckBox();
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
            numStartRegister = new HexNumericUpDown();
            lblTxDataCount = new Label();
            lblTxCrc = new Label();
            btnCalcCrc = new Button();
            btnSend = new Button();
            txtDataCount = new TextBox();
            txtCrc = new TextBox();
            cmbPreset = new ComboBox();
            btnPresetDelete = new Button();
            btnPresetSave = new Button();
            btnTxClear = new Button();
            btnRevert = new Button();
            grpTx = new GroupBox();
            groupBox1 = new GroupBox();
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
            groupBox1.SuspendLayout();
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
            grpRx.Location = new Point(683, 12);
            grpRx.Name = "grpRx";
            grpRx.Size = new Size(690, 420);
            grpRx.TabIndex = 1;
            grpRx.TabStop = false;
            grpRx.Text = "Receive";
            // 
            // btnQuickView
            // 
            btnQuickView.Location = new Point(7, 242);
            btnQuickView.Name = "btnQuickView";
            btnQuickView.Size = new Size(209, 32);
            btnQuickView.TabIndex = 49;
            btnQuickView.Text = "QuickView";
            btnQuickView.UseVisualStyleBackColor = true;
            btnQuickView.Click += btnQuickView_Click;
            // 
            // txtRxDataCount
            // 
            txtRxDataCount.Location = new Point(108, 147);
            txtRxDataCount.Name = "txtRxDataCount";
            txtRxDataCount.ReadOnly = true;
            txtRxDataCount.Size = new Size(108, 25);
            txtRxDataCount.TabIndex = 48;
            // 
            // txtRxCrc
            // 
            txtRxCrc.Location = new Point(108, 177);
            txtRxCrc.Name = "txtRxCrc";
            txtRxCrc.ReadOnly = true;
            txtRxCrc.Size = new Size(108, 25);
            txtRxCrc.TabIndex = 47;
            // 
            // btnCopyToTx
            // 
            btnCopyToTx.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCopyToTx.Location = new Point(116, 207);
            btnCopyToTx.Name = "btnCopyToTx";
            btnCopyToTx.Size = new Size(100, 32);
            btnCopyToTx.TabIndex = 46;
            btnCopyToTx.Text = "Copy to TX";
            btnCopyToTx.UseVisualStyleBackColor = true;
            btnCopyToTx.Click += btnCopyToTx_Click;
            // 
            // btnRxClear
            // 
            btnRxClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRxClear.Location = new Point(7, 207);
            btnRxClear.Name = "btnRxClear";
            btnRxClear.Size = new Size(100, 32);
            btnRxClear.TabIndex = 45;
            btnRxClear.Text = "Clear";
            btnRxClear.UseVisualStyleBackColor = true;
            btnRxClear.Click += btnRxClear_Click;
            // 
            // lblRxCrc
            // 
            lblRxCrc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRxCrc.AutoSize = true;
            lblRxCrc.Location = new Point(70, 177);
            lblRxCrc.Name = "lblRxCrc";
            lblRxCrc.Size = new Size(35, 19);
            lblRxCrc.TabIndex = 44;
            lblRxCrc.Text = "CRC";
            // 
            // lblRxDataCount
            // 
            lblRxDataCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRxDataCount.AutoSize = true;
            lblRxDataCount.Location = new Point(25, 147);
            lblRxDataCount.Name = "lblRxDataCount";
            lblRxDataCount.Size = new Size(80, 19);
            lblRxDataCount.TabIndex = 43;
            lblRxDataCount.Text = "Data Count";
            // 
            // txtRxFc
            // 
            txtRxFc.Location = new Point(108, 57);
            txtRxFc.Name = "txtRxFc";
            txtRxFc.ReadOnly = true;
            txtRxFc.Size = new Size(108, 25);
            txtRxFc.TabIndex = 42;
            // 
            // txtRxStart
            // 
            txtRxStart.Location = new Point(108, 87);
            txtRxStart.Name = "txtRxStart";
            txtRxStart.ReadOnly = true;
            txtRxStart.Size = new Size(108, 25);
            txtRxStart.TabIndex = 41;
            // 
            // txtRxCount
            // 
            txtRxCount.Location = new Point(108, 117);
            txtRxCount.Name = "txtRxCount";
            txtRxCount.ReadOnly = true;
            txtRxCount.Size = new Size(108, 25);
            txtRxCount.TabIndex = 40;
            // 
            // txtRxSlave
            // 
            txtRxSlave.Location = new Point(108, 27);
            txtRxSlave.Name = "txtRxSlave";
            txtRxSlave.ReadOnly = true;
            txtRxSlave.Size = new Size(108, 25);
            txtRxSlave.TabIndex = 37;
            // 
            // lblRxRegisterCount
            // 
            lblRxRegisterCount.AutoSize = true;
            lblRxRegisterCount.Location = new Point(5, 117);
            lblRxRegisterCount.Name = "lblRxRegisterCount";
            lblRxRegisterCount.Size = new Size(100, 19);
            lblRxRegisterCount.TabIndex = 28;
            lblRxRegisterCount.Text = "Register Count";
            // 
            // lblRxStartRegister
            // 
            lblRxStartRegister.AutoSize = true;
            lblRxStartRegister.Location = new Point(14, 87);
            lblRxStartRegister.Name = "lblRxStartRegister";
            lblRxStartRegister.Size = new Size(91, 19);
            lblRxStartRegister.TabIndex = 27;
            lblRxStartRegister.Text = "Start Register";
            // 
            // lblRxFunctionCode
            // 
            lblRxFunctionCode.AutoSize = true;
            lblRxFunctionCode.Location = new Point(7, 57);
            lblRxFunctionCode.Name = "lblRxFunctionCode";
            lblRxFunctionCode.Size = new Size(98, 19);
            lblRxFunctionCode.TabIndex = 25;
            lblRxFunctionCode.Text = "Function Code";
            // 
            // lblRxSlaveAddress
            // 
            lblRxSlaveAddress.AutoSize = true;
            lblRxSlaveAddress.Location = new Point(12, 27);
            lblRxSlaveAddress.Name = "lblRxSlaveAddress";
            lblRxSlaveAddress.Size = new Size(93, 19);
            lblRxSlaveAddress.TabIndex = 23;
            lblRxSlaveAddress.Text = "Slave Address";
            // 
            // gridRx
            // 
            gridRx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            gridRx.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridRx.Columns.AddRange(new DataGridViewColumn[] { colRxReg, colRxNick, colRxHex, colRxDec, colRxBit, colRxQuickView });
            gridRx.Location = new Point(227, 23);
            gridRx.Name = "gridRx";
            gridRx.RowHeadersVisible = false;
            gridRx.RowHeadersWidth = 62;
            gridRx.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridRx.Size = new Size(453, 375);
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
            // colRxQuickView
            // 
            colRxQuickView.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            colRxQuickView.HeaderText = "QV";
            colRxQuickView.Name = "colRxQuickView";
            colRxQuickView.Resizable = DataGridViewTriState.True;
            colRxQuickView.SortMode = DataGridViewColumnSortMode.Automatic;
            colRxQuickView.Width = 30;
            // 
            // grpOpt
            // 
            grpOpt.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            grpOpt.Controls.Add(lblRecordingInterval);
            grpOpt.Controls.Add(btnPollStop);
            grpOpt.Controls.Add(btnPollStart);
            grpOpt.Controls.Add(numInterval);
            grpOpt.Controls.Add(lblPollingInterval);
            grpOpt.Controls.Add(cmbRecordEvery);
            grpOpt.Controls.Add(chkRecording);
            grpOpt.Location = new Point(1113, 438);
            grpOpt.Name = "grpOpt";
            grpOpt.Size = new Size(260, 200);
            grpOpt.TabIndex = 3;
            grpOpt.TabStop = false;
            grpOpt.Text = "Recording / Polling";
            // 
            // lblRecordingInterval
            // 
            lblRecordingInterval.AutoSize = true;
            lblRecordingInterval.Location = new Point(15, 68);
            lblRecordingInterval.Name = "lblRecordingInterval";
            lblRecordingInterval.Size = new Size(101, 19);
            lblRecordingInterval.TabIndex = 7;
            lblRecordingInterval.Text = "Record Interval";
            // 
            // btnPollStop
            // 
            btnPollStop.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPollStop.Location = new Point(135, 140);
            btnPollStop.Name = "btnPollStop";
            btnPollStop.Size = new Size(110, 32);
            btnPollStop.TabIndex = 6;
            btnPollStop.Text = "Stop";
            btnPollStop.UseVisualStyleBackColor = true;
            btnPollStop.Click += btnPollStop_Click;
            // 
            // btnPollStart
            // 
            btnPollStart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPollStart.Location = new Point(15, 140);
            btnPollStart.Name = "btnPollStart";
            btnPollStart.Size = new Size(110, 32);
            btnPollStart.TabIndex = 5;
            btnPollStart.Text = "Start";
            btnPollStart.UseVisualStyleBackColor = true;
            btnPollStart.Click += btnPollStart_Click;
            // 
            // numInterval
            // 
            numInterval.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            numInterval.Location = new Point(123, 100);
            numInterval.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numInterval.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numInterval.Name = "numInterval";
            numInterval.Size = new Size(122, 25);
            numInterval.TabIndex = 4;
            numInterval.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // lblPollingInterval
            // 
            lblPollingInterval.AutoSize = true;
            lblPollingInterval.Location = new Point(15, 103);
            lblPollingInterval.Name = "lblPollingInterval";
            lblPollingInterval.Size = new Size(99, 19);
            lblPollingInterval.TabIndex = 3;
            lblPollingInterval.Text = "Polling Interval";
            // 
            // cmbRecordEvery
            // 
            cmbRecordEvery.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRecordEvery.FormattingEnabled = true;
            cmbRecordEvery.Items.AddRange(new object[] { "1 sec", "5 sec", "10 sec", "30 sec", "60 sec" });
            cmbRecordEvery.Location = new Point(123, 65);
            cmbRecordEvery.Name = "cmbRecordEvery";
            cmbRecordEvery.Size = new Size(122, 25);
            cmbRecordEvery.TabIndex = 2;
            cmbRecordEvery.SelectedIndex = 0;
            // 
            // chkRecording
            // 
            chkRecording.AutoSize = true;
            chkRecording.Location = new Point(15, 35);
            chkRecording.Name = "chkRecording";
            chkRecording.Size = new Size(133, 23);
            chkRecording.TabIndex = 1;
            chkRecording.Text = "Enable Recording";
            chkRecording.UseVisualStyleBackColor = true;
            chkRecording.CheckedChanged += chkRecording_CheckedChanged;
            // 
            // grpLog
            // 
            grpLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            grpLog.Controls.Add(btnSaveLog);
            grpLog.Controls.Add(btnLogClear);
            grpLog.Controls.Add(txtLog);
            grpLog.Location = new Point(12, 438);
            grpLog.Name = "grpLog";
            grpLog.Size = new Size(1092, 200);
            grpLog.TabIndex = 4;
            grpLog.TabStop = false;
            grpLog.Text = "Message (HEX Log)";
            // 
            // btnSaveLog
            // 
            btnSaveLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveLog.Location = new Point(1006, 15);
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
            btnLogClear.Location = new Point(920, 15);
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
            txtLog.Location = new Point(6, 53);
            txtLog.Name = "txtLog";
            txtLog.ReadOnly = true;
            txtLog.Size = new Size(1080, 140);
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
            gridTx.Location = new Point(229, 23);
            gridTx.Name = "gridTx";
            gridTx.RowHeadersVisible = false;
            gridTx.RowHeadersWidth = 62;
            gridTx.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridTx.Size = new Size(426, 375);
            gridTx.TabIndex = 105;
            gridTx.CellBeginEdit += Grid_CellBeginEdit;
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
            lblTxSlaveAddress.Location = new Point(14, 27);
            lblTxSlaveAddress.Name = "lblTxSlaveAddress";
            lblTxSlaveAddress.Size = new Size(93, 19);
            lblTxSlaveAddress.TabIndex = 98;
            lblTxSlaveAddress.Text = "Slave Address";
            // 
            // numSlave
            // 
            numSlave.Location = new Point(111, 27);
            numSlave.Maximum = new decimal(new int[] { 247, 0, 0, 0 });
            numSlave.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSlave.Name = "numSlave";
            numSlave.Size = new Size(108, 25);
            numSlave.TabIndex = 99;
            numSlave.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // lblTxFunctionCode
            // 
            lblTxFunctionCode.AutoSize = true;
            lblTxFunctionCode.Location = new Point(9, 57);
            lblTxFunctionCode.Name = "lblTxFunctionCode";
            lblTxFunctionCode.Size = new Size(98, 19);
            lblTxFunctionCode.TabIndex = 100;
            lblTxFunctionCode.Text = "Function Code";
            // 
            // cmbFunctionCode
            // 
            cmbFunctionCode.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFunctionCode.FormattingEnabled = true;
            cmbFunctionCode.Items.AddRange(new object[] { "03h Read HR", "04h Read IR", "06h Write SR", "10h Write MR" });
            cmbFunctionCode.Location = new Point(111, 57);
            cmbFunctionCode.Name = "cmbFunctionCode";
            cmbFunctionCode.Size = new Size(108, 25);
            cmbFunctionCode.TabIndex = 101;
            cmbFunctionCode.SelectedIndexChanged += cmbFunctionCode_SelectedIndexChanged;
            cmbFunctionCode.TextChanged += cmbFunctionCode_TextChanged;
            // 
            // lblTxStartRegister
            // 
            lblTxStartRegister.AutoSize = true;
            lblTxStartRegister.Location = new Point(16, 87);
            lblTxStartRegister.Name = "lblTxStartRegister";
            lblTxStartRegister.Size = new Size(91, 19);
            lblTxStartRegister.TabIndex = 102;
            lblTxStartRegister.Text = "Start Register";
            // 
            // lblTxRegisterCount
            // 
            lblTxRegisterCount.AutoSize = true;
            lblTxRegisterCount.Location = new Point(7, 117);
            lblTxRegisterCount.Name = "lblTxRegisterCount";
            lblTxRegisterCount.Size = new Size(100, 19);
            lblTxRegisterCount.TabIndex = 103;
            lblTxRegisterCount.Text = "Register Count";
            // 
            // numCount
            // 
            numCount.Location = new Point(111, 117);
            numCount.Maximum = new decimal(new int[] { 125, 0, 0, 0 });
            numCount.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numCount.Name = "numCount";
            numCount.Size = new Size(108, 25);
            numCount.TabIndex = 104;
            numCount.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numCount.ValueChanged += numCount_ValueChanged;
            // 
            // numStartRegister
            // 
            numStartRegister.Hexadecimal = true;
            numStartRegister.Location = new Point(111, 87);
            numStartRegister.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numStartRegister.Name = "numStartRegister";
            numStartRegister.Size = new Size(108, 25);
            numStartRegister.TabIndex = 106;
            // 
            // lblTxDataCount
            // 
            lblTxDataCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTxDataCount.AutoSize = true;
            lblTxDataCount.Location = new Point(27, 147);
            lblTxDataCount.Name = "lblTxDataCount";
            lblTxDataCount.Size = new Size(80, 19);
            lblTxDataCount.TabIndex = 107;
            lblTxDataCount.Text = "Data Count";
            // 
            // lblTxCrc
            // 
            lblTxCrc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTxCrc.AutoSize = true;
            lblTxCrc.Location = new Point(72, 177);
            lblTxCrc.Name = "lblTxCrc";
            lblTxCrc.Size = new Size(35, 19);
            lblTxCrc.TabIndex = 108;
            lblTxCrc.Text = "CRC";
            // 
            // btnCalcCrc
            // 
            btnCalcCrc.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCalcCrc.Location = new Point(10, 207);
            btnCalcCrc.Name = "btnCalcCrc";
            btnCalcCrc.Size = new Size(100, 32);
            btnCalcCrc.TabIndex = 109;
            btnCalcCrc.Text = "Calc CRC";
            btnCalcCrc.UseVisualStyleBackColor = true;
            btnCalcCrc.Click += btnCalcCrc_Click;
            // 
            // btnSend
            // 
            btnSend.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSend.Location = new Point(119, 207);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(100, 32);
            btnSend.TabIndex = 110;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // txtDataCount
            // 
            txtDataCount.Location = new Point(111, 147);
            txtDataCount.Name = "txtDataCount";
            txtDataCount.Size = new Size(108, 25);
            txtDataCount.TabIndex = 112;
            // 
            // txtCrc
            // 
            txtCrc.Location = new Point(111, 177);
            txtCrc.Name = "txtCrc";
            txtCrc.Size = new Size(108, 25);
            txtCrc.TabIndex = 111;
            // 
            // cmbPreset
            // 
            cmbPreset.FormattingEnabled = true;
            cmbPreset.Location = new Point(5, 30);
            cmbPreset.Name = "cmbPreset";
            cmbPreset.Size = new Size(209, 25);
            cmbPreset.TabIndex = 115;
            cmbPreset.SelectedIndexChanged += cmbPreset_SelectedIndexChanged;
            // 
            // btnPresetDelete
            // 
            btnPresetDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPresetDelete.Location = new Point(114, 60);
            btnPresetDelete.Name = "btnPresetDelete";
            btnPresetDelete.Size = new Size(100, 32);
            btnPresetDelete.TabIndex = 116;
            btnPresetDelete.Text = "Delete";
            btnPresetDelete.UseVisualStyleBackColor = true;
            btnPresetDelete.Click += btnPresetDelete_Click;
            // 
            // btnPresetSave
            // 
            btnPresetSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPresetSave.Location = new Point(5, 60);
            btnPresetSave.Name = "btnPresetSave";
            btnPresetSave.Size = new Size(100, 32);
            btnPresetSave.TabIndex = 117;
            btnPresetSave.Text = "Save";
            btnPresetSave.UseVisualStyleBackColor = true;
            btnPresetSave.Click += btnPresetSave_Click;
            // 
            // btnTxClear
            // 
            btnTxClear.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTxClear.Location = new Point(10, 242);
            btnTxClear.Name = "btnTxClear";
            btnTxClear.Size = new Size(100, 32);
            btnTxClear.TabIndex = 113;
            btnTxClear.Text = "Clear";
            btnTxClear.UseVisualStyleBackColor = true;
            btnTxClear.Click += btnTxClear_Click;
            // 
            // btnRevert
            // 
            btnRevert.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRevert.Location = new Point(119, 242);
            btnRevert.Name = "btnRevert";
            btnRevert.Size = new Size(100, 32);
            btnRevert.TabIndex = 118;
            btnRevert.Text = "Revert";
            btnRevert.UseVisualStyleBackColor = true;
            btnRevert.Click += btnRevert_Click;
            // 
            // grpTx
            // 
            grpTx.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            grpTx.Controls.Add(groupBox1);
            grpTx.Controls.Add(btnRevert);
            grpTx.Controls.Add(btnTxClear);
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
            grpTx.Location = new Point(12, 12);
            grpTx.Name = "grpTx";
            grpTx.Size = new Size(665, 420);
            grpTx.TabIndex = 0;
            grpTx.TabStop = false;
            grpTx.Text = "Transmit";
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnPresetDelete);
            groupBox1.Controls.Add(cmbPreset);
            groupBox1.Controls.Add(btnPresetSave);
            groupBox1.Location = new Point(5, 288);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(220, 110);
            groupBox1.TabIndex = 3;
            groupBox1.TabStop = false;
            groupBox1.Text = "Preset";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1385, 651);
            Controls.Add(grpTx);
            Controls.Add(grpLog);
            Controls.Add(grpOpt);
            Controls.Add(grpRx);
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(1212, 690);
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
            groupBox1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion
        private GroupBox grpRx;
        private DataGridView gridRx;
        private GroupBox grpOpt;
        private Button btnPollStop;
        private Button btnPollStart;
        private NumericUpDown numInterval;
        private Label lblPollingInterval;
        private ComboBox cmbRecordEvery;
        private CheckBox chkRecording;
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
        private GroupBox grpTx;
        private Button btnRevert;
        private Button btnTxClear;
        private Button btnPresetSave;
        private Button btnPresetDelete;
        private ComboBox cmbPreset;
        private TextBox txtCrc;
        private TextBox txtDataCount;
        private Button btnSend;
        private Button btnCalcCrc;
        private Label lblTxCrc;
        private Label lblTxDataCount;
        private HexNumericUpDown numStartRegister;
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
        private DataGridViewTextBoxColumn colRxReg;
        private DataGridViewTextBoxColumn colRxNick;
        private DataGridViewTextBoxColumn colRxHex;
        private DataGridViewTextBoxColumn colRxDec;
        private DataGridViewTextBoxColumn colRxBit;
        private DataGridViewCheckBoxColumn colRxQuickView;
        private GroupBox groupBox1;
        private Label lblRecordingInterval;
    }
}
