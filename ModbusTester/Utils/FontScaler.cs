using System.Drawing;
using System.Windows.Forms;

namespace ModbusTester.Utils
{
    /// <summary>
    /// 레이아웃(Bounds/Location) 스케일이 아닌 "Font.Size"만 바꾸는 스케일러.
    /// 확대창/QuickView처럼 "보기(가독성)" 목적 UI에 사용.
    /// </summary>
    public class FontScaler
    {
        public float CurrentSize { get; private set; }

        public void Apply(Control root, float fontSize)
        {
            CurrentSize = fontSize;
            ApplyRecursive(root, fontSize);
        }

        private void ApplyRecursive(Control c, float size)
        {
            if (c.Font != null)
            {
                c.Font = new Font(c.Font.FontFamily, size, c.Font.Style);
            }

            foreach (Control child in c.Controls)
            {
                ApplyRecursive(child, size);
            }
        }
    }
}
