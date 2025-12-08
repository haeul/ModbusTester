using System.Windows.Forms;

namespace ModbusTester.Controls
{
    public class HexNumericUpDown : NumericUpDown
    {
        public HexNumericUpDown()
        {
            Minimum = 0;
            Maximum = 0xFFFF;      // 0 ~ 65535
            Hexadecimal = true;    // Up Down 버튼이 16진수로 움직이도록
        }

        // 표시 형식만 0000h 로 바꿔줌
        protected override void UpdateEditText()
        {
            int v = (int)Value;
            Text = v.ToString("X4") + "h";
        }
    }
}
