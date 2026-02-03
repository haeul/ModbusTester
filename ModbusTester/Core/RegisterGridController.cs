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
    /// - TX Name → RX Name 동기화 (이벤트 기반 + 전체 강제 동기화)
    ///
    /// [추가]
    /// - Start Register 변경 시 TX/RX Grid Register 컬럼 재정렬
    /// - Preset 저장/로드용 TxStartAddress / RxStartAddress 제공
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

        // 현재 그리드가 표시 중인 시작 주소 (Preset 저장/로드에 필요)
        public ushort TxStartAddress { get; private set; } = RegisterMin;
        public ushort RxStartAddress { get; private set; } = RegisterMin;

        // numStartRegister.Value를 내부에서 세팅할 때 ValueChanged 재진입 방지
        private bool _suppressNumStartSync = false;
        public bool IsSuppressingStartSync => _suppressNumStartSync;

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

                // 초기화 후 현재 StartRegister 기준으로 TX/RX 모두 정렬
                ushort start = (ushort)_numStartRegister.Value;
                SetStartAddressBoth(start);
            }
            finally
            {
                _gridTx.ResumeLayout();
                _gridRx.ResumeLayout();
            }
        }

        // (요청 3) Start Register → Grid 시작 레지스터 반영
        public void SetTxStartAddress(ushort start)
        {
            TxStartAddress = NormalizeStart(start);

            // numStartRegister를 내부에서 맞출 때 재진입 방지
            _suppressNumStartSync = true;
            try
            {
                ApplyStartToGrid(_gridTx, TxStartAddress);
                _numStartRegister.Value = TxStartAddress;
            }
            finally
            {
                _suppressNumStartSync = false;
            }
        }

        public void SetRxStartAddress(ushort start)
        {
            RxStartAddress = NormalizeStart(start);
            ApplyStartToGrid(_gridRx, RxStartAddress);
        }

        public void SetStartAddressBoth(ushort start)
        {
            // TX 먼저 적용(= numStartRegister 동기화 포함) 후 RX 적용
            SetTxStartAddress(start);
            SetRxStartAddress(start);
        }

        private static ushort NormalizeStart(ushort start)
        {
            // 현재 그리드는 1024행 고정이지만, 표시 주소는 0x0000~0xFFFF 모두 가능
            // 필요하면 정책에 따라 제한 가능(예: start <= 0xFFFF)
            return start;
        }

        private void ApplyStartToGrid(DataGridView grid, ushort start)
        {
            if (grid.Rows.Count == 0) return;

            // 첫 행 Register를 start로 세팅하고 나머지 재배열
            grid.Rows[0].Cells[_colReg].Value = $"{start:X4}h";

            // 기존 로직 재사용(파싱/증가/랩어라운드 포함)
            RebuildRegisterColumnFromFirstRow(grid);
        }

        // Register 컬럼: 첫 줄만 편집 허용
        public void HandleCellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.ColumnIndex == _colReg && e.RowIndex != 0)
            {
                e.Cancel = true;
            }
        }

        // HEX/DEC/BIT, Register 자동 증가 처리
        // ※ Name 동기화는 여기서 하지 않음 (이벤트 기반으로 FormMain에서 처리)
        public void HandleCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var g = (DataGridView)sender;
            if (e.RowIndex < 0) return;

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

        // TX 영역 비었는지 확인용 (Name 제외)
        public bool IsTxValueAreaEmpty()
        {
            foreach (DataGridViewRow row in _gridTx.Rows)
            {
                if (row.IsNewRow) continue;

                string hex = Convert.ToString(row.Cells[_colHex].Value) ?? "";
                string dec = Convert.ToString(row.Cells[_colDec].Value) ?? "";
                string bit = Convert.ToString(row.Cells[_colBit].Value) ?? "";

                if (!string.IsNullOrWhiteSpace(hex) ||
                    !string.IsNullOrWhiteSpace(dec) ||
                    !string.IsNullOrWhiteSpace(bit))
                {
                    return false;
                }
            }

            return true;
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

        // TX 전체 초기화 (Register 제외)
        public void ClearTxAll()
        {
            foreach (DataGridViewRow row in _gridTx.Rows)
            {
                if (row.IsNewRow) continue;

                row.Cells[_colName].Value = "";
                ClearValueCells(row, respectReadOnly: false);
            }

            // “어떤 상황에서도 동기화” 원칙이면, TX를 비웠으면 RX Name도 따라가야 함
            SyncAllTxNamesToRx();
        }

        // TX 값만 전체 초기화 (Register/Name 유지)
        public void ClearTxValues()
        {
            foreach (DataGridViewRow r in _gridTx.Rows)
            {
                if (r.IsNewRow) continue;
                ClearValueCells(r, respectReadOnly: false);
            }
        }

        // RX 값만 전체 초기화
        public void ClearRxValues()
        {
            foreach (DataGridViewRow r in _gridRx.Rows)
            {
                if (r.IsNewRow) continue;
                ClearValueCells(r, respectReadOnly: false);
            }
        }

        // RX → TX 값 복사 (값만)
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

        // ─────────────────────────
        // TX Name → RX Name 동기화
        // ─────────────────────────

        public void SyncTxNameToRxByRowIndex(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= _gridTx.Rows.Count)
                return;

            var txRow = _gridTx.Rows[rowIndex];
            if (txRow.IsNewRow) return;

            string reg = Convert.ToString(txRow.Cells[_colReg].Value) ?? "";
            string name = Convert.ToString(txRow.Cells[_colName].Value) ?? "";

            if (string.IsNullOrWhiteSpace(reg))
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
        }

        public void SyncAllTxNamesToRx()
        {
            for (int i = 0; i < _gridTx.Rows.Count; i++)
            {
                var row = _gridTx.Rows[i];
                if (row.IsNewRow) continue;

                SyncTxNameToRxByRowIndex(i);
            }
        }

        // ───────── 내부 Helper (그리드 전용) ─────────
        public void HandleGridDeleteKey(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete && e.KeyCode != Keys.Back) return;

            var g = sender as DataGridView;
            if (g == null) return;

            // 사용자가 실제로 편집 중이면(텍스트 박스 커서 들어간 상태) 기본 동작 존중
            if (g.IsCurrentCellInEditMode) return;

            var cell = g.CurrentCell;
            if (cell == null) return;

            int rowIndex = cell.RowIndex;
            int colIndex = cell.ColumnIndex;
            if (rowIndex < 0) return;

            var row = g.Rows[rowIndex];
            if (row.IsNewRow) return;

            // 1) Register 칸에서 Delete/Backspace → Register만 지움 (첫 행만)
            if (colIndex == _colReg)
            {
                if (rowIndex == 0 && !row.Cells[_colReg].ReadOnly)
                    row.Cells[_colReg].Value = "";

                e.Handled = true;
                return;
            }

            // 2) Name 칸에서 Delete → Name만 지움
            if (colIndex == _colName)
            {
                if (!row.Cells[_colName].ReadOnly)
                    row.Cells[_colName].Value = "";

                // (TX Name->RX Name 동기화 정책이면) TX에서만 동기화
                if (ReferenceEquals(g, _gridTx))
                    SyncTxNameToRxByRowIndex(rowIndex);

                e.Handled = true;
                return;
            }

            // 3) HEX/DEC/BIT 칸에서 Delete → 값 3칸만 지움 (Name은 건드리지 않음)
            if (colIndex == _colHex || colIndex == _colDec || colIndex == _colBit)
            {
                ClearValueCells(row, respectReadOnly: false);
                e.Handled = true;
                return;
            }
        }

        private void ClearValueCells(DataGridViewRow row, bool respectReadOnly)
        {
            if (row == null || row.IsNewRow) return;

            if (!respectReadOnly || !row.Cells[_colHex].ReadOnly)
                row.Cells[_colHex].Value = "";
            if (!respectReadOnly || !row.Cells[_colDec].ReadOnly)
                row.Cells[_colDec].Value = "";
            if (!respectReadOnly || !row.Cells[_colBit].ReadOnly)
                row.Cells[_colBit].Value = "";
        }

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

            // 현재 표시 시작 주소 상태 업데이트 + numStartRegister 동기화
            if (ReferenceEquals(grid, _gridTx))
            {
                TxStartAddress = numericStart;

                _suppressNumStartSync = true;
                try
                {
                    _numStartRegister.Value = numericStart;
                }
                finally
                {
                    _suppressNumStartSync = false;
                }

                // TX 시작 주소 변경 시 RX도 동기화
                SetRxStartAddress(numericStart);
            }
            else if (ReferenceEquals(grid, _gridRx))
            {
                RxStartAddress = numericStart;
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
