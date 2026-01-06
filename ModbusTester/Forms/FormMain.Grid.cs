using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormMain
    {
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

        private void Grid_CellDoubleClick_OpenZoom(object? sender, DataGridViewCellEventArgs e)
        {
            if (sender is not DataGridView g) return;

            if (_rxZoom != null && !_rxZoom.IsDisposed)
            {
                _rxZoom.BringToFront();
                _rxZoom.Activate();
                return;
            }

            _rxZoom = new FormGridZoom(g, this, placeOnRight: true, hideQvColumn: false);
            _rxZoom.FormClosed += (_, __) => _rxZoom = null;
            _rxZoom.Show(this);
        }
    }
}
