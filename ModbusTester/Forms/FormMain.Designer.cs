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
            btnPollStop = new Button();
            btnPollStart = new Button();
            numInterval = new NumericUpDown();
            label19 = new Label();
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
            btnRevert = new Button();
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
            lblRxCrc.Location = new Point(71, 177);
            lblRxCrc.Name = "lblRxCrc";
            lblRxCrc.Size = new Size(35, 19);
            lblRxCrc.TabIndex = 44;
            lblRxCrc.Text = "CRC";
            // 
            // lblRxDataCount
            // 
            lblRxDataCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblRxDataCount.AutoSize = true;
            lblRxDataCount.Location = new Point(30, 147);
            lblRxDataCount.Name = "lblRxDataCount";
            lblRxDataCount.Size = new Size(76, 19);
            lblRxDataCount.TabIndex = 43;
            lblRxDataCount.Text = "DataCount";
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
            lblRxRegisterCount.Location = new Point(6, 121);
            lblRxRegisterCount.Name = "lblRxRegisterCount";
            lblRxRegisterCount.Size = new Size(100, 19);
            lblRxRegisterCount.TabIndex = 28;
            lblRxRegisterCount.Text = "Register Count";
            // 
            // lblRxStartRegister
            // 
            lblRxStartRegister.AutoSize = true;
            lblRxStartRegister.Location = new Point(15, 91);
            lblRxStartRegister.Name = "lblRxStartRegister";
            lblRxStartRegister.Size = new Size(91, 19);
            lblRxStartRegister.TabIndex = 27;
            lblRxStartRegister.Text = "Start Register";
            // 
            // lblRxFunctionCode
            // 
            lblRxFunctionCode.AutoSize = true;
            lblRxFunctionCode.Location = new Point(8, 61);
            lblRxFunctionCode.Name = "lblRxFunctionCode";
            lblRxFunctionCode.Size = new Size(98, 19);
            lblRxFunctionCode.TabIndex = 25;
            lblRxFunctionCode.Text = "Function Code";
            // 
            // lblRxSlaveAddress
            // 
            lblRxSlaveAddress.AutoSize = true;
            lblRxSlaveAddress.Location = new Point(13, 31);
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
            grpOpt.Controls.Add(btnPollStop);
            grpOpt.Controls.Add(btnPollStart);
            grpOpt.Controls.Add(numInterval);
            grpOpt.Controls.Add(label19);
            grpOpt.Controls.Add(cmbRecordEvery);
            grpOpt.Controls.Add(chkRecording);
            grpOpt.Location = new Point(1113, 438);
            grpOpt.Name = "grpOpt";
            grpOpt.Size = new Size(260, 222);
            grpOpt.TabIndex = 3;
            grpOpt.TabStop = false;
            grpOpt.Text = "Recording / Polling";
            // 
            // btnPollStop
            // 
            btnPollStop.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnPollStop.Location = new Point(138, 110);
            btnPollStop.Name = "btnPollStop";
            btnPollStop.Size = new Size(100, 32);
            btnPollStop.TabIndex = 6;
            btnPollStop.Text = "Stop";
            btnPollStop.UseVisualStyleBackColor = true;
            btnPollStop.Click += btnPollStop_Click;
            // 
            // btnPollStart
            // 
            btnPollStart.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPollStart.Location = new Point(20, 110);
            btnPollStart.Name = "btnPollStart";
            btnPollStart.Size = new Size(100, 32);
            btnPollStart.TabIndex = 5;
            btnPollStart.Text = "Start";
            btnPollStart.UseVisualStyleBackColor = true;
            btnPollStart.Click += btnPollStart_Click;
            // 
            // numInterval
            // 
            numInterval.Increment = new decimal(new int[] { 50, 0, 0, 0 });
            numInterval.Location = new Point(116, 77);
            numInterval.Maximum = new decimal(new int[] { 60000, 0, 0, 0 });
            numInterval.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numInterval.Name = "numInterval";
            numInterval.Size = new Size(122, 25);
            numInterval.TabIndex = 4;
            numInterval.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // label19
            // 
            label19.AutoSize = true;
            label19.Location = new Point(49, 77);
            label19.Name = "label19";
            label19.Size = new Size(55, 19);
            label19.TabIndex = 3;
            label19.Text = "Interval";
            // 
            // cmbRecordEvery
            // 
            cmbRecordEvery.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRecordEvery.FormattingEnabled = true;
            cmbRecordEvery.Items.AddRange(new object[] { "1 sec", "5 sec", "10 sec", "30 sec", "60 sec" });
            cmbRecordEvery.Location = new Point(116, 40);
            cmbRecordEvery.Name = "cmbRecordEvery";
            cmbRecordEvery.Size = new Size(122, 25);
            cmbRecordEvery.TabIndex = 2;
            cmbRecordEvery.SelectedIndex = 0;
            // 
            // chkRecording
            // 
            chkRecording.AutoSize = true;
            chkRecording.Location = new Point(20, 40);
            chkRecording.Name = "chkRecording";
            chkRecording.Size = new Size(89, 23);
            chkRecording.TabIndex = 1;
            chkRecording.Text = "Recording";
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
            grpLog.Size = new Size(1092, 222);
            grpLog.TabIndex = 4;
            grpLog.TabStop = false;
            grpLog.Text = "Message (HEX Log)";
            // 
            // btnSaveLog
            // 
            btnSaveLog.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveLog.Location = new Point(1006, 17);
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
            btnLogClear.Location = new Point(920, 17);
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
            txtLog.Size = new Size(1075, 160);
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
            lblTxSlaveAddress.Location = new Point(12, 27);
            lblTxSlaveAddress.Name = "lblTxSlaveAddress";
            lblTxSlaveAddress.Size = new Size(93, 19);
            lblTxSlaveAddress.TabIndex = 98;
            lblTxSlaveAddress.Text = "Slave Address";
            // 
            // numSlave
            // 
            numSlave.Location = new Point(109, 27);
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
            lblTxFunctionCode.Location = new Point(7, 57);
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
            cmbFunctionCode.Location = new Point(109, 57);
            cmbFunctionCode.Name = "cmbFunctionCode";
            cmbFunctionCode.Size = new Size(108, 25);
            cmbFunctionCode.TabIndex = 101;
            cmbFunctionCode.SelectedIndexChanged += cmbFunctionCode_SelectedIndexChanged;
            cmbFunctionCode.TextChanged += cmbFunctionCode_TextChanged;
            // 
            // lblTxStartRegister
            // 
            lblTxStartRegister.AutoSize = true;
            lblTxStartRegister.Location = new Point(14, 87);
            lblTxStartRegister.Name = "lblTxStartRegister";
            lblTxStartRegister.Size = new Size(91, 19);
            lblTxStartRegister.TabIndex = 102;
            lblTxStartRegister.Text = "Start Register";
            // 
            // lblTxRegisterCount
            // 
            lblTxRegisterCount.AutoSize = true;
            lblTxRegisterCount.Location = new Point(5, 117);
            lblTxRegisterCount.Name = "lblTxRegisterCount";
            lblTxRegisterCount.Size = new Size(100, 19);
            lblTxRegisterCount.TabIndex = 103;
            lblTxRegisterCount.Text = "Register Count";
            // 
            // numCount
            // 
            numCount.Location = new Point(109, 117);
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
            numStartRegister.Location = new Point(109, 87);
            numStartRegister.Name = "numStartRegister";
            numStartRegister.Size = new Size(108, 25);
            numStartRegister.TabIndex = 106;
            // 
            // lblTxDataCount
            // 
            lblTxDataCount.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTxDataCount.AutoSize = true;
            lblTxDataCount.Location = new Point(31, 147);
            lblTxDataCount.Name = "lblTxDataCount";
            lblTxDataCount.Size = new Size(76, 19);
            lblTxDataCount.TabIndex = 107;
            lblTxDataCount.Text = "DataCount";
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
            btnCalcCrc.Location = new Point(8, 272);
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
            btnSend.Location = new Point(117, 272);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(100, 32);
            btnSend.TabIndex = 110;
            btnSend.Text = "Send";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // txtDataCount
            // 
            txtDataCount.Location = new Point(109, 147);
            txtDataCount.Name = "txtDataCount";
            txtDataCount.Size = new Size(108, 25);
            txtDataCount.TabIndex = 112;
            // 
            // txtCrc
            // 
            txtCrc.Location = new Point(109, 177);
            txtCrc.Name = "txtCrc";
            txtCrc.Size = new Size(108, 25);
            txtCrc.TabIndex = 111;
            // 
            // lblPreset
            // 
            lblPreset.AutoSize = true;
            lblPreset.Location = new Point(5, 207);
            lblPreset.Name = "lblPreset";
            lblPreset.Size = new Size(47, 19);
            lblPreset.TabIndex = 114;
            lblPreset.Text = "Preset";
            // 
            // cmbPreset
            // 
            cmbPreset.FormattingEnabled = true;
            cmbPreset.Location = new Point(57, 207);
            cmbPreset.Name = "cmbPreset";
            cmbPreset.Size = new Size(160, 25);
            cmbPreset.TabIndex = 115;
            cmbPreset.SelectedIndexChanged += cmbPreset_SelectedIndexChanged;
            // 
            // btnPresetDelete
            // 
            btnPresetDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnPresetDelete.Location = new Point(117, 237);
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
            btnPresetSave.Location = new Point(8, 237);
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
            btnTxClear.Location = new Point(8, 307);
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
            btnRevert.Location = new Point(117, 307);
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
            grpTx.Controls.Add(btnRevert);
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
            grpTx.Location = new Point(12, 12);
            grpTx.Name = "grpTx";
            grpTx.Size = new Size(665, 420);
            grpTx.TabIndex = 0;
            grpTx.TabStop = false;
            grpTx.Text = "Transmit";
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(1385, 672);
            Controls.Add(grpTx);
            Controls.Add(grpLog);
            Controls.Add(grpOpt);
            Controls.Add(grpRx);
            Font = new Font("Segoe UI", 10F);
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
        private DataGridViewTextBoxColumn colRxReg;
        private DataGridViewTextBoxColumn colRxNick;
        private DataGridViewTextBoxColumn colRxHex;
        private DataGridViewTextBoxColumn colRxDec;
        private DataGridViewTextBoxColumn colRxBit;
        private DataGridViewCheckBoxColumn colRxQuickView;
    }
}
