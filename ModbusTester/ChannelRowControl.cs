using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class ChannelRowControl : UserControl
    {
        public ushort Address { get; set; }

        public string LabelText
        {
            get => lblBadge.Text;
            set => lblBadge.Text = value;
        }

        public ChannelRowControl()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Form에서 디지털 폰트를 로드했다면 여기로 전달해서 사용할 수 있음.
        /// </summary>
        public void SetValueFont(Font font)
        {
            if (font == null) return;
            lblDecValue.Font = font;
        }

        public void UpdateFromRaw(ushort raw)
        {
            // DEC
            lblDecValue.Text = raw.ToString();

            // HEX (4자리 + h)
            lblHexValue.Text = $"{raw:X4}h";

            // BIT (16비트: 0000 0000 0000 0000)
            string bits = Convert.ToString(raw, 2).PadLeft(16, '0');
            bits = string.Join(" ", Enumerable.Range(0, 4)
                                              .Select(i => bits.Substring(i * 4, 4)));
            lblBitValue.Text = bits;
        }

        public void ClearValues()
        {
            lblDecValue.Text = "0";
            lblHexValue.Text = "0000h";
            lblBitValue.Text = "0000 0000 0000 0000";
        }
    }
}
