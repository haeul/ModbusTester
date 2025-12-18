using System;
using System.Windows.Forms;

namespace ModbusTester
{
    partial class FormMacroSetting : Form
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.SplitContainer splitMain;

        // Left
        private System.Windows.Forms.GroupBox grpMacroList;
        private System.Windows.Forms.ListBox lstMacros;
        private System.Windows.Forms.Panel pnlMacroButtons;
        private System.Windows.Forms.Button btnMacroNew;
        private System.Windows.Forms.Button btnMacroDelete;

        // Right - Top
        private System.Windows.Forms.GroupBox grpMacroInfo;
        private System.Windows.Forms.Label lblMacroName;
        private System.Windows.Forms.TextBox txtMacroName;
        private System.Windows.Forms.Label lblRepeat;
        private System.Windows.Forms.NumericUpDown nudRepeat;
        private System.Windows.Forms.Button btnMacroSave;

        // Right - Middle
        private System.Windows.Forms.GroupBox grpSteps;
        private System.Windows.Forms.DataGridView dgvSteps;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStep;
        private System.Windows.Forms.DataGridViewComboBoxColumn colPreset; // ✅ ComboBox로 변경
        private System.Windows.Forms.DataGridViewTextBoxColumn colDelayMs;

        private System.Windows.Forms.Panel pnlStepButtons;
        private System.Windows.Forms.Button btnStepAdd;
        private System.Windows.Forms.Button btnStepRemove;
        private System.Windows.Forms.Button btnStepUp;
        private System.Windows.Forms.Button btnStepDown;

        // Right - Bottom
        private System.Windows.Forms.GroupBox grpRun;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ProgressBar progressStep;

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
            btnMacroNew = new Button();
            btnMacroDelete = new Button();
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
            grpRun = new GroupBox();
            btnStart = new Button();
            btnStop = new Button();
            lblStatus = new Label();
            progressStep = new ProgressBar();
            grpMacroInfo = new GroupBox();
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
            grpSteps.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvSteps).BeginInit();
            pnlStepButtons.SuspendLayout();
            grpRun.SuspendLayout();
            grpMacroInfo.SuspendLayout();
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
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(grpSteps);
            splitMain.Panel2.Controls.Add(grpRun);
            splitMain.Panel2.Controls.Add(grpMacroInfo);
            splitMain.Size = new Size(931, 600);
            splitMain.SplitterDistance = 201;
            splitMain.TabIndex = 0;
            // 
            // grpMacroList
            // 
            grpMacroList.Controls.Add(lstMacros);
            grpMacroList.Controls.Add(pnlMacroButtons);
            grpMacroList.Location = new Point(10, 0);
            grpMacroList.Name = "grpMacroList";
            grpMacroList.Padding = new Padding(10);
            grpMacroList.Size = new Size(188, 590);
            grpMacroList.TabIndex = 0;
            grpMacroList.TabStop = false;
            grpMacroList.Text = "Macro";
            // 
            // lstMacros
            // 
            lstMacros.Dock = DockStyle.Fill;
            lstMacros.FormattingEnabled = true;
            lstMacros.IntegralHeight = false;
            lstMacros.ItemHeight = 15;
            lstMacros.Location = new Point(10, 26);
            lstMacros.Name = "lstMacros";
            lstMacros.Size = new Size(168, 506);
            lstMacros.TabIndex = 0;
            // 
            // pnlMacroButtons
            // 
            pnlMacroButtons.Controls.Add(btnMacroNew);
            pnlMacroButtons.Controls.Add(btnMacroDelete);
            pnlMacroButtons.Dock = DockStyle.Bottom;
            pnlMacroButtons.Location = new Point(10, 532);
            pnlMacroButtons.Name = "pnlMacroButtons";
            pnlMacroButtons.Size = new Size(168, 48);
            pnlMacroButtons.TabIndex = 1;
            // 
            // btnMacroNew
            // 
            btnMacroNew.Location = new Point(2, 10);
            btnMacroNew.Name = "btnMacroNew";
            btnMacroNew.Size = new Size(80, 26);
            btnMacroNew.TabIndex = 0;
            btnMacroNew.Text = "New";
            btnMacroNew.UseVisualStyleBackColor = true;
            // 
            // btnMacroDelete
            // 
            btnMacroDelete.Location = new Point(88, 10);
            btnMacroDelete.Name = "btnMacroDelete";
            btnMacroDelete.Size = new Size(80, 26);
            btnMacroDelete.TabIndex = 1;
            btnMacroDelete.Text = "Delete";
            btnMacroDelete.UseVisualStyleBackColor = true;
            // 
            // grpSteps
            // 
            grpSteps.Controls.Add(dgvSteps);
            grpSteps.Controls.Add(pnlStepButtons);
            grpSteps.Location = new Point(0, 60);
            grpSteps.Name = "grpSteps";
            grpSteps.Padding = new Padding(10);
            grpSteps.Size = new Size(715, 470);
            grpSteps.TabIndex = 1;
            grpSteps.TabStop = false;
            grpSteps.Text = "Steps";
            // 
            // dgvSteps
            // 
            dgvSteps.AllowUserToAddRows = false;
            dgvSteps.AllowUserToDeleteRows = false;
            dgvSteps.AllowUserToResizeRows = false;
            dgvSteps.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvSteps.Columns.AddRange(new DataGridViewColumn[] { colStep, colPreset, colDelayMs });
            dgvSteps.Dock = DockStyle.Fill;
            dgvSteps.Location = new Point(10, 26);
            dgvSteps.MultiSelect = false;
            dgvSteps.Name = "dgvSteps";
            dgvSteps.RowHeadersVisible = false;
            dgvSteps.RowTemplate.Height = 22;
            dgvSteps.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvSteps.Size = new Size(695, 398);
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
            colDelayMs.HeaderText = "Delay(ms)";
            colDelayMs.Name = "colDelayMs";
            colDelayMs.Width = 90;
            // 
            // pnlStepButtons
            // 
            pnlStepButtons.Controls.Add(btnStepAdd);
            pnlStepButtons.Controls.Add(btnStepRemove);
            pnlStepButtons.Controls.Add(btnStepUp);
            pnlStepButtons.Controls.Add(btnStepDown);
            pnlStepButtons.Dock = DockStyle.Bottom;
            pnlStepButtons.Location = new Point(10, 424);
            pnlStepButtons.Name = "pnlStepButtons";
            pnlStepButtons.Size = new Size(695, 36);
            pnlStepButtons.TabIndex = 1;
            // 
            // btnStepAdd
            // 
            btnStepAdd.Location = new Point(0, 5);
            btnStepAdd.Name = "btnStepAdd";
            btnStepAdd.Size = new Size(40, 26);
            btnStepAdd.TabIndex = 0;
            btnStepAdd.Text = "+";
            btnStepAdd.UseVisualStyleBackColor = true;
            // 
            // btnStepRemove
            // 
            btnStepRemove.Location = new Point(46, 5);
            btnStepRemove.Name = "btnStepRemove";
            btnStepRemove.Size = new Size(40, 26);
            btnStepRemove.TabIndex = 1;
            btnStepRemove.Text = "-";
            btnStepRemove.UseVisualStyleBackColor = true;
            // 
            // btnStepUp
            // 
            btnStepUp.Location = new Point(96, 5);
            btnStepUp.Name = "btnStepUp";
            btnStepUp.Size = new Size(40, 26);
            btnStepUp.TabIndex = 2;
            btnStepUp.Text = "▲";
            btnStepUp.UseVisualStyleBackColor = true;
            // 
            // btnStepDown
            // 
            btnStepDown.Location = new Point(142, 5);
            btnStepDown.Name = "btnStepDown";
            btnStepDown.Size = new Size(40, 26);
            btnStepDown.TabIndex = 3;
            btnStepDown.Text = "▼";
            btnStepDown.UseVisualStyleBackColor = true;
            // 
            // grpRun
            // 
            grpRun.Controls.Add(btnStart);
            grpRun.Controls.Add(btnStop);
            grpRun.Controls.Add(lblStatus);
            grpRun.Controls.Add(progressStep);
            grpRun.Location = new Point(0, 530);
            grpRun.Name = "grpRun";
            grpRun.Padding = new Padding(10);
            grpRun.Size = new Size(715, 60);
            grpRun.TabIndex = 2;
            grpRun.TabStop = false;
            grpRun.Text = "Run";
            // 
            // btnStart
            // 
            btnStart.Location = new Point(14, 26);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(80, 26);
            btnStart.TabIndex = 0;
            btnStart.Text = "Start";
            btnStart.UseVisualStyleBackColor = true;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(98, 26);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(80, 26);
            btnStop.TabIndex = 1;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(214, 12);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(66, 15);
            lblStatus.TabIndex = 2;
            lblStatus.Text = "Status: Idle";
            // 
            // progressStep
            // 
            progressStep.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            progressStep.Location = new Point(214, 32);
            progressStep.Name = "progressStep";
            progressStep.Size = new Size(489, 12);
            progressStep.TabIndex = 3;
            // 
            // grpMacroInfo
            // 
            grpMacroInfo.Controls.Add(lblMacroName);
            grpMacroInfo.Controls.Add(txtMacroName);
            grpMacroInfo.Controls.Add(lblRepeat);
            grpMacroInfo.Controls.Add(nudRepeat);
            grpMacroInfo.Controls.Add(btnMacroSave);
            grpMacroInfo.Location = new Point(0, 0);
            grpMacroInfo.Name = "grpMacroInfo";
            grpMacroInfo.Padding = new Padding(10);
            grpMacroInfo.Size = new Size(715, 60);
            grpMacroInfo.TabIndex = 0;
            grpMacroInfo.TabStop = false;
            grpMacroInfo.Text = "Macro";
            // 
            // lblMacroName
            // 
            lblMacroName.AutoSize = true;
            lblMacroName.Location = new Point(14, 26);
            lblMacroName.Name = "lblMacroName";
            lblMacroName.Size = new Size(39, 15);
            lblMacroName.TabIndex = 0;
            lblMacroName.Text = "Name";
            // 
            // txtMacroName
            // 
            txtMacroName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtMacroName.Location = new Point(64, 22);
            txtMacroName.Name = "txtMacroName";
            txtMacroName.Size = new Size(399, 23);
            txtMacroName.TabIndex = 1;
            // 
            // lblRepeat
            // 
            lblRepeat.AutoSize = true;
            lblRepeat.Location = new Point(478, 26);
            lblRepeat.Name = "lblRepeat";
            lblRepeat.Size = new Size(43, 15);
            lblRepeat.TabIndex = 2;
            lblRepeat.Text = "Repeat";
            // 
            // nudRepeat
            // 
            nudRepeat.Location = new Point(528, 22);
            nudRepeat.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            nudRepeat.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudRepeat.Name = "nudRepeat";
            nudRepeat.Size = new Size(70, 23);
            nudRepeat.TabIndex = 3;
            nudRepeat.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // btnMacroSave
            // 
            btnMacroSave.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnMacroSave.Location = new Point(613, 20);
            btnMacroSave.Name = "btnMacroSave";
            btnMacroSave.Size = new Size(90, 26);
            btnMacroSave.TabIndex = 4;
            btnMacroSave.Text = "Save";
            btnMacroSave.UseVisualStyleBackColor = true;
            // 
            // FormMacroSetting
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(931, 600);
            Controls.Add(splitMain);
            KeyPreview = true;
            MinimumSize = new Size(900, 560);
            Name = "FormMacroSetting";
            Text = "Macro Setting";
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            grpMacroList.ResumeLayout(false);
            pnlMacroButtons.ResumeLayout(false);
            grpSteps.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvSteps).EndInit();
            pnlStepButtons.ResumeLayout(false);
            grpRun.ResumeLayout(false);
            grpRun.PerformLayout();
            grpMacroInfo.ResumeLayout(false);
            grpMacroInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudRepeat).EndInit();
            ResumeLayout(false);
        }

        #endregion
    }
}
