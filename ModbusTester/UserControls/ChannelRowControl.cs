using ModbusTester.Utils;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using ModbusTester.Utils;

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
            lblDecValue.Text = raw.ToString();
            lblHexValue.Text = raw.ToHex4();
            lblBitValue.Text = raw.ToBitString16();
        }

        public void ClearValues()
        {
            ushort zero = 0;
            lblDecValue.Text = "0";
            lblHexValue.Text = zero.ToHex4();
            lblBitValue.Text = zero.ToBitString16();
        }
    }
}
