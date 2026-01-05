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

        private int[] _srcToDstColMap = Array.Empty<int>();

        // ====== 폰트 기반 스케일 기준값 ======
        private float _baseFontSize;
        private int _baseHeaderHeight;
        private int _baseRowHeight;
        private Size _baseClientSize;
        private bool _baseCaptured;

        // ====== UI(코드로 추가) ======
        private readonly Panel _topPanel = new Panel();
        private readonly ComboBox _cmbFontSize = new ComboBox();
        // ==========================================

        public FormGridZoom(DataGridView source, Form owner, bool placeOnRight, bool hideQvColumn)
        {
            InitializeComponent();

            _src = source ?? throw new ArgumentNullException(nameof(source));
            _hideQvColumn = hideQvColumn;

            Text = $"Zoom - {source.Name}";
            StartPosition = FormStartPosition.Manual;

            Size = new Size(520, 900);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;

            KeyPreview = true;
            KeyDown += (_, e) =>
            {
                if (e.KeyCode == Keys.Escape) Close();
            };

            PlaceNextToOwner(owner, placeOnRight);

            // 더블버퍼
            EnableDoubleBuffer(gridZoom);

            // ====== [추가] 상단 콤보박스 UI 붙이기(Designer 수정 없음) ======
            BuildTopUi();
            // ===============================================================

            // 그리드 구조(컬럼 등) 1회 구성
            BuildColumnsFrom(_src);

            // 최초 1회 동기화
            SyncAllRows();

            // 주기적 동기화
            _syncTimer.Interval = 100;
            _syncTimer.Tick += (_, __) => SyncAllRows();
            _syncTimer.Start();

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

        private void BuildTopUi()
        {
            _topPanel.Dock = DockStyle.Top;
            _topPanel.Height = 36;                 // 살짝 낮게
            _topPanel.Padding = new Padding(0);    // 패널 자체는 여백 0
            _topPanel.Margin = new Padding(0);

            // 오른쪽 고정 컨테이너
            var rightBox = new FlowLayoutPanel();
            rightBox.Dock = DockStyle.Right;
            rightBox.AutoSize = true;
            rightBox.WrapContents = false;
            rightBox.FlowDirection = FlowDirection.LeftToRight;

            // 여기서만 여백을 준다 (그리드랑 시각적으로 맞춤)
            rightBox.Padding = new Padding(0, 6, 8, 0);  // top=6, right=8
            rightBox.Margin = new Padding(0);

            var uiFont = new Font(Font.FontFamily, 11f, FontStyle.Regular);

            var lbl = new Label();
            lbl.Text = "Font Size";
            lbl.AutoSize = true;
            lbl.Font = uiFont;
            lbl.Margin = new Padding(0, 2, 8, 0);

            _cmbFontSize.DropDownStyle = ComboBoxStyle.DropDownList;
            _cmbFontSize.Width = 140;
            _cmbFontSize.Font = uiFont;
            _cmbFontSize.Margin = new Padding(0, 0, 0, 0);

            // 목록 채우기
            _cmbFontSize.Items.Clear();
            _cmbFontSize.Items.AddRange(new object[]
            {
        "10", "11", "12", "13", "14", "15", "16", "18", "20", "22", "24"
            });

            var initial = (int)Math.Round(_src.Font.Size);
            int idx = _cmbFontSize.Items.IndexOf(initial.ToString());
            if (idx < 0) idx = _cmbFontSize.Items.IndexOf("10");
            if (idx < 0) idx = 0;
            _cmbFontSize.SelectedIndex = idx;

            _cmbFontSize.SelectedIndexChanged += (_, __) =>
            {
                if (float.TryParse(_cmbFontSize.SelectedItem?.ToString(), out float fs))
                    ApplyZoomFontSize(fs);
            };

            rightBox.Controls.Add(lbl);
            rightBox.Controls.Add(_cmbFontSize);

            // ✅ 구분선 1px (선택)
            var line = new Panel();
            line.Dock = DockStyle.Bottom;
            line.Height = 1;
            line.BackColor = Color.Gainsboro;

            _topPanel.Controls.Clear();
            _topPanel.Controls.Add(rightBox);
            _topPanel.Controls.Add(line);

            Controls.Add(_topPanel);

            gridZoom.Dock = DockStyle.Fill;
            _topPanel.BringToFront();
        }

        private void PlaceNextToOwner(Form owner, bool right)
        {
            if (owner == null)
            {
                StartPosition = FormStartPosition.CenterScreen;
                return;
            }

            int gap = 10;
            var ob = owner.Bounds;

            int x = right ? (ob.Right + gap) : (ob.Left - Width - gap);
            int y = ob.Top;

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
            if (col == null) return false;
            if (string.Equals(col.HeaderText, "QV", StringComparison.OrdinalIgnoreCase)) return true;
            if (col.Name != null && col.Name.IndexOf("QV", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (col.Name != null && col.Name.IndexOf("QuickView", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private void BuildColumnsFrom(DataGridView src)
        {
            // 기본 설정
            gridZoom.AllowUserToAddRows = false;
            gridZoom.AllowUserToDeleteRows = false;
            gridZoom.RowHeadersVisible = false;
            gridZoom.SelectionMode = src.SelectionMode;
            gridZoom.ReadOnly = src.ReadOnly;

            // 가운데 정렬
            gridZoom.EnableHeadersVisualStyles = false;
            gridZoom.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            gridZoom.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            gridZoom.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // 원본 느낌 유지
            gridZoom.Font = new Font(src.Font.FontFamily, src.Font.Size, src.Font.Style);
            gridZoom.RowTemplate.Height = src.RowTemplate.Height;
            gridZoom.ColumnHeadersHeight = src.ColumnHeadersHeight;
            gridZoom.ColumnHeadersHeightSizeMode = src.ColumnHeadersHeightSizeMode;

            // ====== 기준값 캡처(최초 1회) ======
            if (!_baseCaptured)
            {
                _baseCaptured = true;
                _baseFontSize = src.Font.Size;
                _baseHeaderHeight = src.ColumnHeadersHeight > 0 ? src.ColumnHeadersHeight : 24;
                _baseRowHeight = src.RowTemplate.Height > 0 ? src.RowTemplate.Height : 22;
                _baseClientSize = ClientSize;
            }
            // ==================================

            gridZoom.Columns.Clear();

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

                newCol.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                newCol.FillWeight = Math.Max(10, col.Width);

                newCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                newCol.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;

                gridZoom.Columns.Add(newCol);

                _srcToDstColMap[col.Index] = dstIndex;
                dstIndex++;
            }

            // QV를 맨 앞으로
            var qvCol = gridZoom.Columns["colRxQuickView"];
            if (qvCol != null)
                qvCol.DisplayIndex = 0;

            // 정렬 화살표 제거
            foreach (DataGridViewColumn c in gridZoom.Columns)
                c.SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        public void ApplyZoomFontSize(float fontSize)
        {
            if (fontSize <= 1f) return;

            float scale = (_baseFontSize <= 0f) ? 1f : (fontSize / _baseFontSize);

            gridZoom.SuspendLayout();
            try
            {
                // 폰트
                gridZoom.Font = new Font(gridZoom.Font.FontFamily, fontSize, gridZoom.Font.Style);

                // 헤더/행 높이
                int headerH = Math.Max((int)Math.Round(_baseHeaderHeight * scale), 24);
                int rowH = Math.Max((int)Math.Round(_baseRowHeight * scale), 22);

                gridZoom.ColumnHeadersHeight = headerH;
                gridZoom.RowTemplate.Height = rowH;

                foreach (DataGridViewRow r in gridZoom.Rows)
                    r.Height = rowH;

                // QV 폭 스케일
                var qv = gridZoom.Columns["colRxQuickView"];
                if (qv != null)
                {
                    int qvW = (int)Math.Round(30 * scale);
                    qvW = Math.Max(30, qvW);
                    qvW = Math.Min(60, qvW);

                    qv.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    qv.Width = qvW;
                    qv.MinimumWidth = qvW;
                }

                // 창 크기도 스케일 (뒤 열 잘림 방지)
                int newW = (int)Math.Round(_baseClientSize.Width * scale);
                int newH = (int)Math.Round(_baseClientSize.Height * scale);

                newW = Math.Max(480, newW);
                newH = Math.Max(600, newH);

                ClientSize = new Size(newW, newH);
            }
            finally
            {
                gridZoom.ResumeLayout();
            }
        }

        private void SyncAllRows()
        {
            if (_src == null || _src.IsDisposed)
            {
                try { Close(); } catch { }
                return;
            }

            if (InvokeRequired)
            {
                try { BeginInvoke(new Action(SyncAllRows)); } catch { }
                return;
            }

            gridZoom.SuspendLayout();

            try
            {
                int srcRowCount = 0;
                foreach (DataGridViewRow r in _src.Rows)
                {
                    if (!r.IsNewRow) srcRowCount++;
                }

                while (gridZoom.Rows.Count < srcRowCount)
                    gridZoom.Rows.Add();

                while (gridZoom.Rows.Count > srcRowCount)
                    gridZoom.Rows.RemoveAt(gridZoom.Rows.Count - 1);

                int dstRow = 0;

                foreach (DataGridViewRow row in _src.Rows)
                {
                    if (row.IsNewRow) continue;

                    int desiredH = gridZoom.RowTemplate.Height;
                    if (gridZoom.Rows[dstRow].Height != desiredH)
                        gridZoom.Rows[dstRow].Height = desiredH;

                    for (int srcCol = 0; srcCol < row.Cells.Count; srcCol++)
                    {
                        int dstCol = _srcToDstColMap[srcCol];
                        if (dstCol < 0) continue;

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
