using System;
using System.Drawing;
using System.Windows.Forms;

namespace ModbusTester
{
    public class FormQuickView : Form
    {
        private readonly Label _lbl12 = new Label();
        private readonly Label _lbl2A = new Label();
        private readonly TableLayoutPanel _pnl = new TableLayoutPanel();

        // 표시할 레지스터 주소 (기본: 0x0012, 0x002A)
        public ushort Addr12h { get; set; } = 0x0012;
        public ushort Addr2Ah { get; set; } = 0x002A;

        public FormQuickView()
        {
            // ───────── 폼 기본 설정: 순수 WinForms 방식 ─────────
            Text = "Quick View";
            StartPosition = FormStartPosition.Manual;

            // 표준 제목줄 + 버튼 + 사이즈 조절
            FormBorderStyle = FormBorderStyle.Sizable;
            ControlBox = true;
            MinimizeBox = true;
            MaximizeBox = true;

            TopMost = true;                     // 필요 시 항상 위
            BackColor = Color.White;
            Opacity = 1.0;
            AutoScaleMode = AutoScaleMode.Dpi;  // 고해상도 대응
            DoubleBuffered = true;

            // 기본/최소 크기
            Size = new Size(520, 220);
            MinimumSize = new Size(420, 180);

            // ───────── 라벨/레이아웃 구성 ─────────
            var fontBig = new Font("Segoe UI", 28, FontStyle.Bold);
            InitLabel(_lbl12, fontBig, Color.Black);
            InitLabel(_lbl2A, fontBig, Color.Black);

            _pnl.Dock = DockStyle.Fill;
            _pnl.ColumnCount = 1;
            _pnl.RowCount = 2;
            _pnl.Padding = new Padding(16);
            _pnl.BackColor = Color.White;

            // 폰트 높이에 맞춘 절대행 높이 → 잘림 방지
            int rowH = fontBig.Height + 20;
            _pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, rowH));
            _pnl.RowStyles.Add(new RowStyle(SizeType.Absolute, rowH));

            _pnl.Controls.Add(_lbl12, 0, 0);
            _pnl.Controls.Add(_lbl2A, 0, 1);
            Controls.Add(_pnl);

            // ───────── 이벤트 연결 ─────────
            // 캐시 변경 시 UI 갱신
            RegisterCache.Changed += OnCacheChanged;

            // 처음 보여줄 때 커서 근처로 위치 + 화면 밖 방지 + 값 갱신
            Shown += (_, __) => { PlaceNearCursor(); EnsureOnScreen(); RefreshValues(); };

            // 창 크기 변경 시에도 화면 밖 방지
            SizeChanged += (_, __) => EnsureOnScreen();

            // 닫힐 때 이벤트 구독 해제
            FormClosed += (_, __) => RegisterCache.Changed -= OnCacheChanged;

            // UX: 우클릭 닫기 / 더블클릭 TopMost 토글 
            MouseUp += (_, e) => { if (e.Button == MouseButtons.Right) Close(); };
            MouseDoubleClick += (_, __) => TopMost = !TopMost;
        }

        private void InitLabel(Label lab, Font font, Color fore)
        {
            lab.AutoSize = false;
            lab.Dock = DockStyle.Fill;                 // 영역 전체 사용
            lab.TextAlign = ContentAlignment.MiddleLeft;
            lab.Padding = new Padding(10, 0, 10, 0);   // 좌우 여백
            lab.Font = font;
            lab.ForeColor = fore;
            lab.BackColor = Color.White;
        }

        // 캐시 이벤트는 다른 스레드에서 올 수 있으므로 UI 스레드로 마샬링
        private void OnCacheChanged() => BeginInvoke((Action)RefreshValues);

        private void RefreshValues()
        {
            _lbl12.Text = RegisterCache.TryGet(Addr12h, out var v12)
                ? $"Channel 1 : {FormatValue(v12)}"
                : "Channel 1 : --";

            _lbl2A.Text = RegisterCache.TryGet(Addr2Ah, out var v2a)
                ? $"Channel 2 : {FormatValue(v2a)}"
                : "Channel 2 : --";
        }

        private string FormatValue(ushort raw)
        {
            if (raw >= 1000.0)
            {
                double mA = raw / 1000.0;
                return $"{mA:0.000} mA";
            }
            else
            {
                double uA = raw;
                return $"{uA:0} uA";
            }
            
        }

        // 모니터 작업영역 안에 머물도록 위치 보정
        private void EnsureOnScreen()
        {
            var wa = Screen.FromPoint(Location).WorkingArea;
            Location = new Point(
                Math.Max(wa.Left, Math.Min(Left, wa.Right - Width)),
                Math.Max(wa.Top, Math.Min(Top, wa.Bottom - Height))
            );
        }

        // 처음 표시 시 커서 근처에 배치
        private void PlaceNearCursor()
        {
            var cur = Cursor.Position;
            Location = new Point(cur.X + 20, cur.Y + 20);
        }
    }
}
