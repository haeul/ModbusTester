using ModbusTester.Macros;
using ModbusTester.Presets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormMacroSetting : Form
    {
        private readonly FormMain _main;

        private MacroDefinition? _current;
        private bool _dirty;

        private readonly System.Windows.Forms.Timer _runTimer = new System.Windows.Forms.Timer();
        private bool _running;
        private int _repeatLeft;
        private int _stepIndex;
        private MacroDefinition? _runningMacro;

        private bool _loadingUi;

        public FormMacroSetting(FormMain main)
        {
            InitializeComponent();

            this.KeyPreview = true;
            this.KeyDown += FormMacroSetting_KeyDown;
            _main = main;

            _runTimer.Tick += RunTimer_Tick;

            this.Load += FormMacroSetting_Load;

            lstMacros.SelectedIndexChanged += LstMacros_SelectedIndexChanged;

            btnMacroNew.Click += BtnMacroNew_Click;
            btnMacroDelete.Click += BtnMacroDelete_Click;
            btnMacroSave.Click += BtnMacroSave_Click;

            btnStepAdd.Click += BtnStepAdd_Click;
            btnStepRemove.Click += BtnStepRemove_Click;
            btnStepUp.Click += BtnStepUp_Click;
            btnStepDown.Click += BtnStepDown_Click;

            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;

            txtMacroName.TextChanged += (_, __) => MarkDirty();
            nudRepeat.ValueChanged += (_, __) => MarkDirty();

            dgvSteps.CellValueChanged += (_, __) => MarkDirty();
            dgvSteps.CurrentCellDirtyStateChanged += (_, __) =>
            {
                if (dgvSteps.IsCurrentCellDirty)
                    dgvSteps.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            dgvSteps.DataError += (_, __) => { }; // ComboBox value mismatch 방지
        }

        private void FormMacroSetting_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                // 그리드 편집 중이면 값 확정
                dgvSteps.EndEdit();

                // Save 버튼 클릭과 동일하게 처리 (저장 로직 1곳 유지)
                btnMacroSave.PerformClick();
            }
        }
        private void FormMacroSetting_Load(object? sender, EventArgs e)
        {
            // Preset 목록을 ComboBox에 주입
            InitPresetComboSource();

            MacroManager.Load();
            RefreshMacroList();

            SetStatus("Status: Idle");
            btnStop.Enabled = false;

            if (lstMacros.Items.Count > 0)
                lstMacros.SelectedIndex = 0;
        }

        // ✅ 외부에서 Preset 목록이 바뀐 경우 갱신할 수 있도록 공개
        public void RefreshPresetCombo()
        {
            InitPresetComboSource();
        }

        private void InitPresetComboSource()
        {
            if (colPreset is DataGridViewComboBoxColumn combo)
            {
                combo.Items.Clear();

                var names = FunctionPresetManager.Items
                    .Select(p => p.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .OrderBy(n => n)
                    .ToList();

                foreach (var n in names)
                    combo.Items.Add(n);
            }
        }

        // -------------------- 목록/선택 --------------------

        private void RefreshMacroList(string? selectName = null)
        {
            lstMacros.Items.Clear();
            foreach (var m in MacroManager.Items)
                lstMacros.Items.Add(m.Name);

            if (!string.IsNullOrEmpty(selectName))
            {
                for (int i = 0; i < lstMacros.Items.Count; i++)
                {
                    if ((string)lstMacros.Items[i] == selectName)
                    {
                        lstMacros.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void LstMacros_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_running) return;

            if (_dirty && _current != null)
            {
                var r = MessageBox.Show(this,
                    "저장되지 않은 변경 사항이 있습니다.\r\n선택을 변경하면 변경 사항이 사라집니다.\r\n계속할까요?",
                    "Macro",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (r != DialogResult.Yes)
                    return;
            }

            if (lstMacros.SelectedItem is not string name)
                return;

            var macro = MacroManager.Find(name);
            if (macro == null)
                return;

            LoadMacroToUi(macro);
        }

        private void LoadMacroToUi(MacroDefinition macro)
        {
            _loadingUi = true;
            try
            {
                _current = Clone(macro);
                _dirty = false;

                txtMacroName.Text = _current.Name;
                nudRepeat.Value = Math.Max(nudRepeat.Minimum,
                    Math.Min(nudRepeat.Maximum, _current.Repeat));

                dgvSteps.Rows.Clear();
                for (int i = 0; i < _current.Steps.Count; i++)
                {
                    var s = _current.Steps[i];
                    dgvSteps.Rows.Add((i + 1).ToString(), s.PresetName, s.DelayMs.ToString());
                }

                UpdateStepNumbers();
                UpdateTitleDirtyMark();
            }
            finally
            {
                _loadingUi = false;
            }
        }

        // -------------------- New/Delete/Save --------------------

        private void BtnMacroNew_Click(object? sender, EventArgs e)
        {
            if (_running) return;

            var name = MacroManager.CreateUniqueName("Macro");
            var macro = new MacroDefinition
            {
                Name = name,
                Repeat = 1,
                Steps = new List<MacroStep>()
            };

            MacroManager.AddOrUpdate(macro);
            RefreshMacroList(name);
        }

        private void BtnMacroDelete_Click(object? sender, EventArgs e)
        {
            if (_running) return;

            if (lstMacros.SelectedItem is not string name)
            {
                MessageBox.Show(this, "삭제할 Macro를 선택해 주세요.", "Macro",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var r = MessageBox.Show(this, $"'{name}' Macro를 삭제할까요?",
                "Macro Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (r != DialogResult.Yes)
                return;

            MacroManager.Delete(name);
            _current = null;
            _dirty = false;

            txtMacroName.Clear();
            nudRepeat.Value = 1;
            dgvSteps.Rows.Clear();

            RefreshMacroList();
            SetStatus("Status: Idle");
        }

        private void BtnMacroSave_Click(object? sender, EventArgs e)
        {
            if (_running) return;

            if (!TryBuildMacroFromUi(out var macro, out var error))
            {
                MessageBox.Show(this, error, "Macro Save",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MacroManager.AddOrUpdate(macro);
            _current = Clone(macro);
            _dirty = false;

            RefreshMacroList(macro.Name);
            UpdateTitleDirtyMark();
            SetStatus("Status: Saved");
        }

        private bool TryBuildMacroFromUi(out MacroDefinition macro, out string error)
        {
            macro = new MacroDefinition();
            error = "";

            var name = txtMacroName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                error = "Name을 입력해 주세요.";
                return false;
            }

            int repeat = (int)nudRepeat.Value;
            if (repeat < 1) repeat = 1;

            var steps = new List<MacroStep>();

            for (int i = 0; i < dgvSteps.Rows.Count; i++)
            {
                var row = dgvSteps.Rows[i];
                if (row.IsNewRow) continue;

                string preset = (row.Cells[1].Value?.ToString() ?? "").Trim();
                string delayStr = (row.Cells[2].Value?.ToString() ?? "0").Trim();

                if (string.IsNullOrWhiteSpace(preset))
                {
                    error = $"Step {i + 1}: Preset이 비어있습니다.";
                    return false;
                }

                if (!int.TryParse(delayStr, out int delayMs)) delayMs = 0;
                if (delayMs < 0) delayMs = 0;

                steps.Add(new MacroStep
                {
                    PresetName = preset,
                    DelayMs = delayMs
                });
            }

            macro.Name = name;
            macro.Repeat = repeat;
            macro.Steps = steps;

            return true;
        }

        // -------------------- Steps 편집 --------------------

        private void BtnStepAdd_Click(object? sender, EventArgs e)
        {
            if (_running) return;

            // ComboBox는 기본값 null
            dgvSteps.Rows.Add((dgvSteps.Rows.Count + 1).ToString(), null, "0");
            UpdateStepNumbers();
            MarkDirty();
        }

        private void BtnStepRemove_Click(object? sender, EventArgs e)
        {
            if (_running) return;

            if (dgvSteps.SelectedRows.Count == 0)
                return;

            dgvSteps.Rows.RemoveAt(dgvSteps.SelectedRows[0].Index);
            UpdateStepNumbers();
            MarkDirty();
        }

        private void BtnStepUp_Click(object? sender, EventArgs e)
        {
            if (_running) return;

            if (dgvSteps.SelectedRows.Count == 0) return;
            int idx = dgvSteps.SelectedRows[0].Index;
            if (idx <= 0) return;

            SwapRows(idx, idx - 1);
            dgvSteps.Rows[idx - 1].Selected = true;
            UpdateStepNumbers();
            MarkDirty();
        }

        private void BtnStepDown_Click(object? sender, EventArgs e)
        {
            if (_running) return;

            if (dgvSteps.SelectedRows.Count == 0) return;
            int idx = dgvSteps.SelectedRows[0].Index;
            if (idx >= dgvSteps.Rows.Count - 1) return;

            SwapRows(idx, idx + 1);
            dgvSteps.Rows[idx + 1].Selected = true;
            UpdateStepNumbers();
            MarkDirty();
        }

        private void SwapRows(int a, int b)
        {
            var ra = dgvSteps.Rows[a];
            var rb = dgvSteps.Rows[b];

            object? presetA = ra.Cells[1].Value;
            object? delayA = ra.Cells[2].Value;

            ra.Cells[1].Value = rb.Cells[1].Value;
            ra.Cells[2].Value = rb.Cells[2].Value;

            rb.Cells[1].Value = presetA;
            rb.Cells[2].Value = delayA;
        }

        private void UpdateStepNumbers()
        {
            for (int i = 0; i < dgvSteps.Rows.Count; i++)
                dgvSteps.Rows[i].Cells[0].Value = (i + 1).ToString();
        }

        // -------------------- Run(Start/Stop) --------------------

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            if (_running) return;

            if (!TryBuildMacroFromUi(out var macro, out var error))
            {
                MessageBox.Show(this, error, "Macro Run",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (macro.Steps.Count == 0)
            {
                MessageBox.Show(this, "Step이 없습니다. Step을 추가해 주세요.", "Macro Run",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _running = true;
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            btnMacroSave.Enabled = false;
            btnMacroNew.Enabled = false;
            btnMacroDelete.Enabled = false;

            _repeatLeft = Math.Max(1, macro.Repeat);
            _stepIndex = 0;

            progressStep.Minimum = 0;
            progressStep.Maximum = macro.Steps.Count;
            progressStep.Value = 0;

            SetStatus($"Status: Running (1/{_repeatLeft})");

            _runningMacro = macro;
            RunOneStep();
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            StopRun("Status: Stopped");
        }

        private void StopRun(string statusText)
        {
            _runTimer.Stop();
            _running = false;

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            btnMacroSave.Enabled = true;
            btnMacroNew.Enabled = true;
            btnMacroDelete.Enabled = true;

            SetStatus(statusText);
            progressStep.Value = 0;
        }

        private void RunOneStep()
        {
            if (!_running || _runningMacro == null)
                return;

            var macro = _runningMacro;

            if (_stepIndex >= macro.Steps.Count)
            {
                _repeatLeft--;
                if (_repeatLeft <= 0)
                {
                    StopRun("Status: Completed");
                    return;
                }

                _stepIndex = 0;
                progressStep.Value = 0;

                int done = (macro.Repeat - _repeatLeft) + 1;
                SetStatus($"Status: Running ({done}/{macro.Repeat})");
            }

            var step = macro.Steps[_stepIndex];

            if (!_main.TryRunPresetByName(step.PresetName, out string err))
            {
                StopRun($"Status: Error ({err})");
                MessageBox.Show(this, err, "Macro Run",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            progressStep.Value = Math.Min(progressStep.Maximum, _stepIndex + 1);

            int delay = Math.Max(0, step.DelayMs);
            _stepIndex++;

            if (delay <= 0)
            {
                RunOneStep();
                return;
            }

            SetStatus($"Status: Delay {delay} ms");
            _runTimer.Interval = delay;
            _runTimer.Start();
        }


        private void RunTimer_Tick(object? sender, EventArgs e)
        {
            _runTimer.Stop();
            if (!_running || _runningMacro == null) return;

            SetStatus("Status: Running");
            RunOneStep();
        }

        // -------------------- Utils --------------------

        private void SetStatus(string text)
        {
            lblStatus.Text = text;
        }

        private void MarkDirty()
        {
            if (_running) return;
            if (_loadingUi) return;   // 로딩 중이면 dirty 금지
            _dirty = true;
            UpdateTitleDirtyMark();
        }

        private void UpdateTitleDirtyMark()
        {
            this.Text = _dirty ? "Macro Setting *" : "Macro Setting";
        }

        private static MacroDefinition Clone(MacroDefinition src)
        {
            return new MacroDefinition
            {
                Name = src.Name,
                Repeat = src.Repeat,
                Steps = src.Steps.Select(s => new MacroStep
                {
                    PresetName = s.PresetName,
                    DelayMs = s.DelayMs
                }).ToList()
            };
        }
    }
}
