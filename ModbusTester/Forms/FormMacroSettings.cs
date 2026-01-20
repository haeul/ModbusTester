using ModbusTester.Macros;
using ModbusTester.Presets;
using ModbusTester.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;

namespace ModbusTester
{
    public partial class FormMacroSetting : Form
    {
        private readonly FormMain _main;
        private MacroDefinition? _current;
        private bool _dirty;
        private bool _loadingUi;

        private readonly List<MacroInstance> _instances = new List<MacroInstance>();
        private int _instanceSeq = 0;

        private readonly System.Windows.Forms.Timer _runtimeTimer = new System.Windows.Forms.Timer();
        private bool _runtimeExecuting;

        private readonly LayoutScaler _layoutScaler;

        // ===== Runtime Grid Column Index (체크박스 컬럼 추가로 인덱스 +1) =====
        private const int COL_CHK = 0;
        private const int COL_ID = 1;
        private const int COL_MACRO = 2;
        private const int COL_STATUS = 3;
        private const int COL_REPEAT = 4;
        private const int COL_STEP = 5;
        private const int COL_NEXT = 6;
        private const int COL_LAST = 7;

        public FormMacroSetting(FormMain main)
        {
            InitializeComponent();

            // exe에 박혀 있는 아이콘을 그대로 폼 아이콘으로 사용
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _layoutScaler = new LayoutScaler(this);
            _layoutScaler.ApplyInitialScale(1.058f);

            FormClosing += FormMacroSetting_FormClosing;

            KeyPreview = true;
            KeyDown += FormMacroSetting_KeyDown;

            _main = main;

            Load += FormMacroSetting_Load;
            lstMacros.SelectedIndexChanged += LstMacros_SelectedIndexChanged;

            btnMacroNew.Click += BtnMacroNew_Click;
            btnMacroDelete.Click += BtnMacroDelete_Click;
            btnMacroSave.Click += BtnMacroSave_Click;

            btnStepAdd.Click += BtnStepAdd_Click;
            btnStepRemove.Click += BtnStepRemove_Click;
            btnStepUp.Click += BtnStepUp_Click;
            btnStepDown.Click += BtnStepDown_Click;

            btnInstStart.Click += BtnInstStart_Click;
            btnInstStop.Click += BtnInstStop_Click;
            btnInstStopAll.Click += BtnInstStopAll_Click;
            btnInstPauseResume.Click += BtnInstPauseResume_Click;
            btnInstPauseAll.Click += BtnInstPauseAll_Click;

            // 기존 Clear Select / Clear Done 제거
            // btnInstClearSel.Click += BtnInstClearSel_Click;
            // btnInstClearDone.Click += BtnInstClearDone_Click;

            // Clear 버튼 하나로 통합 (체크 기준)
            btnInstClear.Click += BtnInstClear_Click;

            btnInstClearAll.Click += BtnInstClearAll_Click;

            txtMacroName.TextChanged += (_, __) => MarkDirty();
            nudRepeat.ValueChanged += (_, __) => MarkDirty();

            dgvSteps.CellValueChanged += (_, __) => MarkDirty();

            dgvSteps.CurrentCellDirtyStateChanged += (_, __) =>
            {
                if (dgvSteps.IsCurrentCellDirty)
                    dgvSteps.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            dgvSteps.DataError += (_, __) => { };

            dgvInstances.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvInstances.MultiSelect = true;

            // 체크박스만 편집 가능해야 하므로 ReadOnly 전체 true는 제거하고 컬럼별 제어
            dgvInstances.ReadOnly = false;
            dgvInstances.AllowUserToAddRows = false;
            dgvInstances.AllowUserToDeleteRows = false;
            dgvInstances.AllowUserToResizeRows = false;
            dgvInstances.RowHeadersVisible = false;

            // 체크박스 즉시 반영
            dgvInstances.CurrentCellDirtyStateChanged += (_, __) =>
            {
                if (dgvInstances.IsCurrentCellDirty)
                    dgvInstances.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            // 체크박스 헤더 클릭으로 전체 체크/해제
            dgvInstances.ColumnHeaderMouseClick += DgvInstances_ColumnHeaderMouseClick;

            // 체크박스 외 컬럼은 ReadOnly 유지
            foreach (DataGridViewColumn col in dgvInstances.Columns)
            {
                if (col.Index != COL_CHK)
                    col.ReadOnly = true;
            }

            dgvInstances.SelectionChanged += (_, __) => UpdatePauseResumeButtonText();
            dgvInstances.SelectionChanged += (_, __) => UpdatePauseAllButtonText();

            _runtimeTimer.Interval = 50;
            _runtimeTimer.Tick += RuntimeTimer_Tick;
            _runtimeTimer.Start();
        }

        private void FormMacroSetting_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // 1) 실행 중인 인스턴스는 전부 Stop
            StopAllInstancesOnClose();

            // 2) 타이머도 확실히 종료 (Dispose 되지 않고 Hide될 경우를 대비)
            try
            {
                _runtimeTimer.Stop();
                _runtimeTimer.Tick -= RuntimeTimer_Tick;
                _runtimeTimer.Dispose();
            }
            catch { }
        }

        private void StopAllInstancesOnClose()
        {
            foreach (var inst in _instances)
            {
                if (inst.State is InstanceState.Running or InstanceState.Waiting or InstanceState.Paused)
                    inst.Stop("Form Closed");
            }

            RefreshAllInstancesGridRows();
            UpdateRuntimeStatusText();
            UpdatePauseResumeButtonText();
            UpdatePauseAllButtonText();
        }

        private void FormMacroSetting_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                dgvSteps.EndEdit();
                btnMacroSave.PerformClick();
            }
        }

        private void FormMacroSetting_Load(object? sender, EventArgs e)
        {
            InitPresetComboSource();

            MacroManager.Load();
            RefreshMacroList();

            UpdateTitleDirtyMark();
            UpdateRuntimeStatusText();

            if (lstMacros.Items.Count > 0)
                lstMacros.SelectedIndex = 0;
        }

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

        private void BtnMacroNew_Click(object? sender, EventArgs e)
        {
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
            if (lstMacros.SelectedItem is not string name)
            {
                MessageBox.Show(this, "삭제할 Macro를 선택해 주세요.", "Macro",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_instances.Any(x => x.DefinitionName.Equals(name, StringComparison.OrdinalIgnoreCase)
                                 && x.State is InstanceState.Running or InstanceState.Waiting or InstanceState.Paused))
            {
                MessageBox.Show(this,
                    "해당 Macro로 실행 중인 인스턴스가 있습니다.\r\n먼저 인스턴스를 Stop 하세요.",
                    "Macro Delete",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            UpdateTitleDirtyMark();
        }

        private void BtnMacroSave_Click(object? sender, EventArgs e)
        {
            string? oldName = _current?.Name;

            if (!TryBuildMacroFromUi(out var macro, out var error))
            {
                MessageBox.Show(this, error, "Macro Save",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string newName = macro.Name;

            bool isRename = !string.IsNullOrWhiteSpace(oldName)
                            && !oldName.Equals(newName, StringComparison.OrdinalIgnoreCase);

            if (isRename)
            {
                bool hasRunning = _instances.Any(x =>
                    x.DefinitionName.Equals(oldName, StringComparison.OrdinalIgnoreCase)
                    && x.State is InstanceState.Running or InstanceState.Waiting or InstanceState.Paused);

                if (hasRunning)
                {
                    MessageBox.Show(this,
                        "해당 Macro로 실행 중인 인스턴스가 있습니다.\r\n인스턴스를 Stop 한 뒤 이름을 변경해 주세요.",
                        "Macro Save",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var exist = MacroManager.Find(newName);
                if (exist != null)
                {
                    MessageBox.Show(this,
                        "동일한 이름의 Macro가 이미 존재합니다.\r\n다른 이름으로 저장해 주세요.",
                        "Macro Save",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                MacroManager.Delete(oldName!);
            }

            MacroManager.AddOrUpdate(macro);

            _current = Clone(macro);
            _dirty = false;

            RefreshMacroList(macro.Name);
            UpdateTitleDirtyMark();
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

        private void BtnStepAdd_Click(object? sender, EventArgs e)
        {
            dgvSteps.Rows.Add((dgvSteps.Rows.Count + 1).ToString(), null, "0");
            UpdateStepNumbers();
            MarkDirty();
        }

        private void BtnStepRemove_Click(object? sender, EventArgs e)
        {
            if (dgvSteps.SelectedRows.Count == 0)
                return;

            dgvSteps.Rows.RemoveAt(dgvSteps.SelectedRows[0].Index);
            UpdateStepNumbers();
            MarkDirty();
        }

        private void BtnStepUp_Click(object? sender, EventArgs e)
        {
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

        private void BtnInstStart_Click(object? sender, EventArgs e)
        {
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

            var inst = CreateInstanceFromMacro(macro);
            _instances.Add(inst);
            AddInstanceRow(inst);

            UpdateRuntimeStatusText();
        }

        private void BtnInstStop_Click(object? sender, EventArgs e)
        {
            var list = GetTargetInstancesPreferChecked();
            if (list.Count == 0) return;

            foreach (var inst in list)
                inst.Stop("Stopped");

            RefreshInstancesGridRows(list);
            UpdateRuntimeStatusText();
            UpdatePauseResumeButtonText();
        }

        private void BtnInstStopAll_Click(object? sender, EventArgs e)
        {
            foreach (var inst in _instances)
            {
                if (inst.State is InstanceState.Running or InstanceState.Waiting or InstanceState.Paused)
                    inst.Stop("Stopped");
            }

            RefreshAllInstancesGridRows();
            UpdateRuntimeStatusText();
            UpdatePauseResumeButtonText();
        }

        private void BtnInstPauseResume_Click(object? sender, EventArgs e)
        {
            var list = GetTargetInstancesPreferChecked();
            if (list.Count == 0) return;

            foreach (var inst in list)
            {
                if (inst.State == InstanceState.Paused)
                    inst.Resume();
                else if (inst.State is InstanceState.Running or InstanceState.Waiting)
                    inst.Pause();
            }

            RefreshInstancesGridRows(list);
            UpdateRuntimeStatusText();
            UpdatePauseResumeButtonText();
        }

        private void UpdatePauseResumeButtonText()
        {
            var list = GetTargetInstancesPreferChecked();

            if (list.Count == 0)
            {
                btnInstPauseResume.Text = "Pause";
                return;
            }

            bool anyPaused = list.Any(x => x.State == InstanceState.Paused);
            btnInstPauseResume.Text = anyPaused ? "Resume" : "Pause";
        }

        private void BtnInstPauseAll_Click(object? sender, EventArgs e)
        {
            bool anyPaused = _instances.Any(x => x.State == InstanceState.Paused);
            bool anyActive = _instances.Any(x => x.State is InstanceState.Running or InstanceState.Waiting);

            if (!anyPaused && !anyActive)
                return;

            if (anyPaused)
            {
                foreach (var inst in _instances)
                {
                    if (inst.State == InstanceState.Paused)
                        inst.Resume();
                }
            }
            else
            {
                foreach (var inst in _instances)
                {
                    if (inst.State is InstanceState.Running or InstanceState.Waiting)
                        inst.Pause();
                }
            }

            RefreshAllInstancesGridRows();
            UpdateRuntimeStatusText();

            UpdatePauseResumeButtonText();
            UpdatePauseAllButtonText();
        }

        private void UpdatePauseAllButtonText()
        {
            bool anyPaused = _instances.Any(x => x.State == InstanceState.Paused);
            btnInstPauseAll.Text = anyPaused ? "Resume All" : "Pause All";
        }

        // ====== NEW: 체크박스 기반 Clear ======
        private void BtnInstClear_Click(object? sender, EventArgs e)
        {
            var checkedList = GetCheckedInstances();
            if (checkedList.Count == 0) return;

            foreach (var inst in checkedList)
            {
                if (inst.State is InstanceState.Completed or InstanceState.Stopped or InstanceState.Error)
                    _instances.Remove(inst);
            }

            RebuildInstancesGrid();
            UpdateRuntimeStatusText();
            UpdatePauseResumeButtonText();
            UpdatePauseAllButtonText();
        }

        private void BtnInstClearAll_Click(object? sender, EventArgs e)
        {
            _instances.Clear();
            RebuildInstancesGrid();
            UpdateRuntimeStatusText();
            UpdatePauseResumeButtonText();
            UpdatePauseAllButtonText();
        }

        private void RebuildInstancesGrid()
        {
            dgvInstances.Rows.Clear();
            foreach (var inst in _instances)
                AddInstanceRow(inst);
        }

        private void RuntimeTimer_Tick(object? sender, EventArgs e)
        {
            // =======================
            // [MIN FIX #1] Next 실시간 감소 표시 갱신
            // - 기존에는 due 인스턴스만 Refresh 해서 Next 값이 고정되어 보였음
            // - 매 Tick마다 화면에 찍힌 모든 인스턴스의 Next 셀만 갱신
            // =======================
            foreach (DataGridViewRow row in dgvInstances.Rows)
            {
                if (row.Tag is MacroInstance inst)
                    row.Cells[COL_NEXT].Value = inst.NextText;
            }

            if (_runtimeExecuting) return;

            var now = DateTime.Now;

            var due = _instances
                .Where(x => x.IsDue(now))
                .OrderBy(x => x.NextDue)
                .FirstOrDefault();

            if (due == null)
                return;

            try
            {
                _runtimeExecuting = true;

                due.MarkExecuting();
                RefreshInstanceGridRow(due);

                // =======================
                // [MIN FIX #2] Running 상태가 화면에 실제로 보이도록 강제 페인트
                // - Running -> Waiting 전환이 너무 빠르면 UI가 Running을 그리기 전에 Waiting만 보임
                // =======================
                dgvInstances.Invalidate();
                dgvInstances.Update();

                var step = due.GetCurrentStep();
                if (step == null)
                {
                    due.CompleteIfDone();
                    RefreshInstanceGridRow(due);
                    UpdateRuntimeStatusText();
                    UpdatePauseResumeButtonText();
                    return;
                }

                bool ok = _main.TryRunPresetByName(step.PresetName, out string err);

                if (!ok)
                {
                    due.Fail(err);
                    RefreshInstanceGridRow(due);
                    UpdateRuntimeStatusText();
                    UpdatePauseResumeButtonText();

                    MessageBox.Show(this, err, "Macro Run",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                due.OnStepSuccess(step.DelayMs);
                RefreshInstanceGridRow(due);
                UpdateRuntimeStatusText();
                UpdatePauseResumeButtonText();
            }
            finally
            {
                _runtimeExecuting = false;
            }
        }

        private MacroInstance CreateInstanceFromMacro(MacroDefinition macroSnapshot)
        {
            _instanceSeq++;

            int sameCount = _instances.Count(x =>
                x.DefinitionName.Equals(macroSnapshot.Name, StringComparison.OrdinalIgnoreCase)) + 1;

            return new MacroInstance(
                id: _instanceSeq,
                displayName: $"{macroSnapshot.Name}#{sameCount}",
                definitionName: macroSnapshot.Name,
                macro: Clone(macroSnapshot));
        }

        private void AddInstanceRow(MacroInstance inst)
        {
            int r = dgvInstances.Rows.Add(
                false,                    // 체크박스
                inst.Id.ToString(),
                inst.DisplayName,
                inst.State.ToString(),
                inst.RepeatText,
                inst.StepText,
                inst.NextText,
                inst.LastResult);

            dgvInstances.Rows[r].Tag = inst;
        }

        private List<MacroInstance> GetSelectedInstances()
        {
            var list = new List<MacroInstance>();
            foreach (DataGridViewRow row in dgvInstances.SelectedRows)
            {
                if (row.Tag is MacroInstance inst)
                    list.Add(inst);
            }
            return list;
        }
        private List<MacroInstance> GetTargetInstancesPreferChecked()
        {
            var checkedList = GetCheckedInstances();
            if (checkedList.Count > 0)
                return checkedList;

            return GetSelectedInstances();
        }


        private List<MacroInstance> GetCheckedInstances()
        {
            var list = new List<MacroInstance>();
            foreach (DataGridViewRow row in dgvInstances.Rows)
            {
                if (row.Tag is not MacroInstance inst)
                    continue;

                bool isChecked = false;
                try
                {
                    isChecked = Convert.ToBoolean(row.Cells[COL_CHK].Value);
                }
                catch { isChecked = false; }

                if (isChecked)
                    list.Add(inst);
            }
            return list;
        }

        private void RefreshAllInstancesGridRows()
        {
            foreach (DataGridViewRow row in dgvInstances.Rows)
            {
                if (row.Tag is MacroInstance inst)
                    ApplyInstanceToRow(row, inst);
            }
        }

        private void RefreshInstancesGridRows(IEnumerable<MacroInstance> list)
        {
            var set = new HashSet<MacroInstance>(list);
            foreach (DataGridViewRow row in dgvInstances.Rows)
            {
                if (row.Tag is MacroInstance inst && set.Contains(inst))
                    ApplyInstanceToRow(row, inst);
            }
        }

        private void RefreshInstanceGridRow(MacroInstance inst)
        {
            foreach (DataGridViewRow row in dgvInstances.Rows)
            {
                if (ReferenceEquals(row.Tag, inst))
                {
                    ApplyInstanceToRow(row, inst);
                    return;
                }
            }
        }

        private void ApplyInstanceToRow(DataGridViewRow row, MacroInstance inst)
        {
            // 체크박스는 사용자가 컨트롤하므로 건드리지 않음
            row.Cells[COL_ID].Value = inst.Id.ToString();
            row.Cells[COL_MACRO].Value = inst.DisplayName;
            row.Cells[COL_STATUS].Value = inst.State.ToString();
            row.Cells[COL_REPEAT].Value = inst.RepeatText;
            row.Cells[COL_STEP].Value = inst.StepText;
            row.Cells[COL_NEXT].Value = inst.NextText;
            row.Cells[COL_LAST].Value = inst.LastResult;
        }

        private void UpdateRuntimeStatusText()
        {
            int running = _instances.Count(x => x.State == InstanceState.Running || x.State == InstanceState.Waiting);
            int paused = _instances.Count(x => x.State == InstanceState.Paused);
            int error = _instances.Count(x => x.State == InstanceState.Error);

            grpRuntime.Text = $"Runtime / Execution   Running({running})  Paused({paused})  Error({error})  Total({_instances.Count})";
        }

        private void MarkDirty()
        {
            if (_loadingUi) return;

            _dirty = true;
            UpdateTitleDirtyMark();
        }

        private void UpdateTitleDirtyMark()
        {
            Text = _dirty ? "Macro Setting *" : "Macro Setting";
        }

        private void DgvInstances_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex != COL_CHK)
                return;

            bool anyRow = dgvInstances.Rows.Count > 0;
            if (!anyRow) return;

            bool allChecked = true;
            foreach (DataGridViewRow row in dgvInstances.Rows)
            {
                bool v = false;
                try { v = Convert.ToBoolean(row.Cells[COL_CHK].Value); } catch { v = false; }
                if (!v)
                {
                    allChecked = false;
                    break;
                }
            }

            bool target = !allChecked;

            foreach (DataGridViewRow row in dgvInstances.Rows)
                row.Cells[COL_CHK].Value = target;
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

        private enum InstanceState
        {
            Waiting,
            Running,
            Paused,
            Completed,
            Stopped,
            Error
        }

        private sealed class MacroInstance
        {
            public MacroInstance(int id, string displayName, string definitionName, MacroDefinition macro)
            {
                Id = id;
                DisplayName = displayName;
                DefinitionName = definitionName;
                Macro = macro;

                RepeatTotal = Math.Max(1, macro.Repeat);
                RepeatDone = 0;
                StepIndex = 0;

                State = InstanceState.Waiting;
                NextDue = DateTime.Now;
                LastResult = "-";
            }

            public int Id { get; }
            public string DisplayName { get; }
            public string DefinitionName { get; }
            public MacroDefinition Macro { get; }

            public InstanceState State { get; private set; }

            public int RepeatTotal { get; }
            public int RepeatDone { get; private set; }
            public int StepIndex { get; private set; }

            public DateTime NextDue { get; private set; }
            public string LastResult { get; private set; }

            public string RepeatText => $"{Math.Min(RepeatTotal, RepeatDone + 1)}/{RepeatTotal}";
            public string StepText => $"{Math.Min(Macro.Steps.Count, StepIndex + 1)}/{Macro.Steps.Count}";

            public string NextText
            {
                get
                {
                    if (State == InstanceState.Paused) return "-";
                    if (State == InstanceState.Completed) return "-";
                    if (State == InstanceState.Stopped) return "-";
                    if (State == InstanceState.Error) return "-";

                    var ms = (int)Math.Max(0, (NextDue - DateTime.Now).TotalMilliseconds);
                    return ms.ToString();
                }
            }

            public bool IsDue(DateTime now)
            {
                if (State == InstanceState.Paused) return false;
                if (State == InstanceState.Completed) return false;
                if (State == InstanceState.Stopped) return false;
                if (State == InstanceState.Error) return false;

                return now >= NextDue;
            }

            public void MarkExecuting()
            {
                if (State == InstanceState.Waiting)
                    State = InstanceState.Running;
            }

            public MacroStep? GetCurrentStep()
            {
                if (State == InstanceState.Stopped || State == InstanceState.Error || State == InstanceState.Completed)
                    return null;

                if (Macro.Steps == null || Macro.Steps.Count == 0)
                    return null;

                if (StepIndex >= Macro.Steps.Count)
                    return null;

                return Macro.Steps[StepIndex];
            }

            public void OnStepSuccess(int delayMs)
            {
                LastResult = "OK";
                StepIndex++;

                if (StepIndex >= Macro.Steps.Count)
                {
                    RepeatDone++;
                    if (RepeatDone >= RepeatTotal)
                    {
                        State = InstanceState.Completed;
                        NextDue = DateTime.MaxValue;
                        return;
                    }

                    StepIndex = 0;
                }

                int d = Math.Max(0, delayMs);
                NextDue = DateTime.Now.AddMilliseconds(d);

                if (State == InstanceState.Running)
                    State = InstanceState.Waiting;
            }

            public void CompleteIfDone()
            {
                if (Macro.Steps == null || Macro.Steps.Count == 0)
                {
                    State = InstanceState.Completed;
                    LastResult = "No Steps";
                    NextDue = DateTime.MaxValue;
                    return;
                }

                if (RepeatDone >= RepeatTotal)
                {
                    State = InstanceState.Completed;
                    NextDue = DateTime.MaxValue;
                }
            }

            public void Stop(string reason)
            {
                State = InstanceState.Stopped;
                LastResult = reason;
                NextDue = DateTime.MaxValue;
            }

            public void Pause()
            {
                if (State is InstanceState.Completed or InstanceState.Error or InstanceState.Stopped)
                    return;

                State = InstanceState.Paused;
                LastResult = "Paused";
            }

            public void Resume()
            {
                if (State != InstanceState.Paused) return;

                State = InstanceState.Waiting;
                LastResult = "Resumed";
                NextDue = DateTime.Now;
            }

            public void Fail(string err)
            {
                State = InstanceState.Error;
                LastResult = string.IsNullOrWhiteSpace(err) ? "Error" : err;
                NextDue = DateTime.MaxValue;
            }
        }
    }
}
