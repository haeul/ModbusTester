using System;
using System.Drawing;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormMain
    {
        // FormMacroSetting.cs 띄우기
        private void btnMacroSetting_Click(object sender, EventArgs e)
        {
            // 이미 열려 있으면 앞으로 가져오기
            if (_macroForm != null && !_macroForm.IsDisposed)
            {
                if (_macroForm.WindowState == FormWindowState.Minimized)
                    _macroForm.WindowState = FormWindowState.Normal;

                _macroForm.BringToFront();
                _macroForm.Activate();
                return;
            }

            // 새로 생성
            _macroForm = new FormMacroSetting(this);
            _macroForm.StartPosition = FormStartPosition.Manual;

            // 메인 폼 왼쪽에 딱 붙여서 배치
            int formW = _macroForm.Width > 0 ? _macroForm.Width : _macroForm.MinimumSize.Width;

            int x = this.Left - formW;
            int y = this.Top;

            if (y < 0) y = 0;

            _macroForm.Location = new Point(x, y);

            // 닫히면 참조 해제
            _macroForm.FormClosed += (_, __) => _macroForm = null;

            // 모델리스로 표시
            _macroForm.Show(this);
        }
    }
}
