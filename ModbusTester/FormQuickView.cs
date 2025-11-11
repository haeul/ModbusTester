using System;
using System.Drawing;
using System.Windows.Forms;

public class FrmQuickView : Form
{
    private readonly Label _lbl12 = new Label();
    private readonly Label _lbl2A = new Label();

    // 필요시 설정에서 바꾸기 쉽게 상수로
    private const ushort Addr12h = 0x0012;
    private const ushort Addr2Ah = 0x002A;

    public FrmQuickView()
    {
        Text = "Quick Watch";
        FormBorderStyle = FormBorderStyle.None;
        TopMost = true;
        StartPosition = FormStartPosition.Manual;
        BackColor = Color.Black;
        Opacity = 0.9;
        Bounds = new Rectangle(Screen.PrimaryScreen!.WorkingArea.Right - 380,
                               Screen.PrimaryScreen!.WorkingArea.Top + 80,
                               360, 160);

        var fontBig = new Font("Segoe UI", 28, FontStyle.Bold);
        var fontUnit = new Font("Segoe UI", 12, FontStyle.Regular);

        _lbl12.AutoSize = false; _lbl2A.AutoSize = false;
        _lbl12.TextAlign = ContentAlignment.MiddleLeft;
        _lbl2A.TextAlign = ContentAlignment.MiddleLeft;
        _lbl12.Font = fontBig; _lbl2A.Font = fontBig;
        _lbl12.ForeColor = Color.Lime; _lbl2A.ForeColor = Color.Aqua;

        var pnl = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Padding = new Padding(16)
        };
        pnl.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        pnl.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        pnl.Controls.Add(_lbl12, 0, 0);
        pnl.Controls.Add(_lbl2A, 0, 1);

        Controls.Add(pnl);

        DoubleBuffered = true;

        RegisterCache.Changed += OnCacheChanged;
        Shown += (_, __) => RefreshValues();
        FormClosed += (_, __) => RegisterCache.Changed -= OnCacheChanged;

        // 우클릭 닫기
        MouseUp += (_, e) => { if (e.Button == MouseButtons.Right) Close(); };
    }

    private void OnCacheChanged() => BeginInvoke((Action)RefreshValues);

    private void RefreshValues()
    {
        // 12h, 2Ah만 가져와서 보기 좋게 포맷
        if (RegisterCache.TryGet(Addr12h, out var v12))
            _lbl12.Text = $"12h : {FormatValue(v12)}";
        else
            _lbl12.Text = "12h : --";

        if (RegisterCache.TryGet(Addr2Ah, out var v2a))
            _lbl2A.Text = $"2Ah : {FormatValue(v2a)}";
        else
            _lbl2A.Text = "2Ah : --";
    }

    // 예: uA 원시값 → mA 표시(30000 -> 30.000 mA)
    // 필요 없으면 그냥 v.ToString()으로 교체
    private string FormatValue(ushort raw)
    {
        // 부호 있는 값이면 short로 캐스팅
        int ua = raw;                       // uA 가정
        double mA = ua / 1000.0;
        return $"{mA:0.000} mA";
    }
}
