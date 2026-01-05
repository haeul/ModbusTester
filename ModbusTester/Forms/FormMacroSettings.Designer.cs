using System;
using System.Drawing;
using System.Windows.Forms;

namespace ModbusTester
{
    partial class FormMacroSetting : Form
    {
        private System.ComponentModel.IContainer components = null;

        private SplitContainer splitMain;

        // Left
        private GroupBox grpMacroList;
        private ListBox lstMacros;
        private Panel pnlMacroButtons;

        // TableLayout for macro bottom buttons
        private TableLayoutPanel tlpMacroButtons;

        private Button btnMacroNew;
        private Button btnMacroDelete;

        // Right root
        private Panel pnlRight;

        // Right - Top
        private GroupBox grpMacroInfo;

        // FIX: table layout for Macro Settings row
        private TableLayoutPanel tlpMacroInfo;

        private Label lblMacroName;
        private TextBox txtMacroName;
        private Label lblRepeat;
        private NumericUpDown nudRepeat;
        private Button btnMacroSave;

        // Right - Middle
        private GroupBox grpSteps;
        private DataGridView dgvSteps;
        private DataGridViewTextBoxColumn colStep;
        private DataGridViewComboBoxColumn colPreset;
        private DataGridViewTextBoxColumn colDelayMs;
        private Panel pnlStepButtons;
        private Button btnStepAdd;
        private Button btnStepRemove;
        private Button btnStepUp;
        private Button btnStepDown;

        // Right - Bottom
        private GroupBox grpRuntime;
        private DataGridView dgvInstances;

        // ★ NEW: 체크박스 컬럼
        private DataGridViewCheckBoxColumn colInstChk;

        private DataGridViewTextBoxColumn colInstId;
        private DataGridViewTextBoxColumn colInstMacro;
        private DataGridViewTextBoxColumn colInstStatus;
        private DataGridViewTextBoxColumn colInstRepeat;
        private DataGridViewTextBoxColumn colInstStep;
        private DataGridViewTextBoxColumn colInstNext;
        private DataGridViewTextBoxColumn colInstLast;

        private Panel pnlRuntimeButtons;

        // TableLayout for runtime bottom buttons
        private TableLayoutPanel tlpRuntimeButtons;

        private Button btnInstStart;
        private Button btnInstStop;
        private Button btnInstPauseResume;
        private Button btnInstPauseAll;
        private Button btnInstStopAll;

        // 기존 Clear Select / Clear Done 제거 → Clear 하나로 통합
        private Button btnInstClear;

        private Button btnInstClearAll;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            splitMain = new SplitContainer();
            grpMacroList = new GroupBox();
            lstMacros = new ListBox();
            pnlMacroButtons = new Panel();
            tlpMacroButtons = new TableLayoutPanel();
            btnMacroNew = new Button();
            btnMacroDelete = new Button();

            pnlRight = new Panel();
            grpSteps = new GroupBox();
            dgvSteps = new DataGridView();
            colStep = new DataGridViewTextBoxColumn();
            colPreset = new DataGridViewComboBoxColumn();
            colDelayMs = new DataGridViewTextBoxColumn();
            pnlStepButtons = new Panel();
            btnStepAdd = new Button();
            btnStepRemove = new Button();
            btnStepUp = new Button();
            btnStepDown = new Button();

            grpRuntime = new GroupBox();
            dgvInstances = new DataGridView();

            // ★ NEW: checkbox column
            colInstChk = new DataGridViewCheckBoxColumn();

            colInstId = new DataGridViewTextBoxColumn();
            colInstMacro = new DataGridViewTextBoxColumn();
            colInstStatus = new DataGridViewTextBoxColumn();
            colInstRepeat = new DataGridViewTextBoxColumn();
            colInstStep = new DataGridViewTextBoxColumn();
            colInstNext = new DataGridViewTextBoxColumn();
            colInstLast = new DataGridViewTextBoxColumn();

            pnlRuntimeButtons = new Panel();
            tlpRuntimeButtons = new TableLayoutPanel();
            btnInstStart = new Button();
            btnInstStop = new Button();
            btnInstPauseResume = new Button();
            btnInstPauseAll = new Button();
            btnInstStopAll = new Button();
            btnInstClear = new Button();
            btnInstClearAll = new Button();

            grpMacroInfo = new GroupBox();
            tlpMacroInfo = new TableLayoutPanel();
            lblMacroName = new Label();
            txtMacroName = new TextBox();
            lblRepeat = new Label();
            nudRepeat = new NumericUpDown();
            btnMacroSave = new Button();

            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();

            grpMacroList.SuspendLayout();
            pnlMacroButtons.SuspendLayout();
            tlpMacroButtons.SuspendLayout();

            pnlRight.SuspendLayout();

            grpSteps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSteps).BeginInit();
            pnlStepButtons.SuspendLayout();

            grpRuntime.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvInstances).BeginInit();
            pnlRuntimeButtons.SuspendLayout();
            tlpRuntimeButtons.SuspendLayout();

            grpMacroInfo.SuspendLayout();
            tlpMacroInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudRepeat).BeginInit();

            SuspendLayout();

            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.FixedPanel = FixedPanel.Panel1;
            splitMain.Location = new Point(0, 0);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(grpMacroList);
            splitMain.Panel1.Padding = new Padding(8, 8, 0, 8);
            splitMain.Panel1MinSize = 180;
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(pnlRight);
            splitMain.Panel2.Padding = new Padding(0, 8, 8, 8);
            splitMain.Panel2MinSize = 300;
            splitMain.Size = new Size(931, 600);
            splitMain.SplitterDistance = 180;
            splitMain.SplitterWidth = 6;
            splitMain.TabIndex = 0;

            // 
            // grpMacroList
            // 
            grpMacroList.Controls.Add(lstMacros);
            grpMacroList.Controls.Add(pnlMacroButtons);
            grpMacroList.Dock = DockStyle.Fill;
            grpMacroList.Location = new Point(8, 8);
            grpMacroList.Name = "grpMacroList";
            grpMacroList.Padding = new Padding(10, 10, 10, 5);
            grpMacroList.Size = new Size(172, 584);
            grpMacroList.TabIndex = 0;
            grpMacroList.TabStop = false;
            grpMacroList.Text = "Macro List";

            // 
            // lstMacros
            // 
            lstMacros.Dock = DockStyle.Fill;
            lstMacros.IntegralHeight = false;
            lstMacros.ItemHeight = 17;
            lstMacros.Location = new Point(10, 28);
            lstMacros.Name = "lstMacros";
            lstMacros.Size = new Size(152, 507);
            lstMacros.TabIndex = 0;

            // 
            // pnlMacroButtons
            // 
            pnlMacroButtons.Controls.Add(tlpMacroButtons);
            pnlMacroButtons.Dock = DockStyle.Bottom;
            pnlMacroButtons.Location = new Point(10, 535);
            pnlMacroButtons.Name = "pnlMacroButtons";
            pnlMacroButtons.Size = new Size(152, 44);
            pnlMacroButtons.TabIndex = 1;

            // 
            // tlpMacroButtons
            // 
            tlpMacroButtons.ColumnCount = 2;
            tlpMacroButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpMacroButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpMacroButtons.Controls.Add(btnMacroNew, 0, 0);
            tlpMacroButtons.Controls.Add(btnMacroDelete, 1, 0);
            tlpMacroButtons.Dock = DockStyle.Fill;
            tlpMacroButtons.Location = new Point(0, 0);
            tlpMacroButtons.Name = "tlpMacroButtons";
            tlpMacroButtons.Padding = new Padding(0, 8, 0, 8);
            tlpMacroButtons.RowCount = 1;
            tlpMacroButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpMacroButtons.Size = new Size(152, 44);
            tlpMacroButtons.TabIndex = 0;

            // 
            // btnMacroNew
            // 
            btnMacroNew.Dock = DockStyle.Fill;
            btnMacroNew.Location = new Point(0, 8);
            btnMacroNew.Margin = new Padding(0, 0, 2, 0);
            btnMacroNew.Name = "btnMacroNew";
            btnMacroNew.Size = new Size(70, 28);
            btnMacroNew.TabIndex = 0;
            btnMacroNew.Text = "New";
            btnMacroNew.UseVisualStyleBackColor = true;

            // 
            // btnMacroDelete
            // 
            btnMacroDelete.Dock = DockStyle.Fill;
            btnMacroDelete.Location = new Point(82, 8);
            btnMacroDelete.Margin = new Padding(2, 0, 0, 0);
            btnMacroDelete.Name = "btnMacroDelete";
            btnMacroDelete.Size = new Size(70, 28);
            btnMacroDelete.TabIndex = 1;
            btnMacroDelete.Text = "Delete";
            btnMacroDelete.UseVisualStyleBackColor = true;

            // 
            // pnlRight
            // 
            pnlRight.Controls.Add(grpSteps);
            pnlRight.Controls.Add(grpRuntime);
            pnlRight.Controls.Add(grpMacroInfo);
            pnlRight.Dock = DockStyle.Fill;
            pnlRight.Location = new Point(0, 8);
            pnlRight.Name = "pnlRight";
            pnlRight.Size = new Size(737, 584);
            pnlRight.TabIndex = 0;

            // 
            // grpMacroInfo
            // 
            grpMacroInfo.Controls.Add(tlpMacroInfo);
            grpMacroInfo.Dock = DockStyle.Top;
            grpMacroInfo.Location = new Point(0, 0);
            grpMacroInfo.Name = "grpMacroInfo";
            grpMacroInfo.Padding = new Padding(10);
            grpMacroInfo.Size = new Size(737, 68);
            grpMacroInfo.TabIndex = 2;
            grpMacroInfo.TabStop = false;
            grpMacroInfo.Text = "Macro Settings";

            // 
            // tlpMacroInfo
            // 
            tlpMacroInfo.ColumnCount = 5;
            tlpMacroInfo.ColumnStyles.Add(new ColumnStyle());
            tlpMacroInfo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMacroInfo.ColumnStyles.Add(new ColumnStyle());
            tlpMacroInfo.ColumnStyles.Add(new ColumnStyle());
            tlpMacroInfo.ColumnStyles.Add(new ColumnStyle());
            tlpMacroInfo.Controls.Add(lblMacroName, 0, 0);
            tlpMacroInfo.Controls.Add(txtMacroName, 1, 0);
            tlpMacroInfo.Controls.Add(lblRepeat, 2, 0);
            tlpMacroInfo.Controls.Add(nudRepeat, 3, 0);
            tlpMacroInfo.Controls.Add(btnMacroSave, 4, 0);
            tlpMacroInfo.Dock = DockStyle.Fill;
            tlpMacroInfo.Location = new Point(10, 28);
            tlpMacroInfo.Name = "tlpMacroInfo";
            tlpMacroInfo.RowCount = 1;
            tlpMacroInfo.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
            tlpMacroInfo.Size = new Size(717, 30);
            tlpMacroInfo.TabIndex = 0;

            // 
            // lblMacroName
            // 
            lblMacroName.AutoSize = true;
            lblMacroName.Dock = DockStyle.Fill;
            lblMacroName.Location = new Point(0, 2);
            lblMacroName.Margin = new Padding(0, 2, 10, 0);
            lblMacroName.Name = "lblMacroName";
            lblMacroName.Size = new Size(45, 28);
            lblMacroName.TabIndex = 0;
            lblMacroName.Text = "Name";
            lblMacroName.TextAlign = ContentAlignment.MiddleLeft;

            // 
            // txtMacroName
            // 
            txtMacroName.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtMacroName.Location = new Point(55, 3);
            txtMacroName.Margin = new Padding(0, 2, 14, 0);
            txtMacroName.Name = "txtMacroName";
            txtMacroName.Size = new Size(373, 25);
            txtMacroName.TabIndex = 1;

            // 
            // lblRepeat
            // 
            lblRepeat.AutoSize = true;
            lblRepeat.Dock = DockStyle.Fill;
            lblRepeat.Location = new Point(442, 2);
            lblRepeat.Margin = new Padding(0, 2, 10, 0);
            lblRepeat.Name = "lblRepeat";
            lblRepeat.Size = new Size(51, 28);
            lblRepeat.TabIndex = 2;
            lblRepeat.Text = "Repeat";
            lblRepeat.TextAlign = ContentAlignment.MiddleLeft;

            // 
            // nudRepeat
            // 
            nudRepeat.Anchor = AnchorStyles.Left;
            nudRepeat.Location = new Point(503, 3);
            nudRepeat.Margin = new Padding(0, 2, 14, 0);
            nudRepeat.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            nudRepeat.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudRepeat.Name = "nudRepeat";
            nudRepeat.Size = new Size(90, 25);
            nudRepeat.TabIndex = 3;
            nudRepeat.Value = new decimal(new int[] { 1, 0, 0, 0 });

            // 
            // btnMacroSave
            // 
            btnMacroSave.Anchor = AnchorStyles.Right;
            btnMacroSave.Location = new Point(607, 1);
            btnMacroSave.Margin = new Padding(0);
            btnMacroSave.Name = "btnMacroSave";
            btnMacroSave.Size = new Size(110, 28);
            btnMacroSave.TabIndex = 4;
            btnMacroSave.Text = "Save";
            btnMacroSave.UseVisualStyleBackColor = true;

            // 
            // grpSteps
            // 
            grpSteps.Controls.Add(dgvSteps);
            grpSteps.Controls.Add(pnlStepButtons);
            grpSteps.Dock = DockStyle.Fill;
            grpSteps.Location = new Point(0, 68);
            grpSteps.Name = "grpSteps";
            grpSteps.Padding = new Padding(10, 10, 10, 4);
            grpSteps.Size = new Size(737, 264);
            grpSteps.TabIndex = 0;
            grpSteps.TabStop = false;
            grpSteps.Text = "Steps";

            // 
            // dgvSteps
            // 
            dgvSteps.AllowUserToAddRows = false;
            dgvSteps.AllowUserToDeleteRows = false;
            dgvSteps.AllowUserToResizeRows = false;
            dgvSteps.ColumnHeadersHeight = 32;
            dgvSteps.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvSteps.Columns.AddRange(new DataGridViewColumn[] { colStep, colPreset, colDelayMs });
            dgvSteps.Dock = DockStyle.Fill;
            dgvSteps.Location = new Point(10, 28);
            dgvSteps.MultiSelect = false;
            dgvSteps.Name = "dgvSteps";
            dgvSteps.RowHeadersVisible = false;
            dgvSteps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSteps.Size = new Size(717, 190);
            dgvSteps.TabIndex = 0;

            // 
            // colStep
            // 
            colStep.HeaderText = "Step";
            colStep.Name = "colStep";
            colStep.ReadOnly = true;
            colStep.Width = 60;

            // 
            // colPreset
            // 
            colPreset.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colPreset.DropDownWidth = 220;
            colPreset.FlatStyle = FlatStyle.Flat;
            colPreset.HeaderText = "Preset";
            colPreset.Name = "colPreset";

            // 
            // colDelayMs
            // 
            colDelayMs.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            colDelayMs.HeaderText = "Delay(ms)";
            colDelayMs.MinimumWidth = 120;
            colDelayMs.Name = "colDelayMs";
            colDelayMs.Width = 120;

            // 
            // pnlStepButtons
            // 
            pnlStepButtons.Controls.Add(btnStepAdd);
            pnlStepButtons.Controls.Add(btnStepRemove);
            pnlStepButtons.Controls.Add(btnStepUp);
            pnlStepButtons.Controls.Add(btnStepDown);
            pnlStepButtons.Dock = DockStyle.Bottom;
            pnlStepButtons.Location = new Point(10, 218);
            pnlStepButtons.Name = "pnlStepButtons";
            pnlStepButtons.Size = new Size(717, 42);
            pnlStepButtons.TabIndex = 1;

            // 
            // btnStepAdd
            // 
            btnStepAdd.Location = new Point(0, 8);
            btnStepAdd.Name = "btnStepAdd";
            btnStepAdd.Size = new Size(60, 26);
            btnStepAdd.TabIndex = 0;
            btnStepAdd.Text = "+";
            btnStepAdd.UseVisualStyleBackColor = true;

            // 
            // btnStepRemove
            // 
            btnStepRemove.Location = new Point(66, 8);
            btnStepRemove.Name = "btnStepRemove";
            btnStepRemove.Size = new Size(60, 26);
            btnStepRemove.TabIndex = 1;
            btnStepRemove.Text = "-";
            btnStepRemove.UseVisualStyleBackColor = true;

            // 
            // btnStepUp
            // 
            btnStepUp.Location = new Point(132, 8);
            btnStepUp.Name = "btnStepUp";
            btnStepUp.Size = new Size(60, 26);
            btnStepUp.TabIndex = 2;
            btnStepUp.Text = "▲";
            btnStepUp.UseVisualStyleBackColor = true;

            // 
            // btnStepDown
            // 
            btnStepDown.Location = new Point(198, 8);
            btnStepDown.Name = "btnStepDown";
            btnStepDown.Size = new Size(60, 26);
            btnStepDown.TabIndex = 3;
            btnStepDown.Text = "▼";
            btnStepDown.UseVisualStyleBackColor = true;

            // 
            // grpRuntime
            // 
            grpRuntime.Controls.Add(dgvInstances);
            grpRuntime.Controls.Add(pnlRuntimeButtons);
            grpRuntime.Dock = DockStyle.Bottom;
            grpRuntime.Location = new Point(0, 332);
            grpRuntime.Name = "grpRuntime";
            grpRuntime.Padding = new Padding(10, 10, 10, 4);
            grpRuntime.Size = new Size(737, 252);
            grpRuntime.TabIndex = 1;
            grpRuntime.TabStop = false;
            grpRuntime.Text = "Runtime / Execution";

            // 
            // dgvInstances
            // 
            dgvInstances.AllowUserToAddRows = false;
            dgvInstances.AllowUserToDeleteRows = false;
            dgvInstances.AllowUserToResizeRows = false;
            dgvInstances.ColumnHeadersHeight = 32;
            dgvInstances.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

            // ★ 체크박스 컬럼이 맨 앞에 들어감
            dgvInstances.Columns.AddRange(new DataGridViewColumn[]
            {
                colInstChk,
                colInstId, colInstMacro, colInstStatus, colInstRepeat, colInstStep, colInstNext, colInstLast
            });

            dgvInstances.Dock = DockStyle.Fill;
            dgvInstances.Location = new Point(10, 28);
            dgvInstances.Name = "dgvInstances";
            dgvInstances.RowHeadersVisible = false;
            dgvInstances.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInstances.Size = new Size(717, 176);
            dgvInstances.TabIndex = 0;

            // 
            // colInstChk
            // 
            colInstChk.HeaderText = "";
            colInstChk.Name = "colInstChk";
            colInstChk.Width = 36;
            colInstChk.Resizable = DataGridViewTriState.False;

            // 
            // colInstId
            // 
            colInstId.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            colInstId.HeaderText = "ID";
            colInstId.Name = "colInstId";
            colInstId.ReadOnly = true;
            colInstId.Width = 48;

            // 
            // colInstMacro
            // 
            colInstMacro.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            colInstMacro.HeaderText = "Macro";
            colInstMacro.MinimumWidth = 120;
            colInstMacro.Name = "colInstMacro";
            colInstMacro.ReadOnly = true;
            colInstMacro.Width = 120;

            // 
            // colInstStatus
            // 
            colInstStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            colInstStatus.HeaderText = "Status";
            colInstStatus.Name = "colInstStatus";
            colInstStatus.ReadOnly = true;
            colInstStatus.Width = 72;

            // 
            // colInstRepeat
            // 
            colInstRepeat.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            colInstRepeat.HeaderText = "Repeat";
            colInstRepeat.Name = "colInstRepeat";
            colInstRepeat.ReadOnly = true;
            colInstRepeat.Width = 76;

            // 
            // colInstStep
            // 
            colInstStep.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            colInstStep.HeaderText = "Step";
            colInstStep.Name = "colInstStep";
            colInstStep.ReadOnly = true;
            colInstStep.Width = 61;

            // 
            // colInstNext
            // 
            colInstNext.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            colInstNext.HeaderText = "Next(ms)";
            colInstNext.MinimumWidth = 90;
            colInstNext.Name = "colInstNext";
            colInstNext.ReadOnly = true;
            colInstNext.Width = 90;

            // 
            // colInstLast
            // 
            colInstLast.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colInstLast.HeaderText = "Last Result";
            colInstLast.Name = "colInstLast";
            colInstLast.ReadOnly = true;

            // 
            // pnlRuntimeButtons
            // 
            pnlRuntimeButtons.Controls.Add(tlpRuntimeButtons);
            pnlRuntimeButtons.Dock = DockStyle.Bottom;
            pnlRuntimeButtons.Location = new Point(10, 204);
            pnlRuntimeButtons.Name = "pnlRuntimeButtons";
            pnlRuntimeButtons.Size = new Size(717, 44);
            pnlRuntimeButtons.TabIndex = 1;

            // 
            // tlpRuntimeButtons (7 columns)
            // Start / Stop / Pause / Pause All / Stop All / Clear / Clear All
            // 
            tlpRuntimeButtons.ColumnCount = 7;
            tlpRuntimeButtons.ColumnStyles.Clear();

            tlpRuntimeButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F)); // Start
            tlpRuntimeButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F)); // Stop
            tlpRuntimeButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 12F)); // Pause
            tlpRuntimeButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F)); // Pause All
            tlpRuntimeButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F)); // Stop All
            tlpRuntimeButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F)); // Clear
            tlpRuntimeButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16F)); // Clear All

            tlpRuntimeButtons.Dock = DockStyle.Fill;
            tlpRuntimeButtons.Location = new Point(0, 0);
            tlpRuntimeButtons.Name = "tlpRuntimeButtons";
            tlpRuntimeButtons.Padding = new Padding(0, 8, 0, 8);
            tlpRuntimeButtons.RowCount = 1;
            tlpRuntimeButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tlpRuntimeButtons.Size = new Size(717, 44);
            tlpRuntimeButtons.TabIndex = 0;

            // 
            // btnInstStart
            // 
            btnInstStart.Dock = DockStyle.Fill;
            btnInstStart.Location = new Point(0, 8);
            btnInstStart.Margin = new Padding(0, 0, 6, 0);
            btnInstStart.Name = "btnInstStart";
            btnInstStart.Size = new Size(83, 28);
            btnInstStart.TabIndex = 0;
            btnInstStart.Text = "Start";
            btnInstStart.UseVisualStyleBackColor = true;

            // 
            // btnInstStop
            // 
            btnInstStop.Dock = DockStyle.Fill;
            btnInstStop.Location = new Point(89, 8);
            btnInstStop.Margin = new Padding(0, 0, 6, 0);
            btnInstStop.Name = "btnInstStop";
            btnInstStop.Size = new Size(83, 28);
            btnInstStop.TabIndex = 1;
            btnInstStop.Text = "Stop";
            btnInstStop.UseVisualStyleBackColor = true;

            // 
            // btnInstPauseResume
            // 
            btnInstPauseResume.Dock = DockStyle.Fill;
            btnInstPauseResume.Location = new Point(178, 8);
            btnInstPauseResume.Margin = new Padding(0, 0, 6, 0);
            btnInstPauseResume.Name = "btnInstPauseResume";
            btnInstPauseResume.Size = new Size(83, 28);
            btnInstPauseResume.TabIndex = 2;
            btnInstPauseResume.Text = "Pause";
            btnInstPauseResume.UseVisualStyleBackColor = true;

            // 
            // btnInstPauseAll
            // 
            btnInstPauseAll.Dock = DockStyle.Fill;
            btnInstPauseAll.Location = new Point(267, 8);
            btnInstPauseAll.Margin = new Padding(0, 0, 6, 0);
            btnInstPauseAll.Name = "btnInstPauseAll";
            btnInstPauseAll.Size = new Size(83, 28);
            btnInstPauseAll.TabIndex = 3;
            btnInstPauseAll.Text = "Pause All";
            btnInstPauseAll.UseVisualStyleBackColor = true;

            // 
            // btnInstStopAll
            // 
            btnInstStopAll.Dock = DockStyle.Fill;
            btnInstStopAll.Location = new Point(356, 8);
            btnInstStopAll.Margin = new Padding(0, 0, 6, 0);
            btnInstStopAll.Name = "btnInstStopAll";
            btnInstStopAll.Size = new Size(83, 28);
            btnInstStopAll.TabIndex = 4;
            btnInstStopAll.Text = "Stop All";
            btnInstStopAll.UseVisualStyleBackColor = true;

            // 
            // btnInstClear
            // 
            btnInstClear.Dock = DockStyle.Fill;
            btnInstClear.Location = new Point(445, 8);
            btnInstClear.Margin = new Padding(0, 0, 6, 0);
            btnInstClear.Name = "btnInstClear";
            btnInstClear.Size = new Size(83, 28);
            btnInstClear.TabIndex = 5;
            btnInstClear.Text = "Clear";
            btnInstClear.UseVisualStyleBackColor = true;

            // 
            // btnInstClearAll
            // 
            btnInstClearAll.Dock = DockStyle.Fill;
            btnInstClearAll.Location = new Point(534, 8);
            btnInstClearAll.Margin = new Padding(0);
            btnInstClearAll.Name = "btnInstClearAll";
            btnInstClearAll.Size = new Size(94, 28);
            btnInstClearAll.TabIndex = 6;
            btnInstClearAll.Text = "Clear All";
            btnInstClearAll.UseVisualStyleBackColor = true;

            // Controls Add (7개)
            tlpRuntimeButtons.Controls.Add(btnInstStart, 0, 0);
            tlpRuntimeButtons.Controls.Add(btnInstStop, 1, 0);
            tlpRuntimeButtons.Controls.Add(btnInstPauseResume, 2, 0);
            tlpRuntimeButtons.Controls.Add(btnInstClear, 3, 0);
            tlpRuntimeButtons.Controls.Add(btnInstPauseAll, 4, 0);
            tlpRuntimeButtons.Controls.Add(btnInstStopAll, 5, 0);
            tlpRuntimeButtons.Controls.Add(btnInstClearAll, 6, 0);

            // 
            // FormMacroSetting
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(931, 600);
            Controls.Add(splitMain);
            Font = new Font("Segoe UI", 10F);
            MinimumSize = new Size(900, 560);
            Name = "FormMacroSetting";
            Text = "Macro Setting";

            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);

            grpMacroList.ResumeLayout(false);
            pnlMacroButtons.ResumeLayout(false);
            tlpMacroButtons.ResumeLayout(false);

            pnlRight.ResumeLayout(false);

            grpSteps.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvSteps).EndInit();
            pnlStepButtons.ResumeLayout(false);

            grpRuntime.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvInstances).EndInit();
            pnlRuntimeButtons.ResumeLayout(false);
            tlpRuntimeButtons.ResumeLayout(false);

            grpMacroInfo.ResumeLayout(false);
            tlpMacroInfo.ResumeLayout(false);
            tlpMacroInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudRepeat).EndInit();

            ResumeLayout(false);
        }

        #endregion
    }
}
