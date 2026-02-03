using ModbusTester.Utils;
using System;
using System.Windows.Forms;

namespace ModbusTester.Forms
{
    public partial class FormStartMode : Form
    {
        private readonly LayoutScaler _layoutScaler;

        public FormStartMode()
        {
            InitializeComponent();

            this.Text = $"ModbusTester-v{AppVersion.Get()}";
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _layoutScaler = new LayoutScaler(this);
            _layoutScaler.ApplyInitialScale(1.3f);

            StartPosition = FormStartPosition.CenterScreen;

            btnNext.Click += BtnNext_Click;
        }

        private void BtnNext_Click(object? sender, EventArgs e)
        {
            // 1) RTU
            if (rbRtu.Checked)
            {
                var frm = new FormComSetting(); // 기존 RTU 설정창
                frm.FormClosed += (_, __) => this.Show();
                this.Hide();
                frm.Show();
                return;
            }

            // 2) TCP
            if (rbTcp.Checked)
            {
                var frm = new FormTcpSetting(); // 다음 단계에서 만들 폼
                frm.FormClosed += (_, __) => this.Show();
                this.Hide();
                frm.Show();
                return;
            }

            MessageBox.Show("통신 모드를 선택하세요.");
        }
    }
}
