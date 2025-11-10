using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormMain : Form
    {
        // ─────────────────────────────────────────────────────────────────────
        // Fields
        // ─────────────────────────────────────────────────────────────────────
        private readonly SerialPort _sp = new SerialPort();
        private bool _isOpen => _sp.IsOpen;

        // ─────────────────────────────────────────────────────────────────────
        // Ctor / Load
        // ─────────────────────────────────────────────────────────────────────
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object? sender, EventArgs e)
        {
            // ----- COM 기본 -----
            if (cmbPort.Items.Count == 0) cmbPort.Items.AddRange(SerialPort.GetPortNames());
            if (cmbPort.Items.Count > 0) cmbPort.SelectedIndex = 0;

            cmbBaud.Items.Clear();
            cmbBaud.Items.AddRange(new object[] { 1200, 2400, 4800, 9600, 19200, 38400 });
            cmbBaud.SelectedItem = 9600;

            cmbParity.DataSource = Enum.GetValues(typeof(Parity));
            cmbParity.SelectedItem = Parity.None;

            cmbDataBits.Items.Clear();
            cmbDataBits.Items.AddRange(new object[] { 5, 6, 7, 8 });
            cmbDataBits.SelectedItem = 8;

            cmbStopBits.DataSource = Enum.GetValues(typeof(StopBits));
            cmbStopBits.SelectedItem = StopBits.One;

            // ----- 송신 기본 -----
            numSlave.Value = 1;
            if (cmbFunctionCode.Items.Count == 0)
                cmbFunctionCode.Items.AddRange(new object[] { "03h Read HR", "04h Read IR", "06h Write SR", "10h Write MR" });
            cmbFunctionCode.SelectedItem = "10h Write MR";
            numStartRegister.Hexadecimal = true;       // ← 16진수 표시
            numStartRegister.Minimum = 0;
            numStartRegister.Maximum = 0xFFFF;         // 레지스터 전체 범위
            numStartRegister.Value = 0x0001;         // 기본값 0001h
            numStartRegister.Increment = 1;
            numCount.Value = 1;                 // Register Count 1
            txtDataCount.Text = "2";            // 10h 기본(1레지스터=2바이트)
            txtCrc.Clear();

            // ----- 그리드 공통 스타일 + 초기행 -----
            SetupGrid(gridTx);
            SetupGrid(gridRx);
            InitializeTxRows();                 // 0000h ~ 000Fh
            InitializeRxRows();                 // 1000h ~ 100Fh 

            // Register Count / Function Code 바뀔 때 DataCount 자동 계산
            numCount.ValueChanged += (_, __) => RefreshDataCount();
            cmbFunctionCode.SelectedIndexChanged += (_, __) => RefreshDataCount();
            cmbFunctionCode.TextChanged += (_, __) => RefreshDataCount(); // 수동 입력 대비

            // 최초 1회 계산
            RefreshDataCount();

            gridTx.CellEndEdit += GridTx_CellEndEdit;
        }

        // ─────────────────────────────────────────────────────────────────────
        // UI Helpers
        // ─────────────────────────────────────────────────────────────────────
        private void SetupGrid(DataGridView g)
        {
            g.AllowUserToAddRows = false;
            g.RowHeadersVisible = false;
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            if (g.Columns.Count >= 3)
            {
                g.Columns[0].FillWeight = 110; // Register
                g.Columns[1].FillWeight = 90;  // HEX
                g.Columns[2].FillWeight = 90;  // DEC
            }
        }

        private void InitializeTxRows()
        {
            gridTx.Rows.Clear();
            for (int i = 0x1000; i <= 0x11FF; i++)
                gridTx.Rows.Add($"{i:X4}h", "", "");
        }

        private void InitializeRxRows()
        {
            gridRx.Rows.Clear();
            for (int i = 0x1000; i <= 0x11FF; i++)
                gridRx.Rows.Add($"{i:X4}h", "", "");  // 항상 행 보이게
        }

        // ─────────────────────────────────────────────────────────────────────
        // Safe getters (현재 UI 타입: NumericUpDown / ComboBox)
        // ─────────────────────────────────────────────────────────────────────
        private byte GetSlave() => (byte)numSlave.Value;
        private ushort GetStart() => (ushort)numStartRegister.Value;
        private ushort GetCount() => (ushort)numCount.Value;

        private byte GetFunctionCode()
        {
            // "10h Write MR" -> "10h" -> 0x10
            string raw = (cmbFunctionCode.Text ?? "03h").Trim();
            string token = raw.Split(' ')[0];
            if (token.EndsWith("h", StringComparison.OrdinalIgnoreCase))
                return Convert.ToByte(token[..^1], 16);
            return Convert.ToByte(token, 16);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Buttons
        // ─────────────────────────────────────────────────────────────────────
        private void btnOpen_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_isOpen) _sp.Close();

                _sp.PortName = cmbPort.Text;
                _sp.BaudRate = int.TryParse(cmbBaud.Text, out var b) ? b : 38400;
                _sp.Parity = (Parity)(cmbParity.SelectedItem ?? Parity.None);
                _sp.DataBits = Convert.ToInt32(cmbDataBits.SelectedItem ?? 8);
                _sp.StopBits = (StopBits)(cmbStopBits.SelectedItem ?? StopBits.One);
                _sp.ReadTimeout = 500;
                _sp.WriteTimeout = 500;

                _sp.Open();
                Log("PORT OPEN");
            }
            catch (Exception ex)
            {
                MessageBox.Show("포트 열기 실패: " + ex.Message);
            }
        }

        private void btnClose_Click(object? sender, EventArgs e)
        {
            try
            {
                pollTimer.Stop();
                if (_isOpen) _sp.Close();
                Log("PORT CLOSE");
            }
            catch (Exception ex)
            {
                MessageBox.Show("포트 닫기 실패: " + ex.Message);
            }
        }

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
                    // Read Frame (CRC 제외 상태로 만듦)
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

                // 결과 표시
                txtCrc.Text = $"{crcLo:X2} {crcHi:X2}";

                // 로그에도 표시
                Log($"[CRC CALC] {ToHex(frameBody)} {crcLo:X2} {crcHi:X2}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("CRC 계산 실패: " + ex.Message);
            }
        }

        private void btnSend_Click(object? sender, EventArgs e)
        {
            if (!_isOpen) { MessageBox.Show("먼저 포트를 OPEN 하세요."); return; }

            try
            {
                byte slave = GetSlave();
                ushort start = GetStart();
                ushort count = GetCount();
                byte fc = GetFunctionCode();

                if (fc == 0x03 || fc == 0x04)
                {
                    var req = BuildReadFrame(slave, fc, start, count);
                    Log("TX: " + ToHex(req));
                    var resp = SendAndReceive(req);
                    Log("RX: " + ToHex(resp));

                    if (resp.Length >= 3 && resp[1] == (byte)(fc | 0x80))
                        throw new Exception($"장치 오류(예외코드: {resp[2]})");

                    UpdateReceiveHeader(resp, slave, fc, start, count);
                    var values = ParseReadResponse(resp);
                    FillRxGrid(start, values);
                }
                else if (fc == 0x06)
                {
                    ushort val = ReadTxValueOrZero(0);
                    var req = BuildWriteSingleFrame(slave, start, val);
                    Log("TX: " + ToHex(req));
                    var resp = SendAndReceive(req);
                    Log("RX: " + ToHex(resp));
                    UpdateReceiveHeader(resp, slave, fc, start, 1);
                }
                else if (fc == 0x10)
                {
                    var vals = ReadTxValues(count);
                    var req = BuildWriteMultipleFrame(slave, start, vals);
                    Log("TX: " + ToHex(req));
                    var resp = SendAndReceive(req);
                    Log("RX: " + ToHex(resp));
                    UpdateReceiveHeader(resp, slave, fc, start, (ushort)vals.Length);
                }
                else throw new NotSupportedException("지원하지 않는 Function Code");
            }
            catch (Exception ex)
            {
                MessageBox.Show("전송 실패: " + ex.Message);
            }
        }

        private void btnRxClear_Click(object? sender, EventArgs e)
        {
            // 행 유지, 값만 초기화(항상 보이게)
            foreach (DataGridViewRow r in gridRx.Rows)
            {
                if (r.IsNewRow) continue;
                r.Cells[1].Value = "";
                r.Cells[2].Value = "";
            }

            txtRxSlave.Clear();
            txtRxFc.Clear();
            txtRxStart.Clear();
            txtRxCount.Clear();
            txtRxDataCount.Clear();
            txtRxCrc.Clear();
        }

        private void btnCopyToTx_Click(object? sender, EventArgs e)
        {
            gridTx.Rows.Clear();
            foreach (DataGridViewRow r in gridRx.Rows)
            {
                if (r.IsNewRow) continue;
                string reg = Convert.ToString(r.Cells[0].Value) ?? "0000h";
                string hex = Convert.ToString(r.Cells[1].Value) ?? "";
                string dec = Convert.ToString(r.Cells[2].Value) ?? "";
                gridTx.Rows.Add(reg, hex, dec);
            }
        }

        private void btnPollStart_Click(object? sender, EventArgs e)
        {
            pollTimer.Interval = (int)numInterval.Value;
            pollTimer.Start();
        }

        private void btnPollStop_Click(object? sender, EventArgs e)
        {
            pollTimer.Stop();
        }

        private void pollTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (!_isOpen) return;

                byte slave = GetSlave();
                ushort start = GetStart();
                ushort count = GetCount();
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
            }
            catch { /* 폴링 중 예외 무시 */ }
        }

        private void btnLogClear_Click(object? sender, EventArgs e) => txtLog.Clear();

        private void btnSaveLog_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "Text|*.txt",
                FileName = $"modbus_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };
            if (sfd.ShowDialog(this) == DialogResult.OK)
                System.IO.File.WriteAllText(sfd.FileName, txtLog.Text);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Logging / Parse helpers
        // ─────────────────────────────────────────────────────────────────────
        private void Log(string line) => txtLog.AppendText(line + Environment.NewLine);

        private static string ToHex(byte[] bytes) => BitConverter.ToString(bytes).Replace("-", " ");

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

            // 데이터 카운트(READ) or 0
            if (resp.Length >= 3)
                txtRxDataCount.Text = (fc == 0x03 || fc == 0x04) ? resp[2].ToString() : "0";

            // CRC(lo hi)
            if (resp.Length >= 4)
                txtRxCrc.Text = $"{resp[^2]:X2} {resp[^1]:X2}";
        }

        private void FillRxGrid(ushort startAddr, ushort[] values)
        {
            // 기존 행 유지, 주소 매칭하여 값만 갱신. 없으면 추가.
            for (int i = 0; i < values.Length; i++)
            {
                ushort addr = (ushort)(startAddr + i);
                string key = $"{addr:X4}h";

                var row = gridRx.Rows
                    .Cast<DataGridViewRow>()
                    .FirstOrDefault(r => !r.IsNewRow &&
                               string.Equals(Convert.ToString(r.Cells[0].Value), key, StringComparison.OrdinalIgnoreCase));

                if (row == null)
                {
                    int idx = gridRx.Rows.Add(key, "", "");
                    row = gridRx.Rows[idx];
                }

                row.Cells[1].Value = $"{values[i]:X4}";
                row.Cells[2].Value = values[i].ToString();
            }
        }

        private ushort[] ReadTxValues(ushort count)
        {
            var list = new List<ushort>(count);
            for (int i = 0; i < count; i++) list.Add(ReadTxValueOrZero(i));
            return list.ToArray();
        }

        private ushort ReadTxValueOrZero(int rowIndex)
        {
            if (rowIndex >= gridTx.Rows.Count) return 0;

            string hex = Convert.ToString(gridTx.Rows[rowIndex].Cells[1].Value) ?? "";
            string dec = Convert.ToString(gridTx.Rows[rowIndex].Cells[2].Value) ?? "";

            if (!string.IsNullOrWhiteSpace(hex))
            {
                var s = hex.Trim();
                if (s.EndsWith("h", StringComparison.OrdinalIgnoreCase))
                    return Convert.ToUInt16(s[..^1], 16);
                if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    return Convert.ToUInt16(s[2..], 16);
                return Convert.ToUInt16(s, 16); // 숫자만 → 16진수로 처리
            }
            if (!string.IsNullOrWhiteSpace(dec))
                return Convert.ToUInt16(dec.Trim());
            return 0;
        }
        private void GridTx_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = gridTx.Rows[e.RowIndex];

            // 컬럼 인덱스
            int colHex = 1;
            int colDec = 2;

            try
            {
                if (e.ColumnIndex == colHex)
                {
                    // HEX 입력 → DEC 변환
                    string hex = Convert.ToString(row.Cells[colHex].Value)?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(hex))
                    {
                        row.Cells[colDec].Value = "";
                        return;
                    }

                    // "h"나 "0x" 접두/접미사 제거
                    hex = hex.Replace("0x", "", StringComparison.OrdinalIgnoreCase)
                             .Replace("h", "", StringComparison.OrdinalIgnoreCase);

                    if (ushort.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out ushort value))
                        row.Cells[colDec].Value = value.ToString();
                    else
                        row.Cells[colDec].Value = ""; // 잘못된 입력일 경우 비움
                }
                else if (e.ColumnIndex == colDec)
                {
                    // DEC 입력 → HEX 변환
                    string dec = Convert.ToString(row.Cells[colDec].Value)?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(dec))
                    {
                        row.Cells[colHex].Value = "";
                        return;
                    }

                    if (ushort.TryParse(dec, out ushort value))
                        row.Cells[colHex].Value = $"{value:X4}";
                    else
                        row.Cells[colHex].Value = "";
                }
            }
            catch
            {
                // 예외 발생 시 해당 행의 변환만 무시
            }
        }


        // ─────────────────────────────────────────────────────────────────────
        // CRC / Frame builders / IO
        // ─────────────────────────────────────────────────────────────────────
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
            // [addr][fc][startHi][startLo][cntHi][cntLo] + CRC(lo hi)
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
            // [addr][0x06][regHi][regLo][valHi][valLo] + CRC(lo hi)
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
            // [addr][0x10][startHi][startLo][cntHi][cntLo][byteCount][data...] + CRC(lo hi)
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

        private byte[] SendAndReceive(byte[] request)
        {
            _sp.DiscardInBuffer();
            _sp.Write(request, 0, request.Length);

            // 간단한 수신 루프 (장치에 따라 Sleep 조정)
            Thread.Sleep(50);

            var buf = new byte[512];
            int read = 0;

            try { read += _sp.Read(buf, 0, buf.Length); }
            catch (TimeoutException) { }

            Thread.Sleep(40);
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

        private void RefreshDataCount()
        {
            byte fc = GetFunctionCode();
            ushort count = GetCount();

            if (fc == 0x10)            // 다중 레지스터 쓰기: 바이트카운트 = 레지스터개수 * 2
                txtDataCount.Text = (count * 2).ToString();
            else if (fc == 0x06)       // 단일 쓰기: 항상 2바이트
                txtDataCount.Text = "2";
            else                       // 읽기(03/04): 요청 PDU엔 바이트카운트 필드가 없음
                txtDataCount.Text = "0";
        }

    }
}
