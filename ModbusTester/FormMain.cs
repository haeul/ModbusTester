using System;
using System.Drawing;
using System.IO;            // [추가]
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormMain : Form
    {
        // ───────────────────────── Fields ─────────────────────────
        private readonly SerialPort _sp = new SerialPort();
        private bool _isOpen => _sp.IsOpen;

        // Slave 모드용 에뮬레이터 인스턴스
        private ModbusSlave _slave;

        // Recording 관련 필드
        private StreamWriter _recWriter = null;
        private DateTime _recUntil;
        private readonly System.Windows.Forms.Timer _recTimer = new System.Windows.Forms.Timer();

        // Quick View
        private FormQuickView? _quick;

        // ───────────────────────── Ctor ─────────────────────────
        public FormMain()
        {
            InitializeComponent();
        }

        // 모드(마스터/슬레이브)에 따른 UI 잠금
        private void UpdateUiByMode()
        {
            bool slave = chkSlaveMode.Checked;

            // Slave 모드일 때 마스터 전송 관련 UI 비활성화
            btnSend.Enabled = !slave;
            btnCalcCrc.Enabled = !slave;
            btnPollStart.Enabled = !slave;
            btnPollStop.Enabled = !slave;

            gridRx.Enabled = !slave;

            Log(slave ? "[MODE] Slave 모드 (해당 포트로 들어오는 요청에 응답)"
                      : "[MODE] Master 모드 (요청을 전송)");
        }

        // ───────────────────────── Load ─────────────────────────
        private void FormMain_Load(object sender, EventArgs e)
        {
            // COM 포트 목록
            if (cmbPort.Items.Count == 0)
                cmbPort.Items.AddRange(SerialPort.GetPortNames());
            if (cmbPort.Items.Count > 0) cmbPort.SelectedIndex = 0;

            // Baud / Parity / DataBits / StopBits
            cmbBaud.Items.Clear();
            cmbBaud.Items.AddRange(new object[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 });
            cmbBaud.SelectedItem = 9600;

            cmbParity.DataSource = Enum.GetValues(typeof(Parity));
            cmbDataBits.Items.Clear();
            cmbDataBits.Items.AddRange(new object[] { 5, 6, 7, 8 });
            cmbDataBits.SelectedItem = 8;
            cmbStopBits.DataSource = Enum.GetValues(typeof(StopBits));
            cmbStopBits.SelectedItem = StopBits.One;

            // Function Code 기본값
            if (cmbFunctionCode.Items.Count == 0)
                cmbFunctionCode.Items.AddRange(new object[] { "03h Read HR", "04h Read IR", "06h Write SR", "10h Write MR" });
            if (cmbFunctionCode.SelectedIndex < 0)
                cmbFunctionCode.SelectedIndex = 0;

            // TX 기본
            numSlave.Value = 1;
            numStartRegister.Hexadecimal = true;
            numStartRegister.Minimum = 0x0001;
            numStartRegister.Maximum = 0x11FF;
            numStartRegister.Value = 0x0001;
            numCount.Value = 1;
            RefreshDataCount();

            // 그리드 공통 스타일
            SetupGrid(gridTx);
            SetupGrid(gridRx);

            // TX 행 0001h~11FFh
            InitializeTxRows();

            // RX 행 0001h~11FFh
            InitializeRxRows();

            // 모드에 따른 UI 반영
            UpdateUiByMode();

            // Recording 타이머 설정
            _recTimer.Interval = 200; // 종료시점 주기 체크
            _recTimer.Tick += (s, ev) =>
            {
                if (_recWriter == null) { _recTimer.Stop(); return; }
                if (DateTime.Now >= _recUntil) StopRecording();
            };
        }

        // 체크박스 핸들러
        private void chkSlaveMode_CheckedChanged(object sender, EventArgs e)
        {
            pollTimer.Stop();
            UpdateUiByMode();
        }

        // ───────────────────────── UI Helpers ─────────────────────────
        private void SetupGrid(DataGridView g)
        {
            g.AllowUserToAddRows = false;
            g.RowHeadersVisible = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // 3열 고정(Register, HEX, DEC)
            if (g.Columns.Count == 3)
            {
                g.Columns[0].FillWeight = 110; // Register
                g.Columns[1].FillWeight = 90;  // HEX
                g.Columns[2].FillWeight = 90;  // DEC
            }
        }

        private void InitializeTxRows()
        {
            gridTx.Rows.Clear();
            for (int i = 0x0001; i <= 0x11FF; i++)
                gridTx.Rows.Add($"{i:X4}h", "", "");
        }

        private void InitializeRxRows()
        {
            gridRx.Rows.Clear();
            for (int i = 0x0001; i <= 0x11fF; i++)
                gridRx.Rows.Add($"{i:X4}h", "", "");
        }

        // ───────────────────────── Events (COM) ─────────────────────────
        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                pollTimer.Stop();
                // 양쪽 다 닫고 시작 (충돌 방지)
                if (_isOpen) _sp.Close();
                _slave?.Close();

                if (chkSlaveMode.Checked)
                {
                    // ───── Slave 모드: 같은 COM 설정으로 슬레이브 열기 ─────
                    _slave = new ModbusSlave();
                    _slave.InitDemoData();
                    _slave.Open(
                        portName: cmbPort.Text,
                        baud: int.Parse(cmbBaud.Text),
                        parity: (Parity)(cmbParity.SelectedItem ?? Parity.None),
                        dataBits: (int)(cmbDataBits.SelectedItem ?? 8),
                        stopBits: (StopBits)(cmbStopBits.SelectedItem ?? StopBits.One),
                        slaveId: (byte)numSlave.Value
                    );
                    Log($"[SLAVE] OPEN {cmbPort.Text} (ID={numSlave.Value})");
                }
                else
                {
                    // ───── Master 모드: 기존 로직 그대로 ─────
                    _sp.PortName = cmbPort.Text;
                    _sp.BaudRate = int.Parse(cmbBaud.Text);
                    _sp.Parity = (Parity)(cmbParity.SelectedItem ?? Parity.None);
                    _sp.DataBits = (int)(cmbDataBits.SelectedItem ?? 8);
                    _sp.StopBits = (StopBits)(cmbStopBits.SelectedItem ?? StopBits.One);
                    _sp.ReadTimeout = 500;
                    _sp.WriteTimeout = 500;

                    _sp.Open();
                    Log("[MASTER] PORT OPEN " + cmbPort.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("포트 열기 실패: " + ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                pollTimer.Stop();

                // 레코딩 중이면 먼저 안전 종료
                StopRecording();

                if (chkSlaveMode.Checked)
                {
                    _slave?.Close();
                    _slave = null;
                    Log("[SLAVE] CLOSE");
                }
                else
                {
                    if (_isOpen) _sp.Close();
                    Log("[MASTER] PORT CLOSE");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("포트 닫기 실패: " + ex.Message);
            }
        }

        // ───────────────────────── Events (TX/RX Controls) ─────────────────────────
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
                    // Read Frame (CRC 제외하고 본문만 구성)
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

                // CRC 계산
                ushort crc = Crc16(frameBody, frameBody.Length);
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
            if (chkSlaveMode.Checked)
            {
                MessageBox.Show("지금은 Slave 모드입니다. Master 모드에서 전송하세요.");
                return;
            }
            if (!_isOpen) { MessageBox.Show("먼저 포트를 OPEN 하세요."); return; }

            try
            {
                byte slave = (byte)numSlave.Value;
                ushort start = (ushort)numStartRegister.Value;
                ushort count = (ushort)numCount.Value;
                byte fc = GetFunctionCode();

                byte[] req;

                if (fc == 0x03 || fc == 0x04)
                {
                    req = BuildReadFrame(slave, fc, start, count);
                    Log("TX: " + ToHex(req));
                    var resp = SendAndReceive(req);
                    Log("RX: " + ToHex(resp));

                    if (resp.Length >= 3 && resp[1] == (byte)(fc | 0x80))
                        throw new Exception($"장치 오류 (예외코드: {resp[2]})");

                    UpdateReceiveHeader(resp, slave, fc, start, count);
                    var values = ParseReadResponse(resp);
                    FillRxGrid(start, values);

                    RegisterCache.UpdateRange(start, values); // 캐시 갱신
                }
                else if (fc == 0x06)
                {
                    ushort val = ReadTxValueOrZero(0);
                    req = BuildWriteSingleFrame(slave, start, val);
                    Log("TX: " + ToHex(req));
                    var resp = SendAndReceive(req);
                    Log("RX: " + ToHex(resp));
                    UpdateReceiveHeader(resp, slave, fc, start, 1);

                    RegisterCache.UpdateRange(start, new ushort[] { val });
                }
                else if (fc == 0x10)
                {
                    ushort[] vals = ReadTxValues(count);
                    req = BuildWriteMultipleFrame(slave, start, vals);
                    Log("TX: " + ToHex(req));
                    var resp = SendAndReceive(req);
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

        private void btnRxClear_Click(object sender, EventArgs e)
        {
            // 행 유지, 값만 초기화
            foreach (DataGridViewRow r in gridRx.Rows)
            {
                if (r.IsNewRow) continue;
                r.Cells[1].Value = ""; // HEX
                r.Cells[2].Value = ""; // DEC
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
            // RX의 값 있는 셀들만 TX의 같은 주소로 복사
            foreach (DataGridViewRow rx in gridRx.Rows)
            {
                if (rx.IsNewRow) continue;

                string reg = Convert.ToString(rx.Cells[0].Value) ?? "";
                string hex = Convert.ToString(rx.Cells[1].Value) ?? "";
                string dec = Convert.ToString(rx.Cells[2].Value) ?? "";

                // 값이 비었으면 스킵(행은 유지)
                if (string.IsNullOrWhiteSpace(hex) && string.IsNullOrWhiteSpace(dec))
                    continue;

                // 같은 주소의 TX 행 찾아 덮어쓰기
                foreach (DataGridViewRow tx in gridTx.Rows)
                {
                    if (tx.IsNewRow) continue;
                    if (Convert.ToString(tx.Cells[0].Value) == reg)
                    {
                        tx.Cells[1].Value = hex;
                        tx.Cells[2].Value = dec;
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
            using var sfd = new SaveFileDialog { Filter = "Text|*.txt", FileName = $"modbus_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt" };
            if (sfd.ShowDialog(this) == DialogResult.OK)
                System.IO.File.WriteAllText(sfd.FileName, txtLog.Text);
        }

        // Register Count / FC 변경 시 DataCount 자동 갱신
        private void numCount_ValueChanged(object sender, EventArgs e) => RefreshDataCount();
        private void cmbFunctionCode_SelectedIndexChanged(object sender, EventArgs e) => RefreshDataCount();
        private void cmbFunctionCode_TextChanged(object sender, EventArgs e) => RefreshDataCount();

        // 폴링
        private void btnPollStart_Click(object sender, EventArgs e)
        {
            if (chkSlaveMode.Checked)
            {
                MessageBox.Show("Slave 모드에서는 폴링을 사용할 수 없습니다.");
                return;
            }
            pollTimer.Interval = (int)numInterval.Value;
            pollTimer.Start();
        }
        private void btnPollStop_Click(object sender, EventArgs e)
        {
            pollTimer.Stop();
        }

        private void pollTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!_isOpen) return;

                byte slave = (byte)numSlave.Value;
                ushort start = (ushort)numStartRegister.Value;
                ushort count = (ushort)numCount.Value;
                byte fc = GetFunctionCode();
                if (!(fc == 0x03 || fc == 0x04)) return;

                var req = BuildReadFrame(slave, fc, start, count);
                var resp = SendAndReceive(req);

                Log("TX: " + ToHex(req));
                Log("RX: " + ToHex(resp));

                if (resp.Length >= 3 && resp[1] == (byte)(fc | 0x80))
                    return; // 장치 예외 시 무시

                UpdateReceiveHeader(resp, slave, fc, start, count);
                var values = ParseReadResponse(resp);
                FillRxGrid(start, values);

                RegisterCache.UpdateRange(start, values); // 캐시 갱신
            }
            catch { /* 폴링 중 예외 무시 */ }
        }

        private void FillRxGrid(ushort startAddr, ushort[] values)
        {
            if (values == null || values.Length == 0) return;

            // 0001h~11FFh 범위에서만 갱신
            for (int i = 0; i < values.Length; i++)
            {
                ushort addr = (ushort)(startAddr + i);
                if (addr < 0x0001 || addr > 0x11FF) continue;

                string key = $"{addr:X4}h";

                // 주소로 행 찾기
                foreach (DataGridViewRow r in gridRx.Rows)
                {
                    if (r.IsNewRow) continue;
                    if (Convert.ToString(r.Cells[0].Value) == key)
                    {
                        r.Cells[1].Value = $"{values[i]:X4}";
                        r.Cells[2].Value = values[i].ToString();
                        break;
                    }
                }
            }
        }

        // ───────────────────────── Helpers ─────────────────────────
        private void Log(string line)
        {
            txtLog.AppendText(line + Environment.NewLine);

            // [추가] Recording 중이면 파일에도 동시에 기록
            if (_recWriter != null)
            {
                try
                {
                    _recWriter.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {line}");
                    _recWriter.Flush();
                }
                catch
                {
                    // 파일 문제가 생겨도 앱은 계속 동작하도록 무시
                }
            }
        }

        private static string ToHex(byte[] bytes)
            => BitConverter.ToString(bytes).Replace("-", " ");

        private byte GetFunctionCode()
        {
            // "10h Write MR" → "10h" → "10" → 0x10
            string raw = (cmbFunctionCode?.Text ?? "03").Trim();
            int space = raw.IndexOf(' ');
            if (space > 0) raw = raw.Substring(0, space).Trim();
            if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) raw = raw.Substring(2);
            if (raw.EndsWith("h", StringComparison.OrdinalIgnoreCase)) raw = raw.Substring(0, raw.Length - 1);
            if (raw.Length == 1) raw = "0" + raw;

            if (byte.TryParse(raw, System.Globalization.NumberStyles.HexNumber, null, out var fcHex))
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

        // TX 그리드 입력 동기화(HEX<->DEC)
        private void GridTx_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = gridTx.Rows[e.RowIndex];
            if (row.IsNewRow) return;

            // 0:Register(읽기전용), 1:HEX, 2:DEC
            string hex = Convert.ToString(row.Cells[1].Value) ?? "";
            string dec = Convert.ToString(row.Cells[2].Value) ?? "";

            // 편집된 쪽 기준으로 다른쪽 갱신
            if (e.ColumnIndex == 1) // HEX 편집
            {
                if (TryParseUShortFromHex(hex, out ushort v))
                    row.Cells[2].Value = v.ToString();
                else
                    row.Cells[2].Value = "";
            }
            else if (e.ColumnIndex == 2) // DEC 편집
            {
                if (ushort.TryParse(dec, out ushort v))
                    row.Cells[1].Value = $"{v:X4}";
                else
                    row.Cells[1].Value = "";
            }
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
            return ushort.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out value);
        }

        private ushort[] ReadTxValues(ushort count)
        {
            return Enumerable.Range(0, count).Select(ReadTxValueOrZero).ToArray();
        }

        private ushort ReadTxValueOrZero(int rowIndex)
        {
            if (rowIndex >= gridTx.Rows.Count) return 0;
            string hex = Convert.ToString(gridTx.Rows[rowIndex].Cells[1].Value) ?? "";
            string dec = Convert.ToString(gridTx.Rows[rowIndex].Cells[2].Value) ?? "";

            if (!string.IsNullOrWhiteSpace(hex) && TryParseUShortFromHex(hex, out ushort v1))
                return v1;
            if (!string.IsNullOrWhiteSpace(dec) && ushort.TryParse(dec.Trim(), out ushort v2))
                return v2;
            return 0;
        }

        // ───────────────────────── CRC & Frames ─────────────────────────
        private static ushort Crc16(byte[] data, int len)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < len; i++)
            {
                crc ^= data[i];
                for (int b = 0; b < 8; b++)
                {
                    bool lsb = (crc & 0x0001) != 0;
                    crc >>= 1;
                    if (lsb) crc ^= 0xA001;
                }
            }
            return crc;
        }

        private static byte[] WithCrc(byte[] frame)
        {
            var crc = Crc16(frame, frame.Length);
            var arr = new byte[frame.Length + 2];
            Buffer.BlockCopy(frame, 0, arr, 0, frame.Length);
            arr[^2] = (byte)(crc & 0xFF);         // Low
            arr[^1] = (byte)((crc >> 8) & 0xFF);  // High
            return arr;
        }

        private static byte[] BuildReadFrame(byte slave, byte fc, ushort start, ushort count)
        {
            var raw = new byte[]
            {
                slave, fc,
                (byte)(start >> 8), (byte)(start & 0xFF),
                (byte)(count >> 8), (byte)(count & 0xFF)
            };
            return WithCrc(raw);
        }

        private static byte[] BuildWriteSingleFrame(byte slave, ushort addr, ushort value)
        {
            var raw = new byte[]
            {
                slave, 0x06,
                (byte)(addr >> 8), (byte)(addr & 0xFF),
                (byte)(value >> 8), (byte)(value & 0xFF)
            };
            return WithCrc(raw);
        }

        private static byte[] BuildWriteMultipleFrame(byte slave, ushort start, ushort[] values)
        {
            ushort count = (ushort)values.Length;
            byte byteCount = (byte)(count * 2);

            var raw = new byte[7 + byteCount]; // CRC 제외
            raw[0] = slave;
            raw[1] = 0x10;
            raw[2] = (byte)(start >> 8);
            raw[3] = (byte)(start & 0xFF);
            raw[4] = (byte)(count >> 8);
            raw[5] = (byte)(count & 0xFF);
            raw[6] = byteCount;

            for (int i = 0; i < values.Length; i++)
            {
                raw[7 + i * 2] = (byte)(values[i] >> 8);
                raw[8 + i * 2] = (byte)(values[i] & 0xFF);
            }

            return WithCrc(raw);
        }

        private static ushort[] ParseReadResponse(byte[] resp)
        {
            // resp: [addr][0x03/0x04][byteCount][Hi][Lo]...[crcLo][crcHi]
            if (resp.Length < 5) return Array.Empty<ushort>();
            int bc = resp[2];
            int n = bc / 2;
            var arr = new ushort[n];
            for (int i = 0; i < n; i++)
            {
                int idx = 3 + i * 2;
                if (idx + 1 >= resp.Length) break;
                arr[i] = (ushort)((resp[idx] << 8) | resp[idx + 1]);
            }
            return arr;
        }

        private void UpdateReceiveHeader(byte[] resp, byte slave, byte fc, ushort start, ushort count)
        {
            txtRxSlave.Text = slave.ToString();
            txtRxFc.Text = $"0x{fc:X2}";
            txtRxStart.Text = $"0x{start:X4}";
            txtRxCount.Text = count.ToString();

            // DataCount/CRC
            if (resp.Length >= 3)
                txtRxDataCount.Text = (fc == 0x03 || fc == 0x04) ? resp[2].ToString() : "0";
            if (resp.Length >= 2 && resp.Length >= 4)
                txtRxCrc.Text = $"{resp[^2]:X2} {resp[^1]:X2}";
        }

        private byte[] SendAndReceive(byte[] request)
        {
            _sp.DiscardInBuffer();
            _sp.Write(request, 0, request.Length);

            Thread.Sleep(30);

            var buf = new byte[512];
            int read = 0;

            try { read += _sp.Read(buf, 0, buf.Length); }
            catch (TimeoutException) { }

            Thread.Sleep(30);
            try
            {
                if (_sp.BytesToRead > 0)
                    read += _sp.Read(buf, read, Math.Min(buf.Length - read, _sp.BytesToRead));
            }
            catch (TimeoutException) { }

            var resp = new byte[read];
            Buffer.BlockCopy(buf, 0, resp, 0, read);
            return resp;
        }

        private void HexAutoFormat_OnEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var g = (DataGridView)sender;
            if (e.RowIndex < 0) return;

            // 열: 0=Register, 1=HEX, 2=DEC
            if (e.ColumnIndex == 1) // HEX 열 편집 후
            {
                var hexCell = g.Rows[e.RowIndex].Cells[1];
                var decCell = g.Rows[e.RowIndex].Cells[2];

                string raw = Convert.ToString(hexCell.Value) ?? "";
                raw = raw.Trim();

                if (raw.Length == 0) { decCell.Value = ""; return; }

                // 허용 입력: 0x 접두, h/H 접미, 공백, 콤마 등 → 제거
                raw = raw.TrimEnd('h', 'H');
                if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) raw = raw.Substring(2);

                // 숫자/영문 A-F 외 문자 제거 (ab -> ab 유지, abcd12 -> abcd12 유지)
                raw = new string(raw.Where(ch =>
                         (ch >= '0' && ch <= '9') ||
                         (ch >= 'a' && ch <= 'f') ||
                         (ch >= 'A' && ch <= 'F')).ToArray());

                if (raw.Length == 0) { hexCell.Value = ""; decCell.Value = ""; return; }

                // 16진수로 파싱 + 4자리 0패딩 + 대문자 + 'h'
                if (ushort.TryParse(raw, System.Globalization.NumberStyles.HexNumber, null, out ushort v))
                {
                    hexCell.Value = $"{v:X4}h";   // 예: 1, 01, 001, ab → 0001h, 00ABh
                    decCell.Value = v.ToString();
                }
                else
                {
                    // 파싱 불가면 빈칸 처리(원하면 입력 유지도 가능)
                    hexCell.Value = "";
                    decCell.Value = "";
                }
            }
            else if (e.ColumnIndex == 2) // DEC 열 편집 후 → HEX 동기화
            {
                var hexCell = g.Rows[e.RowIndex].Cells[1];
                var decCell = g.Rows[e.RowIndex].Cells[2];

                string raw = Convert.ToString(decCell.Value) ?? "";
                raw = raw.Trim();

                if (raw.Length == 0) { hexCell.Value = ""; return; }

                if (ushort.TryParse(raw, out ushort v))
                {
                    hexCell.Value = $"{v:X4}h";
                    decCell.Value = v.ToString(); // 정규화
                }
                else
                {
                    decCell.Value = "";
                    hexCell.Value = "";
                }
            }
        }

        // ───────────────────────── [추가] Recording 구현 ─────────────────────────

        // 체크박스 이벤트 (디자이너에서 연결 필요)
        private void chkRecording_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRecording.Checked)
            {
                int seconds = ParseRecordSeconds();
                if (seconds <= 0) seconds = 60;
                StartRecording(seconds);
            }
            else
            {
                StopRecording();
            }
        }

        // "60 sec" / "300 sec" / "120" 등에서 초 추출
        private int ParseRecordSeconds()
        {
            string raw = (cmbRecordEvery?.Text ?? "").Trim();
            if (string.IsNullOrEmpty(raw)) return 60;

            // 숫자만 뽑기
            string digits = new string(raw.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out int s)) return s;

            // 아이템이 정수라면
            if (int.TryParse(raw, out s)) return s;

            return 60;
        }

        private void StartRecording(int seconds)
        {
            try
            {
                // 이미 녹화 중이면 재시작
                StopRecording();
                string dir = "C:\\Users\\haeul\\source\\repos\\ModbusTester\\Data";
                string path = Path.Combine(dir, $"modbus_rec_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
                _recWriter = new StreamWriter(path, append: true, Encoding.UTF8);
                _recUntil = DateTime.Now.AddSeconds(seconds);

                _recTimer.Start();
                Log($"[REC] start {seconds}s → {path}");
            }
            catch (Exception ex)
            {
                _recWriter = null;
                Log("[REC] start failed: " + ex.Message);
                MessageBox.Show("Recording 시작 실패: " + ex.Message);
                if (chkRecording.Checked) chkRecording.Checked = false;
            }
        }

        private void StopRecording()
        {
            try
            {
                _recTimer.Stop();

                if (_recWriter != null)
                {
                    _recWriter.Flush();
                    _recWriter.Dispose();
                    _recWriter = null;
                    Log("[REC] stop");
                }
            }
            catch
            {
                // 무시
            }
        }

        // QuickView
        private void ToggleQuickWatch()
        {
            if (_quick == null || _quick.IsDisposed)
            {
                _quick = new FormQuickView();
                _quick.Show(this); // 소유자 지정
            }
            else
            {
                _quick.Close();
            }
        }

        private void btnQuickView_Click(object sender, EventArgs e)
        {
            ToggleQuickWatch();
        }
    }
}
