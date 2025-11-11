using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ModbusTester
{
    public class FormQuickView : Form
    {
        private readonly Label _lbl12 = new Label();
        private readonly Label _lbl2A = new Label();
        private readonly TableLayoutPanel _pnl = new TableLayoutPanel();

        public ushort Addr12h { get; set; } = 0x0012;
        public ushort Addr2Ah { get; set; } = 0x002A;

        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x02;

        // ★ Borderless 리사이즈 허용
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_MINIMIZEBOX = 0x00020000;
                const int WS_MAXIMIZEBOX = 0x00010000;
                const int WS_THICKFRAME = 0x00040000;
                var cp = base.CreateParams;
                cp.Style |= WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;
                return cp;
            }
        }

        public FormQuickView()
        {
            Text = "Quick View";
            FormBorderStyle = FormBorderStyle.None;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            BackColor = Color.Black;
            Opacity = 1.0;                  // ← 반투명 제거
            AutoScaleMode = AutoScaleMode.Dpi;
            DoubleBuffered = true;

            // ★ 기본 크기/최소 크기 (질문: “처음 사이즈 키우기”)
            Size = new Size(520, 220);      // ← 원하는 초기 크기
            MinimumSize = new Size(420, 180);

            // 라벨 스타일
            var fontBig = new Font("Segoe UI", 28, FontStyle.Bold);
            InitLabel(_lbl12, fontBig, Color.Lime);
            InitLabel(_lbl2A, fontBig, Color.Aqua);

            // 레이아웃
            _pnl.Dock = DockStyle.Fill;
            _pnl.ColumnCount = 1;
            _pnl.RowCount = 2;
            _pnl.Padding = new Padding(16);
            _pnl.BackColor = Color.Black;

            // ★ 폰트 높이에 맞춰 절대 높이로 고정 → 잘림 방지
            int rowH = fontBig.Height + 20; // 글자+패딩 여유
            _pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, rowH));
            _pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, rowH));

            _pnl.Controls.Add(_lbl12, 0, 0);
            _pnl.Controls.Add(_lbl2A, 0, 1);
            Controls.Add(_pnl);

            // 어디든 드래그 이동
            MouseDown += DragMove; _pnl.MouseDown += DragMove;
            _lbl12.MouseDown += DragMove; _lbl2A.MouseDown += DragMove;

            RegisterCache.Changed += OnCacheChanged;
            Shown += (_, __) => { PlaceNearCursor(); EnsureOnScreen(); RefreshValues(); };
            SizeChanged += (_, __) => EnsureOnScreen();
            FormClosed += (_, __) => RegisterCache.Changed -= OnCacheChanged;

            // 우클릭 닫기 / 더블클릭 TopMost 토글
            MouseUp += (_, e) => { if (e.Button == MouseButtons.Right) Close(); };
            MouseDoubleClick += (_, __) => TopMost = !TopMost;
        }

        private void InitLabel(Label lab, Font font, Color fore)
        {
            lab.AutoSize = false;
            lab.Dock = DockStyle.Fill;               // ← 영역 꽉 채우기
            lab.TextAlign = ContentAlignment.MiddleLeft;
            lab.Padding = new Padding(10, 0, 10, 0); // ← 여백
            lab.Font = font;
            lab.ForeColor = fore;
            lab.BackColor = Color.Black;
        }

        private void DragMove(object? s, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, (IntPtr)HTCAPTION, IntPtr.Zero);
            }
        }

        private void OnCacheChanged() => BeginInvoke((Action)RefreshValues);

        private void RefreshValues()
        {
            _lbl12.Text = RegisterCache.TryGet(Addr12h, out var v12) ? $"12h : {FormatValue(v12)}" : "12h : --";
            _lbl2A.Text = RegisterCache.TryGet(Addr2Ah, out var v2a) ? $"2Ah : {FormatValue(v2a)}" : "2Ah : --";
        }

        private string FormatValue(ushort raw)
        {
            double mA = raw / 1000.0;
            return $"{mA:0.000} mA";
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x84;
            const int HTLEFT = 10, HTRIGHT = 11, HTTOP = 12, HTBOTTOM = 15;
            const int HTTOPLEFT = 13, HTTOPRIGHT = 14, HTBOTTOMLEFT = 16, HTBOTTOMRIGHT = 17;

            if (m.Msg == WM_NCHITTEST)
            {
                long lp = m.LParam.ToInt64();
                int x = (short)(lp & 0xFFFF);
                int y = (short)((lp >> 16) & 0xFFFF);

                var pt = PointToClient(new Point(x, y));
                int grip = 10;

                bool left = pt.X <= grip;
                bool right = pt.X >= ClientSize.Width - grip;
                bool top = pt.Y <= grip;
                bool bottom = pt.Y >= ClientSize.Height - grip;

                if (left && top) { m.Result = (IntPtr)HTTOPLEFT; return; }
                if (right && top) { m.Result = (IntPtr)HTTOPRIGHT; return; }
                if (left && bottom) { m.Result = (IntPtr)HTBOTTOMLEFT; return; }
                if (right && bottom) { m.Result = (IntPtr)HTBOTTOMRIGHT; return; }
                if (top) { m.Result = (IntPtr)HTTOP; return; }
                if (bottom) { m.Result = (IntPtr)HTBOTTOM; return; }
                if (left) { m.Result = (IntPtr)HTLEFT; return; }
                if (right) { m.Result = (IntPtr)HTRIGHT; return; }

                m.Result = (IntPtr)HTCAPTION; // 드래그 이동
                return;
            }
            base.WndProc(ref m);
        }

        private void EnsureOnScreen()
        {
            var wa = Screen.FromPoint(Location).WorkingArea;
            Location = new Point(
                Math.Max(wa.Left, Math.Min(Left, wa.Right - Width)),
                Math.Max(wa.Top, Math.Min(Top, wa.Bottom - Height))
            );
        }

        private void PlaceNearCursor()
        {
            var cur = Cursor.Position;
            Location = new Point(cur.X + 20, cur.Y + 20);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            // 우하단 사이즈 그립
            var grip = new Rectangle(ClientSize.Width - 16, ClientSize.Height - 16, 16, 16);
            ControlPaint.DrawSizeGrip(e.Graphics, Color.FromArgb(220, 220, 220), grip);
        }
    }
}
