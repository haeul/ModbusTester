using System;
using System.Drawing;
using System.Windows.Forms;

namespace ModbusTester.Services
{
    /// <summary>
    /// RichTextBox 기반 UI 로그 출력 담당
    /// - TX/RX 접두어에 따라 색상 구분
    /// - 마지막 줄 자동 스크롤
    /// </summary>
    public sealed class UiLogger
    {
        private readonly RichTextBox _box;

        public UiLogger(RichTextBox box)
        {
            _box = box ?? throw new ArgumentNullException(nameof(box));
        }

        public void Clear() => _box.Clear();

        public void Append(string line)
        {
            if (line == null) line = string.Empty;

            LogDirection dir = LogDirection.None;

            if (line.StartsWith("TX:", StringComparison.OrdinalIgnoreCase))
                dir = LogDirection.Tx;
            else if (line.StartsWith("RX:", StringComparison.OrdinalIgnoreCase))
                dir = LogDirection.Rx;

            Color color = dir switch
            {
                LogDirection.Tx => Color.DarkBlue,
                LogDirection.Rx => Color.DarkGreen,
                _ => _box.ForeColor
            };

            _box.SelectionStart = _box.TextLength;
            _box.SelectionLength = 0;
            _box.SelectionColor = color;

            _box.AppendText(line + Environment.NewLine);

            _box.SelectionColor = _box.ForeColor;
            _box.ScrollToCaret();
        }

        private enum LogDirection
        {
            None,
            Tx,
            Rx
        }
    }
}
