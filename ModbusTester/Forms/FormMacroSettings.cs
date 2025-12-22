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
        // 메인 폼 기능(프리셋 실행)을 재사용하기 위해 참조 보관
        private readonly FormMain _main;

        // 현재 UI에 로드된 매크로(편집 대상)
        private MacroDefinition? _current;

        // 저장되지 않은 변경사항이 있는지 표시
        private bool _dirty;

        // Step 딜레이 처리를 위한 UI 타이머
        private readonly System.Windows.Forms.Timer _runTimer = new System.Windows.Forms.Timer();

        // 실행 상태 관리
        private bool _running;
        private int _repeatLeft;
        private int _stepIndex;
        private MacroDefinition? _runningMacro; // 실행 중인 매크로 스냅샷(실행 도중 UI가 바뀌어도 실행 상태를 유지)

        // 코드로 UI를 채우는 동안 dirty 표시를 막기 위한 플래그
        private bool _loadingUi;

        public FormMacroSetting(FormMain main)
        {
            InitializeComponent();

            // 폼이 키 입력을 먼저 받도록 설정(Ctrl+S 처리용)
            this.KeyPreview = true;
            this.KeyDown += FormMacroSetting_KeyDown;

            _main = main;

            // 딜레이 타이머 Tick 연결
            _runTimer.Tick += RunTimer_Tick;

            // 초기 로드 이벤트
            this.Load += FormMacroSetting_Load;

            // 목록 선택 변경 시 매크로 로드
            lstMacros.SelectedIndexChanged += LstMacros_SelectedIndexChanged;

            // 매크로 관리 버튼
            btnMacroNew.Click += BtnMacroNew_Click;
            btnMacroDelete.Click += BtnMacroDelete_Click;
            btnMacroSave.Click += BtnMacroSave_Click;

            // Step 편집 버튼
            btnStepAdd.Click += BtnStepAdd_Click;
            btnStepRemove.Click += BtnStepRemove_Click;
            btnStepUp.Click += BtnStepUp_Click;
            btnStepDown.Click += BtnStepDown_Click;

            // 실행 버튼
            btnStart.Click += BtnStart_Click;
            btnStop.Click += BtnStop_Click;

            // 이름/반복 횟수 변경 시 dirty 표시
            txtMacroName.TextChanged += (_, __) => MarkDirty();
            nudRepeat.ValueChanged += (_, __) => MarkDirty();

            // 그리드 값 변경 시 dirty 표시
            dgvSteps.CellValueChanged += (_, __) => MarkDirty();

            // ComboBox 셀은 선택 변경 시 값 확정이 늦을 수 있어 즉시 Commit
            dgvSteps.CurrentCellDirtyStateChanged += (_, __) =>
            {
                if (dgvSteps.IsCurrentCellDirty)
                    dgvSteps.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            // ComboBox 값 불일치 등 DataError가 떠도 폼이 죽지 않게 방지
            dgvSteps.DataError += (_, __) => { };
        }

        private void FormMacroSetting_KeyDown(object? sender, KeyEventArgs e)
        {
            // Ctrl+S 저장
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;

                // 그리드 편집 중이면 값 확정 후 저장
                dgvSteps.EndEdit();
                btnMacroSave.PerformClick();
            }
        }

        private void FormMacroSetting_Load(object? sender, EventArgs e)
        {
            // 프리셋 목록을 Step 콤보박스에 채움
            InitPresetComboSource();

            // 저장된 매크로 로드 후 목록 갱신
            MacroManager.Load();
            RefreshMacroList();

            SetStatus("Status: Idle");
            btnStop.Enabled = false;

            // 매크로가 있으면 첫 항목 자동 선택
            if (lstMacros.Items.Count > 0)
                lstMacros.SelectedIndex = 0;
        }

        // 외부에서 프리셋 목록 변경 시 호출(ComboBox 갱신용)
        public void RefreshPresetCombo()
        {
            InitPresetComboSource();
        }

        private void InitPresetComboSource()
        {
            // dgvSteps의 Preset 컬럼이 ComboBox일 때만 처리
            if (colPreset is DataGridViewComboBoxColumn combo)
            {
                combo.Items.Clear();

                // 프리셋 이름만 모아서 정렬 후 주입
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

            // 저장/생성 후 특정 이름을 다시 선택하고 싶을 때 사용
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
            // 실행 중에는 선택 변경 금지
            if (_running) return;

            // 저장 안 된 변경이 있으면 경고
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
                // 원본을 직접 수정하지 않도록 복제본을 편집 대상으로 사용
                _current = Clone(macro);
                _dirty = false;

                txtMacroName.Text = _current.Name;

                // 범위를 벗어나는 값이 있어도 안전하게 표시
                nudRepeat.Value = Math.Max(nudRepeat.Minimum,
                    Math.Min(nudRepeat.Maximum, _current.Repeat));

                // Step 그리드 채우기
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
            if (_running) return;

            // 중복 없는 이름 생성 후 빈 매크로 추가
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

            // UI 초기화
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

            // UI 내용을 매크로 객체로 조립(검증 포함)
            if (!TryBuildMacroFromUi(out var macro, out var error))
            {
                MessageBox.Show(this, error, "Macro Save",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 저장 후 현재 상태 갱신
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

            // 그리드의 각 행을 Step으로 변환
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
            if (_running) return;

            // Preset은 기본값 null(사용자가 선택)
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
            // 번호 컬럼은 고정이고, Preset/Delay만 서로 교환
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
            // 첫 번째 컬럼에 보이는 Step 번호 재정렬
            for (int i = 0; i < dgvSteps.Rows.Count; i++)
                dgvSteps.Rows[i].Cells[0].Value = (i + 1).ToString();
        }

        private void BtnStart_Click(object? sender, EventArgs e)
        {
            if (_running) return;

            // 저장 여부와 상관없이 현재 UI 상태로 실행
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

            // 실행 중 UI 변경을 막기 위해 버튼 잠금
            _running = true;
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            btnMacroSave.Enabled = false;
            btnMacroNew.Enabled = false;
            btnMacroDelete.Enabled = false;

            _repeatLeft = Math.Max(1, macro.Repeat);
            _stepIndex = 0;

            // 진행 표시 초기화
            progressStep.Minimum = 0;
            progressStep.Maximum = macro.Steps.Count;
            progressStep.Value = 0;

            SetStatus($"Status: Running (1/{_repeatLeft})");

            _runningMacro = macro;
            RunOneStep();
        }

        private void BtnStop_Click(object? sender, EventArgs e)
        {
            StopRun("Status: Stopped", StopReason.Stopped);
        }

        private enum StopReason
        {
            Completed,
            Stopped,
            Error
        }

        private void StopRun(string statusText, StopReason reason)
        {
            _runTimer.Stop();
            _running = false;

            btnStart.Enabled = true;
            btnStop.Enabled = false;
            btnMacroSave.Enabled = true;
            btnMacroNew.Enabled = true;
            btnMacroDelete.Enabled = true;

            SetStatus(statusText);

            // 진행바 정책
            if (reason == StopReason.Completed)
            {
                // 완료는 끝까지 채워서 남김
                progressStep.Value = progressStep.Maximum;
            }
            else
            {
                // Stopped/Error는 현재 값 유지(리셋하지 않음)
                // 필요하면 여기서만 0으로 바꾸는 정책도 가능
            }
        }

        private void RunOneStep()
        {
            if (!_running || _runningMacro == null)
                return;

            var macro = _runningMacro;

            // 한 바퀴 끝났으면 반복 처리
            if (_stepIndex >= macro.Steps.Count)
            {
                _repeatLeft--;
                if (_repeatLeft <= 0)
                {
                    StopRun("Status: Completed", StopReason.Completed);
                    return;
                }


                _stepIndex = 0;
                progressStep.Value = 0;

                int done = (macro.Repeat - _repeatLeft) + 1;
                SetStatus($"Status: Running ({done}/{macro.Repeat})");
            }

            // 현재 Step 실행
            var step = macro.Steps[_stepIndex];

            // 실제 실행은 메인 폼의 Preset 실행 로직을 사용
            if (!_main.TryRunPresetByName(step.PresetName, out string err))
            {
                StopRun($"Status: Error ({err})", StopReason.Error);
                MessageBox.Show(this, err, "Macro Run",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            // 진행 표시 업데이트
            progressStep.Value = Math.Min(progressStep.Maximum, _stepIndex + 1);

            int delay = Math.Max(0, step.DelayMs);
            _stepIndex++;

            // 딜레이가 없으면 즉시 다음 Step 진행
            if (delay <= 0)
            {
                RunOneStep();
                return;
            }

            // 딜레이가 있으면 타이머로 대기 후 다음 Step 진행
            SetStatus($"Status: Delay {delay} ms");
            _runTimer.Interval = delay;
            _runTimer.Start();
        }

        private void RunTimer_Tick(object? sender, EventArgs e)
        {
            // 딜레이 종료 후 다음 Step 진행
            _runTimer.Stop();
            if (!_running || _runningMacro == null) return;

            SetStatus("Status: Running");
            RunOneStep();
        }

        private void SetStatus(string text)
        {
            lblStatus.Text = text;
        }

        private void MarkDirty()
        {
            // 실행 중이거나, 코드로 UI를 채우는 중이면 dirty 처리하지 않음
            if (_running) return;
            if (_loadingUi) return;

            _dirty = true;
            UpdateTitleDirtyMark();
        }

        private void UpdateTitleDirtyMark()
        {
            // 저장 안 된 변경사항이 있으면 제목에 표시
            this.Text = _dirty ? "Macro Setting *" : "Macro Setting";
        }

        private static MacroDefinition Clone(MacroDefinition src)
        {
            // UI 편집용으로 안전하게 복제(원본 보호)
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
