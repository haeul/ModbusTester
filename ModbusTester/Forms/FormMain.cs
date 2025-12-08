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

        private bool _isOpen => !_slaveMode && _sp != null && _sp.IsOpen;

        public FormMain(SerialPort? sp, ModbusSlave? slave, bool slaveMode, byte slaveId)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;

            // exe에 박혀 있는 아이콘을 그대로 폼 아이콘으로 사용
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _layoutScaler = new LayoutScaler(this);
            _layoutScaler.ApplyInitialScale(1.058f);

            _sp = sp;
            _slave = slave;
            _slaveMode = slaveMode;

            // Recording 디렉토리: 실행 폴더 하위의 Data
            string recDir = Path.Combine(Application.StartupPath, "Data");
            _recService = new RecordingService(recDir);

            // Master 모드에서만 통신 객체 생성
            //if (!_slaveMode)
            //{
            //    if (_sp == null)
            //        throw new ArgumentNullException(nameof(sp), "Master 모드에서는 SerialPort가 필요합니다.");

            //    _client = new SerialModbusClient(_sp);
            //    _master = new ModbusMasterService(_client);
            //    _poller = new ModbusPoller(_client);
            //}

            // Master 모드에서만 통신 객체 생성
            // → 오프라인 UI 용도로 _sp == null 인 경우도 허용
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
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Function Code 기본값
            if (cmbFunctionCode.Items.Count == 0)
                cmbFunctionCode.Items.AddRange(new object[]
                {
                    "03h Read HR",
                    "04h Read IR",
                    "06h Write SR",
                    "10h Write MR"
                });

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
            UpdateUiByMode();
        }

        // Register 첫 줄만 수정 가능하게
        private void Grid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            _gridController.HandleCellBeginEdit(sender, e);
        }

        // HEX/DEC/BIT 포맷 및 Name 동기화
        private void HexAutoFormat_OnEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            _gridController.HandleCellEndEdit(sender, e);
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
                    // Read 계열: ModbusRtu가 전체 프레임(슬레이브+FC+주소+수량+CRC) 생성
                    frame = ModbusRtu.BuildReadFrame(slave, fc, start, count);
                    txtDataCount.Text = "0";
                }
                else if (fc == 0x06)
                {
                    // Write Single: 첫 번째 TX 값 사용
                    ushort val = _gridController.ReadTxValueOrZero(0);
                    frame = ModbusRtu.BuildWriteSingleFrame(slave, start, val);
                    txtDataCount.Text = "2";
                }
                else if (fc == 0x10)
                {
                    // Write Multiple: TX 그리드 값들 사용
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
        }

        private void btnTxClear_Click(object sender, EventArgs e)
        {
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

                byte slave = (byte)numSlave.Value;
                ushort start = (ushort)numStartRegister.Value;
                ushort count = (ushort)numCount.Value;
                byte fc = GetFunctionCode();
                if (!(fc == 0x03 || fc == 0x04)) return;

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
            // 텍스트 추가
            txtLog.AppendText(line + Environment.NewLine);

            // 그 위치로 스크롤
            txtLog.ScrollToCaret();
        }

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
            int seconds = ParseRecordSeconds();   // RecordEvery 설정 (ex: 1, 5, 10, 30, 60)
            if (seconds <= 0) seconds = 60;

            byte slave = (byte)numSlave.Value;
            byte fc = GetFunctionCode();                 // 0x03, 0x04 등
            ushort start = (ushort)numStartRegister.Value;
            ushort count = (ushort)numCount.Value;

            _recService.Start(slave, fc, start, count, seconds);
            Log($"[REC] start every {seconds}s → {_recService.CurrentFilePath}");
        }

        private void UpdateRecordingState()
        {
            // 폴링 + 체크박스 ON 일 때만 녹화
            if (chkRecording.Checked && pollTimer.Enabled)
            {
                if (!_recService.IsRecording)
                {
                    StartRecordingInternal();
                }
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
                int x = Math.Min(this.Right, wa.Right - _quick.Width);
                int y = Math.Max(wa.Top, Math.Min(this.Top, wa.Bottom - _quick.Height));
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
                if (!TryParseUShortFromHex(regText, out ushort addr))
                    continue;

                string name = Convert.ToString(r.Cells[COL_NAME].Value) ?? "";
                string label = !string.IsNullOrWhiteSpace(name)
                               ? name.Trim()
                               : $"{addr:X4}h";

                list.Add((addr, label));
            }

            return list;
        }

        private static bool TryParseUShortFromHex(string s, out ushort value)
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

        private void RefreshPresetCombo()
        {
            cmbPreset.Items.Clear();

            foreach (var p in FunctionPresetManager.Items)
            {
                cmbPreset.Items.Add(p);
            }

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
                    RegisterCount = regCount
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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            
            try
            {
                _recService.Dispose();
            }
            catch { }

            try
            {
                _slave?.Dispose();
            }
            catch { }

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
