using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ModbusTester.Utils;
using static ModbusTester.Utils.HexExtensions;

namespace ModbusTester.Core
{
    /// <summary>
    /// FormMain에서 쓰는 TX/RX 레지스터 DataGridView 관련 로직 모음.
    /// - 그리드 초기화
    /// - 셀 편집 처리(HEX/DEC/BIT 동기화, Register 자동 증가)
    /// - RX 그리드 값 채우기
    /// - TX 스냅샷(실행취소용)
    /// - TX/RX 값 클리어, Copy 기능
    /// - TX 값 읽기(ReadTxValues / ReadTxValueOrZero)
    /// </summary>
    public class RegisterGridController
    {
        private readonly DataGridView _gridTx;
        private readonly DataGridView _gridRx;
        private readonly NumericUpDown _numStartRegister;

        private readonly int _colReg;
        private readonly int _colName;
        private readonly int _colHex;
        private readonly int _colDec;
        private readonly int _colBit;

        private class TxRowSnapshot
        {
            public string Reg = "";
            public string Name = "";
            public string Hex = "";
            public string Dec = "";
            public string Bit = "";
        }

        private List<TxRowSnapshot>? _lastTxSnapshot;

        private const ushort RegisterMin = 0x0000;
        private const ushort RegisterMax = 0xFFFF;
        private const ushort GridRegisterMax = 0x03FF;

        public RegisterGridController(
            DataGridView gridTx,
            DataGridView gridRx,
            NumericUpDown numStartRegister,
            int colReg,
            int colName,
            int colHex,
            int colDec,
            int colBit)
        {
            _gridTx = gridTx ?? throw new ArgumentNullException(nameof(gridTx));
            _gridRx = gridRx ?? throw new ArgumentNullException(nameof(gridRx));
            _numStartRegister = numStartRegister ?? throw new ArgumentNullException(nameof(numStartRegister));

            _colReg = colReg;
            _colName = colName;
            _colHex = colHex;
            _colDec = colDec;
            _colBit = colBit;
        }

        // 그리드 공통 설정 (열 폭/정렬/정렬 방지 등)
        public void SetupGrids()
        {
            SetupGrid(_gridTx);
            SetupGrid(_gridRx);
        }

        private void SetupGrid(DataGridView g)
        {
            g.AllowUserToAddRows = false;
            g.RowHeadersVisible = false;

            g.SelectionMode = DataGridViewSelectionMode.CellSelect;
            g.EditMode = DataGridViewEditMode.EditOnEnter;
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            g.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            g.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            foreach (DataGridViewColumn col in g.Columns)
                col.SortMode = DataGridViewColumnSortMode.NotSortable;

            if (g.Columns.Count > _colBit)
            {
                g.Columns[_colReg].FillWeight = 80;
                g.Columns[_colName].FillWeight = 100;
                g.Columns[_colHex].FillWeight = 80;
                g.Columns[_colDec].FillWeight = 80;
                g.Columns[_colBit].FillWeight = 180;

                g.Columns[_colReg].ReadOnly = false;
                g.Columns[_colName].ReadOnly = false;
                g.Columns[_colHex].ReadOnly = false;
                g.Columns[_colDec].ReadOnly = false;
                g.Columns[_colBit].ReadOnly = false;
            }

            var qvCol = g.Columns["colRxQuickView"];
            if (qvCol != null)
            {
                qvCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                qvCol.Width = 30;
                qvCol.ReadOnly = false;
            }
        }

        // 0000h~03FFh까지 TX/RX 두 그리드에 채우기 (기존 InitializeGridsAsync 역할)
        public async Task InitializeGridsAsync()
        {
            _gridTx.SuspendLayout();
            _gridRx.SuspendLayout();
            try
            {
                _gridTx.Rows.Clear();
                _gridRx.Rows.Clear();

                const int chunk = 512;
                int counter = 0;

                for (int addr = RegisterMin; addr <= GridRegisterMax; addr++)
                {
                    string reg = $"{addr:X4}h";
                    _gridTx.Rows.Add(reg, "", "", "", "");
                    _gridRx.Rows.Add(reg, "", "", "", "");

                    counter++;
                    if (counter % chunk == 0)
                        await Task.Yield();
                }
            }
            finally
            {
                _gridTx.ResumeLayout();
                _gridRx.ResumeLayout();
            }
        }

        // Register 컬럼: 첫 줄만 편집 허용
        public void HandleCellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex == _colReg && e.RowIndex != 0)
            {
                e.Cancel = true;
            }
        }

        // HEX/DEC/BIT, Name, Register 자동 증가 처리
        public void HandleCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var g = (DataGridView)sender;
            if (e.RowIndex < 0) return;

            // TX Name 수정 시 RX Name 동기화
            if (e.ColumnIndex == _colName && ReferenceEquals(g, _gridTx))
            {
                var row = g.Rows[e.RowIndex];
                if (row.IsNewRow) return;

                string reg = Convert.ToString(row.Cells[_colReg].Value) ?? "";
                string name = Convert.ToString(row.Cells[_colName].Value) ?? "";

                if (string.IsNullOrEmpty(reg))
                    return;

                foreach (DataGridViewRow rxRow in _gridRx.Rows)
                {
                    if (rxRow.IsNewRow) continue;
                    if (Convert.ToString(rxRow.Cells[_colReg].Value) == reg)
                    {
                        rxRow.Cells[_colName].Value = name;
                        break;
                    }
                }

                return;
            }

            // 첫 줄 Register 수정 시 전체 Register 재배열
            if (e.ColumnIndex == _colReg && e.RowIndex == 0)
            {
                RebuildRegisterColumnFromFirstRow(g);
                return;
            }

            // HEX 편집
            if (e.ColumnIndex == _colHex)
            {
                var hexCell = g.Rows[e.RowIndex].Cells[_colHex];
                var decCell = g.Rows[e.RowIndex].Cells[_colDec];

                string raw = Convert.ToString(hexCell.Value) ?? "";
                raw = raw.Trim();

                if (raw.Length == 0)
                {
                    decCell.Value = "";
                    UpdateBitCell(g.Rows[e.RowIndex]);
                    return;
                }

                // "0x", "h" 제거 + HEX 문자만 남기기
                raw = raw.TrimEnd('h', 'H');
                if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    raw = raw.Substring(2);

                raw = new string(raw.Where(ch =>
                    ch >= '0' && ch <= '9' ||
                    ch >= 'a' && ch <= 'f' ||
                    ch >= 'A' && ch <= 'F').ToArray());

                if (raw.Length == 0)
                {
                    hexCell.Value = "";
                    decCell.Value = "";
                    UpdateBitCell(g.Rows[e.RowIndex]);
                    return;
                }

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
            // DEC 편집
            else if (e.ColumnIndex == _colDec)
            {
                var hexCell = g.Rows[e.RowIndex].Cells[_colHex];
                var decCell = g.Rows[e.RowIndex].Cells[_colDec];

                string raw = Convert.ToString(decCell.Value) ?? "";
                raw = raw.Trim();

                if (raw.Length == 0)
                {
                    hexCell.Value = "";
                    UpdateBitCell(g.Rows[e.RowIndex]);
                    return;
                }

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
            // BIT 편집 (TX 그리드만 허용)
            else if (e.ColumnIndex == _colBit)
            {
                if (!ReferenceEquals(g, _gridTx))
                    return;

                var bitCell = g.Rows[e.RowIndex].Cells[_colBit];
                var hexCell = g.Rows[e.RowIndex].Cells[_colHex];
                var decCell = g.Rows[e.RowIndex].Cells[_colDec];

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
                    bitCell.Value = v.ToBitString16();
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

        // RX 그리드에 값 채워넣기
        public void FillRxGrid(ushort startAddr, ushort[] values)
        {
            if (values == null || values.Length == 0) return;

            for (int i = 0; i < values.Length; i++)
            {
                ushort addr = (ushort)(startAddr + i);
                if (addr < RegisterMin || addr > RegisterMax) continue;

                string key = $"{addr:X4}h";

                foreach (DataGridViewRow r in _gridRx.Rows)
                {
                    if (r.IsNewRow) continue;
                    if (Convert.ToString(r.Cells[_colReg].Value) == key)
                    {
                        r.Cells[_colHex].Value = $"{values[i]:X4}h";
                        r.Cells[_colDec].Value = values[i].ToString();
                        UpdateBitCell(r);
                        break;
                    }
                }
            }
        }

        // TX 스냅샷 저장/복원
        public void SaveTxSnapshot()
        {
            var list = new List<TxRowSnapshot>();

            foreach (DataGridViewRow r in _gridTx.Rows)
            {
                if (r.IsNewRow) continue;

                list.Add(new TxRowSnapshot
                {
                    Reg = Convert.ToString(r.Cells[_colReg].Value) ?? "",
                    Name = Convert.ToString(r.Cells[_colName].Value) ?? "",
                    Hex = Convert.ToString(r.Cells[_colHex].Value) ?? "",
                    Dec = Convert.ToString(r.Cells[_colDec].Value) ?? "",
                    Bit = Convert.ToString(r.Cells[_colBit].Value) ?? "",
                });
            }

            _lastTxSnapshot = list;
        }

        public void RevertTxSnapshot()
        {
            if (_lastTxSnapshot == null) return;
            if (_gridTx.Rows.Count == 0) return;

            int idx = 0;

            foreach (DataGridViewRow r in _gridTx.Rows)
            {
                if (r.IsNewRow) continue;
                if (idx >= _lastTxSnapshot.Count) break;

                var snap = _lastTxSnapshot[idx++];

                r.Cells[_colReg].Value = snap.Reg;
                r.Cells[_colName].Value = snap.Name;
                r.Cells[_colHex].Value = snap.Hex;
                r.Cells[_colDec].Value = snap.Dec;
                r.Cells[_colBit].Value = snap.Bit;
            }
        }

        // TX 값만 전체 초기화 (Register/Name 유지)
        public void ClearTxValues()
        {
            foreach (DataGridViewRow r in _gridTx.Rows)
            {
                if (r.IsNewRow) continue;
                r.Cells[_colHex].Value = "";
                r.Cells[_colDec].Value = "";
                r.Cells[_colBit].Value = "";
            }
        }

        // RX 값만 전체 초기화
        public void ClearRxValues()
        {
            foreach (DataGridViewRow r in _gridRx.Rows)
            {
                if (r.IsNewRow) continue;
                r.Cells[_colHex].Value = "";
                r.Cells[_colDec].Value = "";
                r.Cells[_colBit].Value = "";
            }
        }

        // RX → TX 값 복사
        public void CopyRxToTx()
        {
            foreach (DataGridViewRow rx in _gridRx.Rows)
            {
                if (rx.IsNewRow) continue;

                string reg = Convert.ToString(rx.Cells[_colReg].Value) ?? "";
                string hex = Convert.ToString(rx.Cells[_colHex].Value) ?? "";
                string dec = Convert.ToString(rx.Cells[_colDec].Value) ?? "";

                if (string.IsNullOrWhiteSpace(hex) && string.IsNullOrWhiteSpace(dec))
                    continue;

                foreach (DataGridViewRow tx in _gridTx.Rows)
                {
                    if (tx.IsNewRow) continue;
                    if (Convert.ToString(tx.Cells[_colReg].Value) == reg)
                    {
                        tx.Cells[_colHex].Value = hex;
                        tx.Cells[_colDec].Value = dec;
                        UpdateBitCell(tx);
                        break;
                    }
                }
            }
        }

        // TX 값 읽기 (CRC 계산/전송용)
        public ushort[] ReadTxValues(ushort count)
        {
            return Enumerable.Range(0, count).Select(ReadTxValueOrZero).ToArray();
        }

        public ushort ReadTxValueOrZero(int rowIndex)
        {
            if (rowIndex >= _gridTx.Rows.Count) return 0;
            string hex = Convert.ToString(_gridTx.Rows[rowIndex].Cells[_colHex].Value) ?? "";
            string dec = Convert.ToString(_gridTx.Rows[rowIndex].Cells[_colDec].Value) ?? "";

            if (!string.IsNullOrWhiteSpace(hex) && TryParseUShortFromHex(hex, out ushort v1))
                return v1;
            if (!string.IsNullOrWhiteSpace(dec) && ushort.TryParse(dec.Trim(), out ushort v2))
                return v2;
            return 0;
        }

        // ───────── 내부 Helper (그리드 전용) ─────────

        private void UpdateBitCell(DataGridViewRow row)
        {
            if (row == null || row.IsNewRow) return;

            string hex = Convert.ToString(row.Cells[_colHex].Value) ?? "";
            string dec = Convert.ToString(row.Cells[_colDec].Value) ?? "";

            ushort v;
            if (!string.IsNullOrWhiteSpace(hex) && TryParseUShortFromHex(hex, out ushort hv))
                v = hv;
            else if (!string.IsNullOrWhiteSpace(dec) && ushort.TryParse(dec.Trim(), out ushort dv))
                v = dv;
            else
            {
                row.Cells[_colBit].Value = "";
                return;
            }

            row.Cells[_colBit].Value = v.ToBitString16();
        }

        private void RebuildRegisterColumnFromFirstRow(DataGridView grid)
        {
            if (grid.Rows.Count == 0) return;

            string raw = Convert.ToString(grid.Rows[0].Cells[_colReg].Value) ?? "";
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
            if (!int.TryParse(normalized, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out startInt))
            {
                startInt = 0;
            }

            if (startInt < RegisterMin) startInt = RegisterMin;
            if (startInt > RegisterMax) startInt = RegisterMax;

            ushort numericStart = (ushort)startInt;

            grid.Rows[0].Cells[_colReg].Value = $"{numericStart:X4}h";

            ushort addr = numericStart;
            bool wrapped = false;

            for (int r = 1; r < grid.Rows.Count; r++)
            {
                var row = grid.Rows[r];
                if (row.IsNewRow) continue;

                addr = NextAddress(addr, ref wrapped);
                row.Cells[_colReg].Value = $"{addr:X4}h";
            }

            if (ReferenceEquals(grid, _gridTx))
            {
                _numStartRegister.Value = numericStart;
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
    }
}
