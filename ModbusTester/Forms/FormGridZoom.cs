using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormGridZoom : Form
    {
        private readonly DataGridView _src;
        private readonly bool _hideQvColumn;
        private readonly System.Windows.Forms.Timer _syncTimer = new System.Windows.Forms.Timer();

        // 원본 컬럼 인덱스 -> 확대창 컬럼 인덱스 매핑
        // (QV를 빼면 인덱스가 어긋나기 때문에 필요)
        private int[] _srcToDstColMap = Array.Empty<int>();

        public FormGridZoom(DataGridView source, Form owner, bool placeOnRight, bool hideQvColumn)
        {
            InitializeComponent();

            _src = source ?? throw new ArgumentNullException(nameof(source));
            _hideQvColumn = hideQvColumn;

            Text = $"Zoom - {source.Name}";
            StartPosition = FormStartPosition.Manual;

            // 전체화면 X, 원하는 크기
            Size = new Size(520, 900);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;

            // ESC 닫기
            KeyPreview = true;
            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape) Close();
            };

            // 메인 폼 기준 좌/우 배치
            PlaceNextToOwner(owner, placeOnRight);

            // 더블버퍼(버벅임/깜빡임 체감 개선)
            EnableDoubleBuffer(gridZoom);

            // 그리드 구조(컬럼 등) 1회 구성
            BuildColumnsFrom(_src);

            // 최초 1회 동기화
            SyncAllRows();

            // 주기적 동기화 (실시간 반영)
            _syncTimer.Interval = 100; // 100ms (원하면 200으로)
            _syncTimer.Tick += (_, __) => SyncAllRows();
            _syncTimer.Start();

            // 닫힐 때 정리
            FormClosed += (_, __) =>
            {
                try
                {
                    _syncTimer.Stop();
                    _syncTimer.Dispose();
                }
                catch { }
            };
        }

        private void PlaceNextToOwner(Form owner, bool right)
        {
            if (owner == null)
            {
                // owner가 없으면 그냥 화면 중앙 근처
                StartPosition = FormStartPosition.CenterScreen;
                return;
            }

            int gap = 10;
            var ob = owner.Bounds;

            int x = right ? (ob.Right + gap) : (ob.Left - Width - gap);
            int y = ob.Top;

            // 작업표시줄 제외 영역으로 보정
            Rectangle wa = Screen.FromControl(owner).WorkingArea;

            if (x < wa.Left) x = wa.Left;
            if (x + Width > wa.Right) x = wa.Right - Width;
            if (y < wa.Top) y = wa.Top;
            if (y + Height > wa.Bottom) y = wa.Bottom - Height;

            Location = new Point(x, y);
        }

        private static void EnableDoubleBuffer(DataGridView dgv)
        {
            if (dgv == null) return;

            typeof(DataGridView).InvokeMember(
                "DoubleBuffered",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty,
                null,
                dgv,
                new object[] { true }
            );
        }

        private bool IsQvColumn(DataGridViewColumn col)
        {
            // 프로젝트마다 이름이 다를 수 있어 HeaderText/Name 둘 다 체크
            if (col == null) return false;
            if (string.Equals(col.HeaderText, "QV", StringComparison.OrdinalIgnoreCase)) return true;
            if (col.Name != null && col.Name.IndexOf("QV", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (col.Name != null && col.Name.IndexOf("QuickView", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private void BuildColumnsFrom(DataGridView src)
        {
            // 기본 설정
            gridZoom.Dock = DockStyle.Fill;
            gridZoom.AllowUserToAddRows = false;
            gridZoom.AllowUserToDeleteRows = false;
            gridZoom.RowHeadersVisible = false;
            gridZoom.SelectionMode = src.SelectionMode;
            gridZoom.ReadOnly = src.ReadOnly;

            // 헤더 스타일 테마 덮어쓰기 방지 + 가운데 정렬
            gridZoom.EnableHeadersVisualStyles = false;
            gridZoom.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridZoom.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Fill로 꽉 채우기
            gridZoom.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // 높이/폰트는 원본 느낌 최대한 유지
            gridZoom.Font = new Font(src.Font.FontFamily, src.Font.Size, src.Font.Style);
            gridZoom.RowTemplate.Height = src.RowTemplate.Height;
            gridZoom.ColumnHeadersHeight = src.ColumnHeadersHeight;
            gridZoom.ColumnHeadersHeightSizeMode = src.ColumnHeadersHeightSizeMode;

            // 컬럼 구성
            gridZoom.Columns.Clear();

            // 매핑 만들기 위해 먼저 src 기준으로 dst 인덱스 계산
            _srcToDstColMap = new int[src.Columns.Count];
            for (int i = 0; i < _srcToDstColMap.Length; i++)
                _srcToDstColMap[i] = -1;

            int dstIndex = 0;

            foreach (DataGridViewColumn col in src.Columns)
            {
                if (_hideQvColumn && IsQvColumn(col))
                {
                    _srcToDstColMap[col.Index] = -1;
                    continue;
                }

                var newCol = (DataGridViewColumn)Activator.CreateInstance(col.GetType())!;
                newCol.HeaderText = col.HeaderText;
                newCol.Name = col.Name;
                newCol.ReadOnly = col.ReadOnly;

                // Fill 비율(원본 폭을 가중치로 사용)
                newCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                newCol.FillWeight = Math.Max(10, col.Width);

                // 컬럼별 가운데 정렬
                newCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                newCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                gridZoom.Columns.Add(newCol);

                _srcToDstColMap[col.Index] = dstIndex;
                dstIndex++;
            }
        }

        private void SyncAllRows()
        {
            // 원본이 없어졌으면 중지
            if (_src == null || _src.IsDisposed)
            {
                try { Close(); } catch { }
                return;
            }

            // 원본이 다른 스레드에서 바뀌는 경우(드물지만) UI 스레드 보정
            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(SyncAllRows)); } catch { }
                return;
            }

            gridZoom.SuspendLayout();

            try
            {
                // 1) 원본 유효 행 수 계산
                int srcRowCount = 0;
                foreach (DataGridViewRow r in _src.Rows)
                {
                    if (!r.IsNewRow) srcRowCount++;
                }

                // 2) 행 개수 맞추기
                while (gridZoom.Rows.Count < srcRowCount)
                    gridZoom.Rows.Add();

                while (gridZoom.Rows.Count > srcRowCount)
                    gridZoom.Rows.RemoveAt(gridZoom.Rows.Count - 1);

                // 3) 값 복사(변경된 것만)
                int dstRow = 0;

                foreach (DataGridViewRow row in _src.Rows)
                {
                    if (row.IsNewRow) continue;

                    // 행 높이도 원본 느낌 유지
                    if (gridZoom.Rows[dstRow].Height != row.Height)
                        gridZoom.Rows[dstRow].Height = row.Height;

                    for (int srcCol = 0; srcCol < row.Cells.Count; srcCol++)
                    {
                        int dstCol = _srcToDstColMap[srcCol];
                        if (dstCol < 0) continue; // QV 제거 등으로 매핑이 없으면 skip

                        object? newVal = row.Cells[srcCol].Value;
                        object? oldVal = gridZoom.Rows[dstRow].Cells[dstCol].Value;

                        if (!Equals(oldVal, newVal))
                            gridZoom.Rows[dstRow].Cells[dstCol].Value = newVal;
                    }

                    dstRow++;
                }
            }
            finally
            {
                gridZoom.ResumeLayout();
            }
        }
    }
}
