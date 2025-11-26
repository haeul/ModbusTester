using System;                        // .NET 기본 네임스페이스(기본 타입, 이벤트 등)
using System.Collections.Generic;    // List<T>, 등
using System.Drawing;               // GDI+ 관련 타입(폼 컨트롤의 색/폰트/이미지 등)
using System.IO;                    // 파일/스트림 입출력 (Recording에 사용)
using System.IO.Ports;              // 시리얼 포트 API
using System.Linq;                  // LINQ (배열/컬렉션 처리에 사용)
using System.Text;                  // 문자열 인코딩 등
using System.Threading;             // Thread.Sleep 등 스레딩 유틸
using System.Threading.Tasks;       // async/await, Task
using System.Windows.Forms;         // WinForms UI 구성요소
using System.Globalization;         // 16진수 파싱(NumberStyles 등)

using ModbusTester.Modbus;
using ModbusTester.Services;

namespace ModbusTester
{
    public partial class FormMain : Form
    {
        // ───────────────────────── Fields ─────────────────────────

        private readonly LayoutScaler _layoutScaler;

        // COM Setting에서 넘겨줌 (Master 모드에서만 사용 가능)
        private readonly SerialPort? _sp;
        private readonly ModbusSlave? _slave;
        private readonly bool _slaveMode;

        // Master 모드일 때만 사용되는 통신 클라이언트 & 폴러
        private readonly SerialModbusClient? _client;
        private readonly ModbusPoller? _poller;

        // 포트 열림 여부 (Master + _sp != null + IsOpen)
        private bool _isOpen => !_slaveMode && _sp != null && _sp.IsOpen;

        // Recording 
        private readonly RecordingService _recService =
            new RecordingService("C:\\Users\\haeul\\source\\repos\\ModbusTester\\Data");

        // QuickView 폼
        private FormQuickView? _quick;

        // Register 주소 범위
        private const ushort RegisterMin = 0x0000;
        private const ushort RegisterMax = 0xFFFF;
        private const ushort GridRegisterMax = 0x03FF;

        // 그리드 열 인덱스 (Register, Name, HEX, DEC, BIT)
        private const int COL_REG = 0;
        private const int COL_NAME = 1;
        private const int COL_HEX = 2;
        private const int COL_DEC = 3;
        private const int COL_BIT = 4;

        // TX 그리드 실행취소용 스냅샷
        private class TxRowSnapshot
        {
            public string Reg = "";
            public string Name = "";
            public string Hex = "";
            public string Dec = "";
            public string Bit = "";
        }
        private List<TxRowSnapshot>? _lastTxSnapshot;

        // ───────────────────────── 생성자 ─────────────────────────

        public FormMain(SerialPort? sp, ModbusSlave? slave, bool slaveMode, byte slaveId)
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;

            _layoutScaler = new LayoutScaler(this);
            _layoutScaler.ApplyInitialScale(1.0f);

            _sp = sp;
            _slave = slave;
            _slaveMode = slaveMode;

            // Master 모드일 때만 SerialModbusClient & ModbusPoller 생성
            if (!_slaveMode)
            {
                if (_sp == null)
                    throw new ArgumentNullException(nameof(sp), "Master 모드에서는 SerialPort가 필요합니다.");

                _client = new SerialModbusClient(_sp);
                _poller = new ModbusPoller(_client);
            }

            Shown += FormMain_Shown;
            numSlave.Value = slaveId;
        }

        // 폼이 화면에 표시된 직후 호출됨
        private async void FormMain_Shown(object? sender, EventArgs e)
        {
            // 그리드를 비동기로 채워서 "폼은 바로 뜨고", 행 추가는 백그라운드처럼 진행
            await InitializeGridsAsync();
        }

        // ───────────────────────── Load & 초기화 ─────────────────────────

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Function Code 기본값
            if (cmbFunctionCode.Items.Count == 0)
                cmbFunctionCode.Items.AddRange(new object[] { "03h Read HR", "04h Read IR", "06h Write SR", "10h Write MR" });
            if (cmbFunctionCode.SelectedIndex < 0)
                cmbFunctionCode.SelectedIndex = 0;         // 기본 03h 선택

            // TX 기본
            numStartRegister.Hexadecimal = true;
            numStartRegister.Minimum = 0x0000;
            numStartRegister.Maximum = 0xFFFF;
            numStartRegister.Value = 0x0000;
            numCount.Value = 1;
            RefreshDataCount();

            // 그리드 공통 스타일
            SetupGrid(gridTx);
            SetupGrid(gridRx);

            // QV 컬럼 화면 왼쪽으로
            var qvCol = gridRx.Columns["colRxQuickView"];
            if (qvCol != null)
                qvCol.DisplayIndex = 0;

            // 모드에 따른 UI 반영
            UpdateUiByMode();

            // 프리셋 로드
            FunctionPresetManager.Load();
            RefreshPresetCombo();
        }

        // 폼이 뜬 뒤 TX/RX 그리드에 0000h ~ GridRegisterMax 주소를 비동기로 채워 넣는 메서드
        private async Task InitializeGridsAsync()
        {
            gridTx.SuspendLayout();
            gridRx.SuspendLayout();
            try
            {
                gridTx.Rows.Clear();
                gridRx.Rows.Clear();

                const int chunk = 512;
                int counter = 0;

                for (int addr = RegisterMin; addr <= GridRegisterMax; addr++)
                {
                    string reg = $"{addr:X4}h";
                    gridTx.Rows.Add(reg, "", "", "", ""); // REG, NAME, HEX, DEC, BIT
                    gridRx.Rows.Add(reg, "", "", "", "");

                    counter++;
                    if (counter % chunk == 0)
                        await Task.Yield();
                }
            }
            finally
            {
                gridTx.ResumeLayout();
                gridRx.ResumeLayout();
            }
        }

        // ───────────────────────── 모드/공통 UI ─────────────────────────

        // 모드(마스터/슬레이브)에 따른 UI 잠금
        private void UpdateUiByMode()
        {
            bool slave = _slaveMode;

            btnSend.Enabled = !slave;
            btnCalcCrc.Enabled = !slave;
            btnPollStart.Enabled = !slave;
            btnPollStop.Enabled = !slave;

            gridRx.Enabled = !slave;

            Log(slave ? "[MODE] Slave 모드 (해당 포트로 들어오는 요청에 응답)"
                      : "[MODE] Master 모드 (요청을 전송)");
        }

        private void chkSlaveMode_CheckedChanged(object sender, EventArgs e)
        {
            pollTimer.Stop();
            UpdateUiByMode();
        }

        // 그리드 공통 셋업
        private void SetupGrid(DataGridView g)
        {
            g.AllowUserToAddRows = false;
            g.RowHeadersVisible = false;

            g.SelectionMode = DataGridViewSelectionMode.CellSelect;
            g.EditMode = DataGridViewEditMode.EditOnEnter;

            // 전체는 비율로 채우기
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // 기본 가운데 정렬
            g.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            g.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 정렬 막기
            foreach (DataGridViewColumn col in g.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;

            // Register / Name / HEX / DEC / BIT 비율 설정
            if (g.Columns.Count >= 5)
            {
                g.Columns[COL_REG].FillWeight = 80;   // Register
                g.Columns[COL_NAME].FillWeight = 100; // Name
                g.Columns[COL_HEX].FillWeight = 80;   // HEX
                g.Columns[COL_DEC].FillWeight = 80;   // DEC
                g.Columns[COL_BIT].FillWeight = 180;  // BIT

                g.Columns[COL_REG].ReadOnly = false;
                g.Columns[COL_NAME].ReadOnly = false;
                g.Columns[COL_HEX].ReadOnly = false;
                g.Columns[COL_DEC].ReadOnly = false;
                g.Columns[COL_BIT].ReadOnly = false;
            }

            // QV 체크박스 컬럼은 있으면 따로 고정폭으로 처리
            var qvCol = g.Columns["colRxQuickView"];
            if (qvCol != null)
            {
                qvCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                qvCol.Width = 30;              // 체크박스 딱 맞는 폭
                qvCol.ReadOnly = false;        // 체크 가능해야 하니까
            }
        }

        // Register 컬럼은 첫 줄만 편집 가능하게 만들기 위한 핸들러
        private void Grid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            var g = (DataGridView)sender;

            if (e.ColumnIndex == COL_REG && e.RowIndex != 0)
            {
                e.Cancel = true;
                return;
            }
        }

        // ───────────────────────── BIT / 값 변환 ─────────────────────────

        private static string ToBitString(ushort v)
        {
            // "0000000000000000" → "0000 0000 0000 0000"
            string s = Convert.ToString(v, 2).PadLeft(16, '0');
            return string.Join(" ", Enumerable.Range(0, 4).Select(i => s.Substring(i * 4, 4)));
        }

        private void UpdateBitCell(DataGridViewRow row)
        {
            if (row == null || row.IsNewRow) return;

            string hex = Convert.ToString(row.Cells[COL_HEX].Value) ?? "";
            string dec = Convert.ToString(row.Cells[COL_DEC].Value) ?? "";

            ushort v;
            if (!string.IsNullOrWhiteSpace(hex) && TryParseUShortFromHex(hex, out ushort hv))
                v = hv;
            else if (!string.IsNullOrWhiteSpace(dec) && ushort.TryParse(dec.Trim(), out ushort dv))
                v = dv;
            else
            {
                row.Cells[COL_BIT].Value = "";
                return;
            }

            row.Cells[COL_BIT].Value = ToBitString(v);
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

        private static bool TryParseUShortFromBitString(string s, out ushort value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;

            // 0,1만 남기고 나머지(공백 등)는 제거
            s = new string(s.Where(ch => ch == '0' || ch == '1').ToArray());
            if (s.Length == 0) return false;

            // 16비트 초과면 뒤쪽 16비트만 사용
            if (s.Length > 16)
                s = s[^16..];

            // 16비트 미만이면 왼쪽을 0으로 채움
            s = s.PadLeft(16, '0');

            try
            {
                value = Convert.ToUInt16(s, 2);   // 2진수 → ushort
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ───────────────────────── 버튼/이벤트 (TX/RX) ─────────────────────────

        private void btnCalcCrc_Click(object sender, EventArgs e)
        {
            try
            {
                byte slave = (byte)numSlave.Value;
                ushort start = (ushort)numStartRegister.Value;
                ushort count = (ushort)numCount.Value;
                byte fc = GetFunctionCode();

                byte[] frameBody;

                if (fc == 0x03 || fc == 0x04)
                {
                    frameBody = new byte[]
                    {
                        slave, fc,
                        (byte)(start >> 8), (byte)(start & 0xFF),
                        (byte)(count >> 8), (byte)(count & 0xFF)
                    };
                    txtDataCount.Text = "0";
                }
                else if (fc == 0x06)
                {
                    ushort val = ReadTxValueOrZero(0);
                    frameBody = new byte[]
                    {
                        slave, fc,
                        (byte)(start >> 8), (byte)(start & 0xFF),
                        (byte)(val >> 8), (byte)(val & 0xFF)
                    };
                    txtDataCount.Text = "2";
                }
                else if (fc == 0x10)
                {
                    ushort[] vals = ReadTxValues(count);
                    byte byteCount = (byte)(vals.Length * 2);
                    var temp = new byte[7 + byteCount];
                    temp[0] = slave;
                    temp[1] = fc;
                    temp[2] = (byte)(start >> 8);
                    temp[3] = (byte)(start & 0xFF);
                    temp[4] = (byte)(count >> 8);
                    temp[5] = (byte)(count & 0xFF);
                    temp[6] = byteCount;

                    for (int i = 0; i < vals.Length; i++)
                    {
                        temp[7 + i * 2] = (byte)(vals[i] >> 8);
                        temp[8 + i * 2] = (byte)(vals[i] & 0xFF);
                    }
                    frameBody = temp;
                    txtDataCount.Text = byteCount.ToString();
                }
                else
                {
                    throw new NotSupportedException("지원하지 않는 Function Code");
                }

                ushort crc = ModbusRtu.Crc16(frameBody, frameBody.Length);
                byte crcLo = (byte)(crc & 0xFF);
                byte crcHi = (byte)((crc >> 8) & 0xFF);

                txtCrc.Text = $"{crcLo:X2} {crcHi:X2}";
                Log($"[CRC CALC] {ToHex(frameBody)} {crcLo:X2} {crcHi:X2}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("CRC 계산 실패: " + ex.Message);
            }
        }

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
            if (_client == null)
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

                byte[] req;

                if (fc == 0x03 || fc == 0x04)
                {
                    req = ModbusRtu.BuildReadFrame(slave, fc, start, count);
                    Log("TX: " + ToHex(req));
                    var resp = _client.SendAndReceive(req);
                    Log("RX: " + ToHex(resp));

                    if (resp.Length >= 3 && resp[1] == (byte)(fc | 0x80))
                        throw new Exception($"장치 오류 (예외코드: {resp[2]})");

                    UpdateReceiveHeader(resp, slave, fc, start, count);
                    var values = ModbusRtu.ParseReadResponse(resp);
                    FillRxGrid(start, values);

                    RegisterCache.UpdateRange(start, values);
                }
                else if (fc == 0x06)
                {
                    ushort val = ReadTxValueOrZero(0);
                    req = ModbusRtu.BuildWriteSingleFrame(slave, start, val);
                    Log("TX: " + ToHex(req));
                    var resp = _client.SendAndReceive(req);
                    Log("RX: " + ToHex(resp));
                    UpdateReceiveHeader(resp, slave, fc, start, 1);

                    RegisterCache.UpdateRange(start, new ushort[] { val });
                }
                else if (fc == 0x10)
                {
                    ushort[] vals = ReadTxValues(count);
                    req = ModbusRtu.BuildWriteMultipleFrame(slave, start, vals);
                    Log("TX: " + ToHex(req));
                    var resp = _client.SendAndReceive(req);
                    Log("RX: " + ToHex(resp));
                    UpdateReceiveHeader(resp, slave, fc, start, (ushort)vals.Length);

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

        private void btnRestore_Click(object sender, EventArgs e)
        {
            RestoreTxSnapshot();
        }

        private void btnTxClear_Click(object sender, EventArgs e)
        {
            // 실행취소를 위해 현재 상태 백업
            SaveTxSnapshot();

            // 값만 Clear (Register / Name 은 유지)
            foreach (DataGridViewRow r in gridTx.Rows)
            {
                if (r.IsNewRow) continue;
                r.Cells[COL_HEX].Value = "";
                r.Cells[COL_DEC].Value = "";
                r.Cells[COL_BIT].Value = "";
            }
        }

        private void btnRxClear_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow r in gridRx.Rows)
            {
                if (r.IsNewRow) continue;
                r.Cells[COL_HEX].Value = "";
                r.Cells[COL_DEC].Value = "";
                r.Cells[COL_BIT].Value = "";
            }

            txtRxSlave.Clear();
            txtRxFc.Clear();
            txtRxStart.Clear();
            txtRxCount.Clear();
            txtRxDataCount.Clear();
            txtRxCrc.Clear();
        }

        private void btnCopyToTx_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow rx in gridRx.Rows)
            {
                if (rx.IsNewRow) continue;

                string reg = Convert.ToString(rx.Cells[COL_REG].Value) ?? "";
                string hex = Convert.ToString(rx.Cells[COL_HEX].Value) ?? "";
                string dec = Convert.ToString(rx.Cells[COL_DEC].Value) ?? "";

                if (string.IsNullOrWhiteSpace(hex) && string.IsNullOrWhiteSpace(dec))
                    continue;

                foreach (DataGridViewRow tx in gridTx.Rows)
                {
                    if (tx.IsNewRow) continue;
                    if (Convert.ToString(tx.Cells[COL_REG].Value) == reg)
                    {
                        tx.Cells[COL_HEX].Value = hex;
                        tx.Cells[COL_DEC].Value = dec;
                        UpdateBitCell(tx);
                        break;
                    }
                }
            }
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

        // ───────────────────────── 폴링 ─────────────────────────

        private void btnPollStart_Click(object sender, EventArgs e)
        {
            if (_slaveMode)
            {
                MessageBox.Show("Slave 모드에서는 폴링을 사용할 수 없습니다.");
                return;
            }

            pollTimer.Interval = (int)numInterval.Value;
            pollTimer.Start();

            // 폴링 상태가 바뀌었으니까 녹화 상태도 재조정
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

                Log("TX: " + ToHex(result.Request));
                Log("RX: " + ToHex(result.Response));

                if (result.IsException)
                    return;

                UpdateReceiveHeader(result.Response, slave, fc, start, count);
                FillRxGrid(start, result.Values);

                RegisterCache.UpdateRange(start, result.Values);
            }
            catch
            {
                // 폴링에서는 조용히 무시
            }
        }

        private void FillRxGrid(ushort startAddr, ushort[] values)
        {
            if (values == null || values.Length == 0) return;

            for (int i = 0; i < values.Length; i++)
            {
                ushort addr = (ushort)(startAddr + i);
                if (addr < 0x0000 || addr > 0xFFFF) continue;

                string key = $"{addr:X4}h";

                foreach (DataGridViewRow r in gridRx.Rows)
                {
                    if (r.IsNewRow) continue;
                    if (Convert.ToString(r.Cells[COL_REG].Value) == key)
                    {
                        r.Cells[COL_HEX].Value = $"{values[i]:X4}h";
                        r.Cells[COL_DEC].Value = values[i].ToString();
                        UpdateBitCell(r);
                        break;
                    }
                }
            }
        }

        // ───────────────────────── 로그/공통 Helper ─────────────────────────

        private void Log(string line)
        {
            txtLog.AppendText(line + Environment.NewLine);
            _recService.Log(line);
        }

        private static string ToHex(byte[] bytes)
            => BitConverter.ToString(bytes).Replace("-", " ");

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

        private ushort[] ReadTxValues(ushort count)
        {
            return Enumerable.Range(0, count).Select(ReadTxValueOrZero).ToArray();
        }

        private ushort ReadTxValueOrZero(int rowIndex)
        {
            if (rowIndex >= gridTx.Rows.Count) return 0;
            string hex = Convert.ToString(gridTx.Rows[rowIndex].Cells[COL_HEX].Value) ?? "";
            string dec = Convert.ToString(gridTx.Rows[rowIndex].Cells[COL_DEC].Value) ?? "";

            if (!string.IsNullOrWhiteSpace(hex) && TryParseUShortFromHex(hex, out ushort v1))
                return v1;
            if (!string.IsNullOrWhiteSpace(dec) && ushort.TryParse(dec.Trim(), out ushort v2))
                return v2;
            return 0;
        }

        private void UpdateReceiveHeader(byte[] resp, byte slave, byte fc, ushort start, ushort count)
        {
            txtRxSlave.Text = slave.ToString();
            txtRxFc.Text = $"0x{fc:X2}";
            txtRxStart.Text = $"0x{start:X4}";
            txtRxCount.Text = count.ToString();

            if (resp.Length >= 3)
                txtRxDataCount.Text = (fc == 0x03 || fc == 0x04) ? resp[2].ToString() : "0";
            if (resp.Length >= 4)
                txtRxCrc.Text = $"{resp[^2]:X2} {resp[^1]:X2}";
        }

        // ───────── Register 첫 줄 입력 기준으로 Register 컬럼 재배열 ─────────

        private void RebuildRegisterColumnFromFirstRow(DataGridView grid)
        {
            if (grid.Rows.Count == 0) return;

            string raw = Convert.ToString(grid.Rows[0].Cells[COL_REG].Value) ?? "";
            raw = raw.Trim();

            if (raw.Length == 0)
                raw = "0";

            if (raw.EndsWith("h", StringComparison.OrdinalIgnoreCase))
                raw = raw[..^1];
            if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                raw = raw[2..];

            int idx = 0;
            while (idx < raw.Length - 1 && raw[idx] == '0')
                idx++;
            raw = raw[idx..];

            if (raw.Length == 0)
                raw = "0";

            string normalized = raw.ToUpperInvariant();

            if (!normalized.All(c => Uri.IsHexDigit(c)))
            {
                normalized = "0";
            }

            int startInt;
            if (!int.TryParse(normalized, NumberStyles.HexNumber,
                              CultureInfo.InvariantCulture, out startInt))
            {
                startInt = 0;
            }

            if (startInt < RegisterMin) startInt = RegisterMin;
            if (startInt > RegisterMax) startInt = RegisterMax;

            ushort numericStart = (ushort)startInt;

            grid.Rows[0].Cells[COL_REG].Value = $"{numericStart:X4}h";

            ushort addr = numericStart;
            bool wrapped = false;

            for (int r = 1; r < grid.Rows.Count; r++)
            {
                var row = grid.Rows[r];
                if (row.IsNewRow) continue;

                addr = NextAddress(addr, ref wrapped);
                row.Cells[COL_REG].Value = $"{addr:X4}h";
            }

            if (ReferenceEquals(grid, gridTx))
            {
                numStartRegister.Value = numericStart;
            }
        }

        private static ushort NextAddress(ushort current, ref bool wrapped)
        {
            if (!wrapped)
            {
                if (current < RegisterMax)
                {
                    current++;
                }
                else
                {
                    wrapped = true;
                    current = RegisterMin;
                }
            }
            else
            {
                if (current < RegisterMax)
                    current++;
                else
                    current = RegisterMin;
            }
            return current;
        }

        private void HexAutoFormat_OnEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var g = (DataGridView)sender;
            if (e.RowIndex < 0) return;

            // TX Name 변경 → RX Name 동기화
            if (e.ColumnIndex == COL_NAME && ReferenceEquals(g, gridTx))
            {
                var row = g.Rows[e.RowIndex];
                if (row.IsNewRow) return;

                string reg = Convert.ToString(row.Cells[COL_REG].Value) ?? "";
                string name = Convert.ToString(row.Cells[COL_NAME].Value) ?? "";

                if (string.IsNullOrEmpty(reg))
                    return;

                foreach (DataGridViewRow rxRow in gridRx.Rows)
                {
                    if (rxRow.IsNewRow) continue;
                    if (Convert.ToString(rxRow.Cells[COL_REG].Value) == reg)
                    {
                        rxRow.Cells[COL_NAME].Value = name;
                        break;
                    }
                }

                return; // Name 편집은 여기서 끝
            }

            // 첫 줄 Register 수정 → 시작주소 재배열
            if (e.ColumnIndex == COL_REG && e.RowIndex == 0)
            {
                RebuildRegisterColumnFromFirstRow(g);
                return;
            }

            if (e.ColumnIndex == COL_HEX)
            {
                var hexCell = g.Rows[e.RowIndex].Cells[COL_HEX];
                var decCell = g.Rows[e.RowIndex].Cells[COL_DEC];

                string raw = Convert.ToString(hexCell.Value) ?? "";
                raw = raw.Trim();

                if (raw.Length == 0) { decCell.Value = ""; UpdateBitCell(g.Rows[e.RowIndex]); return; }

                raw = raw.TrimEnd('h', 'H');
                if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    raw = raw.Substring(2);

                raw = new string(raw.Where(ch =>
                         (ch >= '0' && ch <= '9') ||
                         (ch >= 'a' && ch <= 'f') ||
                         (ch >= 'A' && ch <= 'F')).ToArray());

                if (raw.Length == 0) { hexCell.Value = ""; decCell.Value = ""; UpdateBitCell(g.Rows[e.RowIndex]); return; }

                if (ushort.TryParse(raw, NumberStyles.HexNumber, null, out ushort v))
                {
                    hexCell.Value = $"{v:X4}h";
                    decCell.Value = v.ToString();
                }
                else
                {
                    hexCell.Value = "";
                    decCell.Value = "";
                }

                UpdateBitCell(g.Rows[e.RowIndex]);
            }
            else if (e.ColumnIndex == COL_DEC)
            {
                var hexCell = g.Rows[e.RowIndex].Cells[COL_HEX];
                var decCell = g.Rows[e.RowIndex].Cells[COL_DEC];

                string raw = Convert.ToString(decCell.Value) ?? "";
                raw = raw.Trim();

                if (raw.Length == 0) { hexCell.Value = ""; UpdateBitCell(g.Rows[e.RowIndex]); return; }

                if (ushort.TryParse(raw, out ushort v))
                {
                    hexCell.Value = $"{v:X4}h";
                    decCell.Value = v.ToString();
                }
                else
                {
                    decCell.Value = "";
                    hexCell.Value = "";
                }

                UpdateBitCell(g.Rows[e.RowIndex]);
            }
            else if (e.ColumnIndex == COL_BIT)
            {
                // RX 그리드에서는 BIT 편집을 무시 (TX에서만 허용)
                if (!ReferenceEquals(g, gridTx))
                    return;

                var bitCell = g.Rows[e.RowIndex].Cells[COL_BIT];
                var hexCell = g.Rows[e.RowIndex].Cells[COL_HEX];
                var decCell = g.Rows[e.RowIndex].Cells[COL_DEC];

                string raw = Convert.ToString(bitCell.Value) ?? "";
                raw = raw.Trim();

                if (raw.Length == 0)
                {
                    hexCell.Value = "";
                    decCell.Value = "";
                    return;
                }

                if (TryParseUShortFromBitString(raw, out ushort v))
                {
                    bitCell.Value = ToBitString(v);
                    hexCell.Value = $"{v:X4}h";
                    decCell.Value = v.ToString();
                }
                else
                {
                    bitCell.Value = "";
                    hexCell.Value = "";
                    decCell.Value = "";
                }
            }
        }

        // ───────────────────────── Recording 구현 ─────────────────────────

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
                UpdateRecordingState();   // 에러났으니 상태 다시 정리
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

            _recService.Start(seconds);
            Log($"[REC] start {seconds}s → {_recService.CurrentFilePath}");
        }

        // FormMain 안 어딘가에 추가
        private void UpdateRecordingState()
        {
            // 1) 녹화를 해야 하는 조건: 체크박스 ON + 폴링 ON
            if (chkRecording.Checked && pollTimer.Enabled)
            {
                if (!_recService.IsRecording)
                {
                    StartRecordingInternal();
                }
            }
            // 2) 그 외에는 녹화 중이면 꺼야 함
            else
            {
                if (_recService.IsRecording)
                {
                    _recService.Stop();
                    Log("[REC] stop");
                }
            }
        }


        // ───────────────────────── QuickView ─────────────────────────

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
                // 새로 생성해서 타깃 설정 후 표시
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
                // 이미 열려 있으면 타깃만 교체
                _quick.UpdateTargets(targets);
                _quick.Focus();
            }
        }

        // RX Grid에서 QuickView 체크된 항목들 가져오기
        // label: Name이 있으면 Name, 없으면 주소(XXXXh)
        private List<(ushort addr, string label)> GetRxQuickViewTargets()
        {
            var list = new List<(ushort addr, string label)>();

            // QV 체크박스 컬럼 인덱스 (이름이 colRxQuickView 라고 가정)
            int selColIndex = gridRx.Columns["colRxQuickView"]?.Index ?? -1;
            if (selColIndex < 0) return list;

            foreach (DataGridViewRow r in gridRx.Rows)
            {
                if (r.IsNewRow) continue;

                // 체크 여부
                bool isChecked = r.Cells[selColIndex].Value is bool b && b;
                if (!isChecked) continue;

                // Register: "0001h" → addr
                string regText = Convert.ToString(r.Cells[COL_REG].Value) ?? "";
                if (!TryParseUShortFromHex(regText, out ushort addr))
                    continue;

                // 닉네임 있으면 그걸, 없으면 주소
                string name = Convert.ToString(r.Cells[COL_NAME].Value) ?? "";
                string label = !string.IsNullOrWhiteSpace(name)
                               ? name.Trim()
                               : $"{addr:X4}h";

                list.Add((addr, label));
            }

            return list;
        }

        // ───────────────────────── Preset 기능 ─────────────────────────

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

        // ───────────────────────── TX 스냅샷 ─────────────────────────

        private void SaveTxSnapshot()
        {
            var list = new List<TxRowSnapshot>();

            foreach (DataGridViewRow r in gridTx.Rows)
            {
                if (r.IsNewRow) continue;

                list.Add(new TxRowSnapshot
                {
                    Reg = Convert.ToString(r.Cells[COL_REG].Value) ?? "",
                    Name = Convert.ToString(r.Cells[COL_NAME].Value) ?? "",
                    Hex = Convert.ToString(r.Cells[COL_HEX].Value) ?? "",
                    Dec = Convert.ToString(r.Cells[COL_DEC].Value) ?? "",
                    Bit = Convert.ToString(r.Cells[COL_BIT].Value) ?? "",
                });
            }

            _lastTxSnapshot = list;
        }

        private void RestoreTxSnapshot()
        {
            if (_lastTxSnapshot == null) return;
            if (gridTx.Rows.Count == 0) return;

            int idx = 0;

            foreach (DataGridViewRow r in gridTx.Rows)
            {
                if (r.IsNewRow) continue;
                if (idx >= _lastTxSnapshot.Count) break;

                var snap = _lastTxSnapshot[idx++];

                r.Cells[COL_REG].Value = snap.Reg;
                r.Cells[COL_NAME].Value = snap.Name;
                r.Cells[COL_HEX].Value = snap.Hex;
                r.Cells[COL_DEC].Value = snap.Dec;
                r.Cells[COL_BIT].Value = snap.Bit;
            }
        }
    }
}
