using ModbusTester.Controls;
using ModbusTester.Core;
using ModbusTester.Modbus;
using ModbusTester.Presets;
using ModbusTester.Services;
using ModbusTester.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ModbusTester.Utils.HexExtensions;

namespace ModbusTester
{
    public partial class FormMain : Form
    {
        // 컬럼 인덱스
        private const int COL_REG = 0;
        private const int COL_NAME = 1;
        private const int COL_HEX = 2;
        private const int COL_DEC = 3;
        private const int COL_BIT = 4;

        private readonly LayoutScaler _layoutScaler;

        private readonly SerialPort? _sp;
        private readonly ModbusSlave? _slave;
        private readonly bool _slaveMode;

        private readonly SerialModbusClient? _client;
        private readonly ModbusMasterService? _master;
        private readonly ModbusPoller? _poller;

        private FormQuickView? _quick;

        private readonly RegisterGridController _gridController;

        private readonly RecordingService _recService;

        private FormMacroSetting? _macroForm;

        private FormGridZoom? _rxZoom;

        private PollingConfig? _pollingConfig;

        private bool _isOpen => !_slaveMode && _sp != null && _sp.IsOpen;

        public FormMain(SerialPort? sp, ModbusSlave? slave, bool slaveMode, byte slaveId)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;

            // exe에 박혀 있는 아이콘을 그대로 폼 아이콘으로 사용
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _layoutScaler = new LayoutScaler(this);
            _layoutScaler.ApplyInitialScale(1.058f);

            _sp = sp;
            _slave = slave;
            _slaveMode = slaveMode;

            // Recording 디렉토리: 실행 폴더 하위의 Data
            string recDir = Path.Combine(Application.StartupPath, "Data");
            _recService = new RecordingService(recDir);

            // Master 모드에서만 통신 객체 생성 (오프라인 UI 용도로 _sp == null 허용)
            if (!_slaveMode && _sp != null)
            {
                _client = new SerialModbusClient(_sp);
                _master = new ModbusMasterService(_client);
                _poller = new ModbusPoller(_client);
            }

            // 그리드 컨트롤러
            _gridController = new RegisterGridController(
                gridTx,
                gridRx,
                numStartRegister,
                COL_REG,
                COL_NAME,
                COL_HEX,
                COL_DEC,
                COL_BIT
            );

            Shown += FormMain_Shown;
            numSlave.Value = slaveId;
        }

        private async void FormMain_Shown(object? sender, EventArgs e)
        {
            await _gridController.InitializeGridsAsync();

            // 그리드 초기화 후 TX Name을 RX로 1회 동기화(초기 상태 안정화)
            _gridController.SyncAllTxNamesToRx();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Function Code 기본값
            if (cmbFunctionCode.Items.Count == 0)
            {
                cmbFunctionCode.Items.AddRange(new object[]
                {
                    "03h Read HR",
                    "04h Read IR",
                    "06h Write SR",
                    "10h Write MR"
                });
            }

            if (cmbFunctionCode.SelectedIndex < 0)
                cmbFunctionCode.SelectedIndex = 0;

            // TX 기본 설정
            numStartRegister.Hexadecimal = true;
            numStartRegister.Minimum = 0x0000;
            numStartRegister.Maximum = 0xFFFF;
            numStartRegister.Value = 0x0000;

            numCount.Value = 1;
            RefreshDataCount();

            // 그리드 공통 설정
            _gridController.SetupGrids();

            // (중복 방지) 이벤트 재결합
            gridTx.KeyDown -= GridTx_KeyDown;
            gridTx.KeyDown += GridTx_KeyDown;

            gridTx.KeyDown += Grid_DeleteKeyDown;
            gridRx.KeyDown += Grid_DeleteKeyDown;

            gridTx.EditingControlShowing -= GridTx_EditingControlShowing;
            gridTx.EditingControlShowing += GridTx_EditingControlShowing;

            gridTx.CellValueChanged -= GridTx_CellValueChanged;
            gridTx.CellValueChanged += GridTx_CellValueChanged;

            gridTx.CurrentCellDirtyStateChanged -= GridTx_CurrentCellDirtyStateChanged;
            gridTx.CurrentCellDirtyStateChanged += GridTx_CurrentCellDirtyStateChanged;

            // 아래 두 이벤트는 디자이너에서 이미 연결되어 있을 수 있음
            // (중복 호출 방지 위해 -= 후 += 형태로 안정 결합)
            gridTx.CellBeginEdit -= Grid_CellBeginEdit;
            gridTx.CellBeginEdit += Grid_CellBeginEdit;

            gridRx.CellBeginEdit -= Grid_CellBeginEdit;
            gridRx.CellBeginEdit += Grid_CellBeginEdit;

            gridTx.CellEndEdit -= HexAutoFormat_OnEndEdit;
            gridTx.CellEndEdit += HexAutoFormat_OnEndEdit;

            gridRx.CellEndEdit -= HexAutoFormat_OnEndEdit;
            gridRx.CellEndEdit += HexAutoFormat_OnEndEdit;

            gridRx.CellDoubleClick += Grid_CellDoubleClick_OpenZoom;

            // RX 그리드에서 QV 컬럼을 맨 앞으로
            var qvCol = gridRx.Columns["colRxQuickView"];
            if (qvCol != null)
                qvCol.DisplayIndex = 0;

            // 모드에 따른 UI 반영
            UpdateUiByMode();

            // 프리셋 로드
            FunctionPresetManager.Load();
            RefreshPresetCombo();
        }

        private void Grid_CellDoubleClick_OpenZoom(object? sender, DataGridViewCellEventArgs e)
        {
            if (sender is not DataGridView g) return;

            // RX 확대창이 이미 있으면 재사용
            if (_rxZoom != null && !_rxZoom.IsDisposed)
            {
                _rxZoom.BringToFront();
                _rxZoom.Activate();
                return;
            }

            // 새로 생성
            _rxZoom = new FormGridZoom(gridRx, this, placeOnRight: true, hideQvColumn: false);
            _rxZoom.FormClosed += (_, __) => _rxZoom = null;
            _rxZoom.Show(this);
        }

        private void UpdateUiByMode()
        {
            bool slave = _slaveMode;

            grpTx.Enabled = !slave;   // TX 영역
            grpRx.Enabled = !slave;   // RX 영역
            grpOpt.Enabled = !slave;  // Polling / Recording 옵션

            btnSend.Enabled = !slave;
            btnCalcCrc.Enabled = !slave;
            btnPollStart.Enabled = !slave;
            btnPollStop.Enabled = !slave;

            gridTx.Enabled = !slave;
            gridRx.Enabled = !slave;

            Log(slave
                ? "[MODE] Slave 모드 (해당 포트로 들어오는 요청에 응답)"
                : "[MODE] Master 모드 (요청을 전송)");
        }

        private void chkSlaveMode_CheckedChanged(object sender, EventArgs e)
        {
            pollTimer.Stop();
            _pollingConfig = null;
            UpdateRecordingState();
            UpdateUiByMode();
        }

        private void Grid_DeleteKeyDown(object sender, KeyEventArgs e)
        {
            _gridController.HandleGridDeleteKey(sender, e);
        }

        // Register 첫 줄만 수정 가능하게
        private void Grid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            _gridController.HandleCellBeginEdit(sender, e);
        }

        // HEX/DEC/BIT 포맷 처리
        private void HexAutoFormat_OnEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            _gridController.HandleCellEndEdit(sender, e);

            // Name 편집은 EndEdit에서 처리하지 않음.
            // Name 동기화는 CellValueChanged( + CommitEdit )로 100% 처리.
        }

        // Dirty 상태(특히 편집 중 Delete/Backspace, 체크박스 등)를 “즉시 커밋”해서 CellValueChanged가 항상 뜨게 함
        private void GridTx_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (gridTx.IsCurrentCellDirty)
                gridTx.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        // Name 값이 어떤 방식으로든 바뀌면 TX → RX 즉시 동기화
        private void GridTx_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (e == null) return;
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex != COL_NAME) return;

            _gridController.SyncTxNameToRxByRowIndex(e.RowIndex);
        }

        // ───────────────────── 드래그/삭제 처리 (TX Name/Value) ─────────────────────

        private void GridTx_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete && e.KeyCode != Keys.Back)
                return;

            ClearSelectedTxCells();

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        private void GridTx_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is TextBox tb)
            {
                tb.KeyDown -= GridTx_EditingTextBox_KeyDown;
                tb.KeyDown += GridTx_EditingTextBox_KeyDown;
            }
        }

        private void GridTx_EditingTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete && e.KeyCode != Keys.Back)
                return;

            ClearSelectedTxCells();

            e.Handled = true;
            e.SuppressKeyPress = true;
        }

        /// <summary>
        /// Delete/Backspace 입력 시 선택된 셀에 따라 Name 또는 HEX/DEC/BIT 묶음 정리
        /// - Name 셀만 선택하면 Name만 지우고 TX→RX 동기화 유지
        /// - HEX/DEC/BIT 중 하나라도 선택하면 같은 행의 세 값 모두 지움(Name은 유지)
        /// </summary>
        private void ClearSelectedTxCells()
        {
            if (gridTx.SelectedCells == null || gridTx.SelectedCells.Count == 0)
                return;

            var nameRows = new HashSet<int>();
            var valueRows = new HashSet<int>();

            foreach (DataGridViewCell cell in gridTx.SelectedCells)
            {
                if (cell.OwningRow == null || cell.OwningRow.IsNewRow)
                    continue;

                if (cell.ColumnIndex == COL_NAME)
                {
                    nameRows.Add(cell.RowIndex);
                }
                else if (cell.ColumnIndex == COL_HEX || cell.ColumnIndex == COL_DEC || cell.ColumnIndex == COL_BIT)
                {
                    valueRows.Add(cell.RowIndex);
                }
            }

            foreach (int r in valueRows)
            {
                if (r < 0 || r >= gridTx.Rows.Count) continue;
                var row = gridTx.Rows[r];
                if (row.IsNewRow) continue;

                row.Cells[COL_HEX].Value = "";
                row.Cells[COL_DEC].Value = "";
                row.Cells[COL_BIT].Value = "";
            }

            foreach (int r in nameRows)
            {
                if (r < 0 || r >= gridTx.Rows.Count) continue;
                var row = gridTx.Rows[r];
                if (row.IsNewRow) continue;

                // 편집 중일 수 있으니 값 변경 → 커밋 → 즉시 동기화까지 보장
                row.Cells[COL_NAME].Value = "";

                // 프로그램적으로 바꾼 경우/편집 상태에 따라 CellValueChanged가 애매하게 안 뜨는 케이스 방지
                _gridController.SyncTxNameToRxByRowIndex(r);
            }

            if (gridTx.IsCurrentCellInEditMode)
                gridTx.EndEdit();
        }
        // ───────────────────── CRC 계산 버튼 ─────────────────────

        private void btnCalcCrc_Click(object sender, EventArgs e)
        {
            try
            {
                byte slave = (byte)numSlave.Value;
                ushort start = (ushort)numStartRegister.Value;
                ushort count = (ushort)numCount.Value;
                byte fc = GetFunctionCode();

                byte[] frame;

                if (fc == 0x03 || fc == 0x04)
                {
                    frame = ModbusRtu.BuildReadFrame(slave, fc, start, count);
                    txtDataCount.Text = "0";
                }
                else if (fc == 0x06)
                {
                    ushort val = _gridController.ReadTxValueOrZero(0);
                    frame = ModbusRtu.BuildWriteSingleFrame(slave, start, val);
                    txtDataCount.Text = "2";
                }
                else if (fc == 0x10)
                {
                    ushort[] vals = _gridController.ReadTxValues(count);
                    frame = ModbusRtu.BuildWriteMultipleFrame(slave, start, vals);
                    byte byteCount = (byte)(vals.Length * 2);
                    txtDataCount.Text = byteCount.ToString();
                }
                else
                {
                    throw new NotSupportedException("지원하지 않는 Function Code");
                }

                if (frame.Length < 4)
                    throw new InvalidOperationException("프레임 길이가 너무 짧습니다.");

                // RTU 프레임 끝 2바이트가 CRC (Low, High)
                byte crcLo = frame[^2];
                byte crcHi = frame[^1];

                txtCrc.Text = $"{crcHi:X2}{crcLo:X2}h";
                Log($"[CRC CALC] {frame.ToHex()}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("CRC 계산 실패: " + ex.Message);
            }
        }

        // ───────────────────── 수동 전송 버튼 ─────────────────────

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (_slaveMode)
            {
                MessageBox.Show("지금은 Slave 모드입니다. Master 모드에서 전송하세요.");
                return;
            }
            if (!_isOpen)
            {
                MessageBox.Show("먼저 포트를 OPEN 하세요.");
                return;
            }
            if (_master == null)
            {
                MessageBox.Show("통신 클라이언트가 초기화되지 않았습니다.");
                return;
            }

            try
            {
                byte slave = (byte)numSlave.Value;
                ushort start = (ushort)numStartRegister.Value;
                ushort count = (ushort)numCount.Value;
                byte fc = GetFunctionCode();

                if (fc == 0x03 || fc == 0x04)
                {
                    var result = _master.ReadRegisters(slave, (FunctionCode)fc, start, count);

                    Log("TX: " + result.Request.ToHex());
                    Log("RX: " + result.Response.ToHex());

                    UpdateReceiveHeader(result.Response, slave, fc, start, count);
                    _gridController.FillRxGrid(start, result.Values);

                    RegisterCache.UpdateRange(start, result.Values);
                }
                else if (fc == 0x06)
                {
                    ushort val = _gridController.ReadTxValueOrZero(0);
                    var result = _master.WriteSingleRegister(slave, start, val);

                    Log("TX: " + result.Request.ToHex());
                    Log("RX: " + result.Response.ToHex());

                    UpdateReceiveHeader(result.Response, slave, fc, start, 1);
                    RegisterCache.UpdateRange(start, new ushort[] { val });
                }
                else if (fc == 0x10)
                {
                    ushort[] vals = _gridController.ReadTxValues(count);
                    var result = _master.WriteMultipleRegisters(slave, start, vals);

                    Log("TX: " + result.Request.ToHex());
                    Log("RX: " + result.Response.ToHex());

                    UpdateReceiveHeader(result.Response, slave, fc, start, (ushort)vals.Length);
                    RegisterCache.UpdateRange(start, vals);
                }
                else
                {
                    throw new NotSupportedException("지원하지 않는 Function Code");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("전송 실패: " + ex.Message);
            }
        }

        // ───────────────────── TX/RX/Log 버튼들 ─────────────────────

        private void btnRevert_Click(object sender, EventArgs e)
        {
            _gridController.RevertTxSnapshot();

            // Revert는 TX Name도 바뀔 수 있으니 1회 전체 동기화
            _gridController.SyncAllTxNamesToRx();
        }

        private void btnTxClear_Click(object sender, EventArgs e)
        {
            // 닉네임 제외 값 영역이 다 비어있으면 Clear 막기 (스냅샷도 갱신 안 함)
            if (_gridController.IsTxValueAreaEmpty())
                return;

            _gridController.SaveTxSnapshot();
            _gridController.ClearTxValues();
        }

        private void btnRxClear_Click(object sender, EventArgs e)
        {
            _gridController.ClearRxValues();

            txtRxSlave.Clear();
            txtRxFc.Clear();
            txtRxStart.Clear();
            txtRxCount.Clear();
            txtRxDataCount.Clear();
            txtRxCrc.Clear();
        }

        private void btnCopyToTx_Click(object sender, EventArgs e)
        {
            _gridController.CopyRxToTx();
        }

        private void btnLogClear_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }

        private void btnSaveLog_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "Text|*.txt",
                FileName = $"modbus_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };
            if (sfd.ShowDialog(this) == DialogResult.OK)
                File.WriteAllText(sfd.FileName, txtLog.Text);
        }

        private void numCount_ValueChanged(object sender, EventArgs e) => RefreshDataCount();
        private void cmbFunctionCode_SelectedIndexChanged(object sender, EventArgs e) => RefreshDataCount();
        private void cmbFunctionCode_TextChanged(object sender, EventArgs e) => RefreshDataCount();

        // ───────────────────── 폴링/Recording ─────────────────────

        private void btnPollStart_Click(object sender, EventArgs e)
        {
            if (_slaveMode)
            {
                MessageBox.Show("Slave 모드에서는 폴링을 사용할 수 없습니다.");
                return;
            }

            byte fc = GetFunctionCode();
            if (!(fc == 0x03 || fc == 0x04))
            {
                MessageBox.Show("폴링은 Function Code 03/04(Read)만 지원합니다.");
                return;
            }

            _pollingConfig = new PollingConfig
            {
                Slave = (byte)numSlave.Value,
                Start = (ushort)numStartRegister.Value,
                Count = (ushort)numCount.Value,
                FunctionCode = fc
            };

            pollTimer.Interval = (int)numInterval.Value;
            pollTimer.Start();

            UpdateRecordingState();
        }

        private void btnPollStop_Click(object sender, EventArgs e)
        {
            pollTimer.Stop();
            UpdateRecordingState();
        }

        private void pollTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!_isOpen) return;
                if (_poller == null) return;

                if (_pollingConfig == null) return;

                var cfg = _pollingConfig.Value;
                byte slave = cfg.Slave;
                ushort start = cfg.Start;
                ushort count = cfg.Count;
                byte fc = cfg.FunctionCode;

                var result = _poller.Poll(slave, fc, start, count);

                Log("TX: " + result.Request.ToHex());
                Log("RX: " + result.Response.ToHex());

                if (result.IsException)
                    return;

                UpdateReceiveHeader(result.Response, slave, fc, start, count);
                _gridController.FillRxGrid(start, result.Values);

                RegisterCache.UpdateRange(start, result.Values);
                _recService.AppendSnapshotIfDue(DateTime.Now, result.Values);
            }
            catch
            {
                // 폴링 중 예외는 조용히 무시
            }
        }

        private void Log(string line)
        {
            LogDirection dir = LogDirection.None;

            if (line.StartsWith("TX:", StringComparison.OrdinalIgnoreCase))
                dir = LogDirection.Tx;
            else if (line.StartsWith("RX:", StringComparison.OrdinalIgnoreCase))
                dir = LogDirection.Rx;

            Color color = dir switch
            {
                LogDirection.Tx => Color.DarkBlue,
                LogDirection.Rx => Color.DarkGreen,
                _ => txtLog.ForeColor
            };

            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionLength = 0;
            txtLog.SelectionColor = color;

            txtLog.AppendText(line + Environment.NewLine);

            txtLog.SelectionColor = txtLog.ForeColor;
            txtLog.ScrollToCaret();
        }

        private enum LogDirection { None, Tx, Rx }

        private byte GetFunctionCode()
        {
            string raw = (cmbFunctionCode?.Text ?? "03").Trim();
            int space = raw.IndexOf(' ');
            if (space > 0) raw = raw.Substring(0, space).Trim();
            if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) raw = raw.Substring(2);
            if (raw.EndsWith("h", StringComparison.OrdinalIgnoreCase)) raw = raw.Substring(0, raw.Length - 1);
            if (raw.Length == 1) raw = "0" + raw;

            if (byte.TryParse(raw, NumberStyles.HexNumber, null, out var fcHex))
                return fcHex;
            if (byte.TryParse(raw, out var fcDec))
                return fcDec;

            return 0x03;
        }

        private void RefreshDataCount()
        {
            byte fc = GetFunctionCode();
            ushort count = (ushort)numCount.Value;

            if (fc == 0x10)
                txtDataCount.Text = (count * 2).ToString();
            else if (fc == 0x06)
                txtDataCount.Text = "2";
            else
                txtDataCount.Text = "0";
        }

        private void chkRecording_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateRecordingState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Recording 설정 실패: " + ex.Message);
                chkRecording.Checked = false;
                UpdateRecordingState();
            }
        }

        private int ParseRecordSeconds()
        {
            string raw = (cmbRecordEvery?.Text ?? "").Trim();
            if (string.IsNullOrEmpty(raw)) return 60;

            string digits = new string(raw.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out int s)) return s;

            if (int.TryParse(raw, out s)) return s;

            return 60;
        }

        private void StartRecordingInternal()
        {
            int seconds = ParseRecordSeconds();
            if (seconds <= 0) seconds = 60;

            byte slave = (byte)numSlave.Value;
            byte fc = GetFunctionCode();
            ushort start = (ushort)numStartRegister.Value;
            ushort count = (ushort)numCount.Value;

            if (_pollingConfig != null && pollTimer.Enabled)
            {
                var cfg = _pollingConfig.Value;
                slave = cfg.Slave;
                fc = cfg.FunctionCode;
                start = cfg.Start;
                count = cfg.Count;
            }

            _recService.Start(slave, fc, start, count, seconds);
            Log($"[REC] start every {seconds}s → {_recService.CurrentFilePath}");
        }

        private void UpdateRecordingState()
        {
            if (chkRecording.Checked && pollTimer.Enabled)
            {
                if (!_recService.IsRecording)
                    StartRecordingInternal();
            }
            else
            {
                if (_recService.IsRecording)
                {
                    _recService.Stop();
                    Log("[REC] stop");
                }
            }
        }

        // ───────────────────── QuickView ─────────────────────

        private void btnQuickView_Click(object sender, EventArgs e)
        {
            var targets = GetRxQuickViewTargets();
            if (targets.Count == 0)
            {
                MessageBox.Show("QuickView에 표시할 레지스터를 QV 컬럼에서 선택하세요.");
                return;
            }

            if (_quick == null || _quick.IsDisposed)
            {
                _quick = new FormQuickView(targets);
                _quick.UpdateTargets(targets);
                _quick.Show(this);

                var wa = Screen.FromControl(this).WorkingArea;
                int x = Math.Min(Right, wa.Right - _quick.Width);
                int y = Math.Max(wa.Top, Math.Min(Top, wa.Bottom - _quick.Height));
                _quick.Location = new Point(x, y);
            }
            else
            {
                _quick.UpdateTargets(targets);
                _quick.Focus();
            }
        }

        private List<(ushort addr, string label)> GetRxQuickViewTargets()
        {
            var list = new List<(ushort addr, string label)>();

            int selColIndex = gridRx.Columns["colRxQuickView"]?.Index ?? -1;
            if (selColIndex < 0) return list;

            foreach (DataGridViewRow r in gridRx.Rows)
            {
                if (r.IsNewRow) continue;

                bool isChecked = r.Cells[selColIndex].Value is bool b && b;
                if (!isChecked) continue;

                string regText = Convert.ToString(r.Cells[COL_REG].Value) ?? "";
                if (!TryParseUShortFromRegText(regText, out ushort addr))
                    continue;

                string name = Convert.ToString(r.Cells[COL_NAME].Value) ?? "";
                string label = !string.IsNullOrWhiteSpace(name)
                               ? name.Trim()
                               : $"{addr:X4}h";

                list.Add((addr, label));
            }

            return list;
        }

        private static bool TryParseUShortFromRegText(string s, out ushort value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;

            s = s.Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                s = s[2..];
            if (s.EndsWith("h", StringComparison.OrdinalIgnoreCase))
                s = s[..^1];

            return ushort.TryParse(s, NumberStyles.HexNumber, null, out value);
        }

        // ───────────────────── Preset ─────────────────────

        private void ApplyPresetTxRows(FunctionPreset preset)
        {
            if (preset.TxRows == null || preset.TxRows.Count == 0)
                return;

            var map = preset.TxRows.ToDictionary(x => x.Address);

            ushort start = preset.StartAddress;
            int count = preset.RegisterCount;

            for (int i = 0; i < count && i < gridTx.Rows.Count; i++)
            {
                ushort addr = (ushort)(start + i);
                if (!map.TryGetValue(addr, out var saved))
                    continue;

                var row = gridTx.Rows[i];
                if (row.IsNewRow) continue;

                // Name
                row.Cells[COL_NAME].Value = saved.Name ?? "";

                // Value가 있으면 DEC/HEX/BIT를 재계산해서 채움
                if (saved.Value.HasValue)
                {
                    ushort v = saved.Value.Value;
                    row.Cells[COL_DEC].Value = v.ToString();
                    row.Cells[COL_HEX].Value = v.ToString("X4") + "h";
                    row.Cells[COL_BIT].Value = Convert.ToString(v, 2).PadLeft(16, '0');
                }
            }

            // 프리셋 적용은 대량 변경이므로 마지막에 1회 전체 동기화가 가장 깔끔/확실
            _gridController.SyncAllTxNamesToRx();
        }

        private ushort? TryReadUShortFromCells(DataGridViewRow row)
        {
            string decText = Convert.ToString(row.Cells[COL_DEC].Value) ?? "";
            if (ushort.TryParse(decText.Trim(), out ushort dec))
                return dec;

            string hexText = Convert.ToString(row.Cells[COL_HEX].Value) ?? "";
            hexText = hexText.Trim()
                             .Replace("0x", "", StringComparison.OrdinalIgnoreCase)
                             .Replace("h", "", StringComparison.OrdinalIgnoreCase);

            if (ushort.TryParse(hexText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort hex))
                return hex;

            return null;
        }

        private List<TxRowPreset> CaptureTxRows(ushort start, int count)
        {
            var list = new List<TxRowPreset>();

            for (int i = 0; i < count && i < gridTx.Rows.Count; i++)
            {
                var row = gridTx.Rows[i];
                if (row.IsNewRow) continue;

                string name = Convert.ToString(row.Cells[COL_NAME].Value) ?? "";
                name = name.Trim();

                ushort? value = TryReadUShortFromCells(row);

                bool hasName = !string.IsNullOrWhiteSpace(name);
                bool hasValue = value.HasValue;

                if (!hasName && !hasValue)
                    continue; // 완전 빈 행은 저장 안 함

                list.Add(new TxRowPreset
                {
                    Address = (ushort)(start + i),
                    Name = name,
                    Value = value
                });
            }

            return list;
        }

        private void RefreshPresetCombo()
        {
            cmbPreset.Items.Clear();

            foreach (var p in FunctionPresetManager.Items)
                cmbPreset.Items.Add(p);

            cmbPreset.SelectedIndex = -1;
        }

        private FunctionPreset? CreatePresetFromUi(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            try
            {
                byte slaveId = (byte)numSlave.Value;

                byte fc = 0x03;
                if (cmbFunctionCode.SelectedItem is string s && s.Length >= 2)
                {
                    string hexPart = s.Substring(0, 2);
                    fc = Convert.ToByte(hexPart, 16);
                }

                ushort startAddr = (ushort)numStartRegister.Value;
                ushort regCount = (ushort)numCount.Value;

                return new FunctionPreset
                {
                    Name = name.Trim(),
                    SlaveId = slaveId,
                    FunctionCode = fc,
                    StartAddress = startAddr,
                    RegisterCount = regCount,
                    TxRows = CaptureTxRows(startAddr, regCount)
                };
            }
            catch
            {
                MessageBox.Show(this,
                    "현재 설정 값에서 프리셋을 만들 수 없습니다.\r\n" +
                    "Slave, Function Code, Start Register, Register Count를 확인해 주세요.",
                    "Preset 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return null;
            }
        }

        private void ApplyPresetToUi(FunctionPreset preset)
        {
            numSlave.Value = Math.Max(numSlave.Minimum,
                                Math.Min(numSlave.Maximum, preset.SlaveId));

            string key = preset.FunctionCode.ToString("X2");
            int foundIndex = -1;
            for (int i = 0; i < cmbFunctionCode.Items.Count; i++)
            {
                if (cmbFunctionCode.Items[i] is string s &&
                    s.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                {
                    foundIndex = i;
                    break;
                }
            }
            if (foundIndex >= 0)
                cmbFunctionCode.SelectedIndex = foundIndex;

            if (preset.StartAddress >= numStartRegister.Minimum &&
                preset.StartAddress <= numStartRegister.Maximum)
            {
                numStartRegister.Value = preset.StartAddress;
            }
            else
            {
                numStartRegister.Value = numStartRegister.Minimum;
            }

            if (preset.RegisterCount >= numCount.Minimum &&
                preset.RegisterCount <= numCount.Maximum)
            {
                numCount.Value = preset.RegisterCount;
            }
            else
            {
                numCount.Value = numCount.Minimum;
            }

            _gridController.ClearTxAll();     // (이 안에서 TX->RX Name 1회 동기화도 수행)
            ApplyPresetTxRows(preset);        // 프리셋 복원 (마지막에 다시 1회 전체 동기화)
        }

        private string PromptForPresetName(string? defaultName = null)
        {
            using (var dlg = new Form())
            using (var txt = new TextBox())
            using (var ok = new Button())
            using (var cancel = new Button())
            {
                dlg.Text = "프리셋 이름 입력";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.ClientSize = new Size(300, 120);

                txt.Left = 10;
                txt.Top = 10;
                txt.Width = dlg.ClientSize.Width - 20;
                txt.Text = defaultName ?? "";

                ok.Text = "OK";
                ok.DialogResult = DialogResult.OK;
                ok.Width = 80;
                ok.Left = dlg.ClientSize.Width - 180;
                ok.Top = 50;

                cancel.Text = "Cancel";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Width = 80;
                cancel.Left = dlg.ClientSize.Width - 90;
                cancel.Top = 50;

                dlg.Controls.Add(txt);
                dlg.Controls.Add(ok);
                dlg.Controls.Add(cancel);
                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                var result = dlg.ShowDialog(this);
                if (result == DialogResult.OK)
                    return txt.Text.Trim();

                return "";
            }
        }
        public bool TryRunPresetByName(string presetName, out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(presetName))
            {
                error = "Preset 이름이 비어 있습니다.";
                return false;
            }

            if (_slaveMode)
            {
                error = "Slave 모드에서는 실행할 수 없습니다.";
                return false;
            }

            if (!_isOpen)
            {
                error = "포트가 OPEN 상태가 아닙니다.";
                return false;
            }

            if (_master == null)
            {
                error = "통신 클라이언트(_master)가 초기화되지 않았습니다.";
                return false;
            }

            // 1) Preset 찾기
            var preset = FunctionPresetManager.Items
                .FirstOrDefault(p => string.Equals(p.Name, presetName, StringComparison.OrdinalIgnoreCase));

            if (preset == null)
            {
                error = $"Preset을 찾을 수 없습니다: {presetName}";
                return false;
            }

            try
            {
                byte slave = preset.SlaveId;
                ushort start = preset.StartAddress;
                ushort count = preset.RegisterCount;
                byte fc = preset.FunctionCode;

                if (!(fc == 0x03 || fc == 0x04 || fc == 0x06 || fc == 0x10))
                {
                    error = $"지원하지 않는 Function Code: 0x{fc:X2}";
                    return false;
                }

                if (count == 0)
                    count = (ushort)Math.Max(1, preset.TxRows?.Count ?? 1);

                if (fc == 0x03 || fc == 0x04)
                {
                    var result = _master.ReadRegisters(slave, (FunctionCode)fc, start, count);

                    Log("TX: " + result.Request.ToHex());
                    Log("RX: " + result.Response.ToHex());

                    UpdateReceiveHeader(result.Response, slave, fc, start, count);
                    _gridController.FillRxGrid(start, result.Values);
                    RegisterCache.UpdateRange(start, result.Values);
                }
                else if (fc == 0x06)
                {
                    ushort val = ReadPresetValue(preset, start);
                    var result = _master.WriteSingleRegister(slave, start, val);

                    Log("TX: " + result.Request.ToHex());
                    Log("RX: " + result.Response.ToHex());

                    UpdateReceiveHeader(result.Response, slave, fc, start, 1);
                    RegisterCache.UpdateRange(start, new ushort[] { val });
                }
                else if (fc == 0x10)
                {
                    ushort[] vals = ReadPresetValues(preset, count);
                    var result = _master.WriteMultipleRegisters(slave, start, vals);

                    Log("TX: " + result.Request.ToHex());
                    Log("RX: " + result.Response.ToHex());

                    UpdateReceiveHeader(result.Response, slave, fc, start, (ushort)vals.Length);
                    RegisterCache.UpdateRange(start, vals);
                }
                else
                {
                    error = $"지원하지 않는 Function Code: 0x{fc:X2}";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private ushort ReadPresetValue(FunctionPreset preset, ushort address)
        {
            var found = preset.TxRows?.FirstOrDefault(r => r.Address == address);
            if (found != null && found.Value.HasValue)
                return found.Value.Value;

            return 0;
        }

        private ushort[] ReadPresetValues(FunctionPreset preset, ushort count)
        {
            ushort start = preset.StartAddress;
            var arr = new ushort[count];

            if (preset.TxRows != null && preset.TxRows.Count > 0)
            {
                var map = preset.TxRows.ToDictionary(r => r.Address, r => r.Value);
                for (int i = 0; i < count; i++)
                {
                    ushort addr = (ushort)(start + i);
                    if (map.TryGetValue(addr, out var value) && value.HasValue)
                        arr[i] = value.Value;
                    else
                        arr[i] = 0;
                }
            }

            return arr;
        }

        private void btnPresetSave_Click(object sender, EventArgs e)
        {
            string defaultName = (cmbPreset.SelectedItem as FunctionPreset)?.Name ?? "";

            string name = PromptForPresetName(defaultName);
            if (string.IsNullOrWhiteSpace(name))
                return;

            var preset = CreatePresetFromUi(name);
            if (preset == null)
                return;

            FunctionPresetManager.AddOrUpdate(preset);
            RefreshPresetCombo();

            foreach (var item in cmbPreset.Items)
            {
                if (item is FunctionPreset fp && fp.Name == preset.Name)
                {
                    cmbPreset.SelectedItem = fp;
                    break;
                }
            }
        }

        private void btnPresetDelete_Click(object sender, EventArgs e)
        {
            if (cmbPreset.SelectedItem is not FunctionPreset preset)
            {
                MessageBox.Show(this, "삭제할 프리셋을 먼저 선택해 주세요.",
                    "Preset", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(this,
                $"'{preset.Name}' 프리셋을 삭제할까요?",
                "Preset 삭제",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            FunctionPresetManager.Delete(preset.Name);
            RefreshPresetCombo();
        }

        private void cmbPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPreset.SelectedItem is not FunctionPreset preset)
                return;

            ApplyPresetToUi(preset);
        }

        // FormMacroSetting.cs 띄우기
        private void btnMacroSetting_Click(object sender, EventArgs e)
        {
            // 이미 열려 있으면 앞으로 가져오기
            if (_macroForm != null && !_macroForm.IsDisposed)
            {
                if (_macroForm.WindowState == FormWindowState.Minimized)
                    _macroForm.WindowState = FormWindowState.Normal;

                _macroForm.BringToFront();
                _macroForm.Activate();
                return;
            }

            // 새로 생성
            _macroForm = new FormMacroSetting(this);
            _macroForm.StartPosition = FormStartPosition.Manual;

            // 메인 폼 왼쪽에 딱 붙여서 배치
            int formW = _macroForm.Width > 0 ? _macroForm.Width : _macroForm.MinimumSize.Width;

            int x = this.Left - formW;
            int y = this.Top;

            if (y < 0) y = 0;

            _macroForm.Location = new Point(x, y);

            // 닫히면 참조 해제
            _macroForm.FormClosed += (_, __) => _macroForm = null;

            // 모델리스로 표시
            _macroForm.Show(this);
        }

        // ───────────────────── RX 헤더 업데이트 ─────────────────────

        private void UpdateReceiveHeader(byte[] resp, byte slave, byte fc, ushort start, ushort count)
        {
            txtRxSlave.Text = slave.ToString();
            txtRxFc.Text = $"{fc:X2}h";
            txtRxStart.Text = $"{start:X4}h";
            txtRxCount.Text = count.ToString();

            if (resp.Length >= 3)
                txtRxDataCount.Text = (fc == 0x03 || fc == 0x04) ? resp[2].ToString() : "0";
            if (resp.Length >= 4)
                txtRxCrc.Text = $"{resp[^1]:X2}{resp[^2]:X2}h";
        }

        // ───────────────────── 종료 처리 ─────────────────────

        private struct PollingConfig
        {
            public byte Slave;
            public byte FunctionCode;
            public ushort Start;
            public ushort Count;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            try { _recService.Dispose(); } catch { }
            try { _slave?.Dispose(); } catch { }

            try
            {
                if (_sp != null)
                {
                    if (_sp.IsOpen)
                        _sp.Close();

                    _sp.Dispose();
                }
            }
            catch { }
        }
    }
}