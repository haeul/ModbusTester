using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormMain
    {
        private void btnQuickView_Click(object sender, EventArgs e)
        {
            var targets = GetRxQuickViewTargets();
            if (targets.Count == 0)
            {
                MessageBox.Show("QuickView에 표시할 레지스터를 QV 컬럼에서 선택하세요.");
                return;
            }

            if (_quick == null || _quick.IsDisposed)
            {
                _quick = new FormQuickView(targets);
                _quick.UpdateTargets(targets);
                _quick.Show(this);

                var wa = Screen.FromControl(this).WorkingArea;
                int x = Math.Min(Right, wa.Right - _quick.Width);
                int y = Math.Max(wa.Top, Math.Min(Top, wa.Bottom - _quick.Height));
                _quick.Location = new System.Drawing.Point(x, y);
            }
            else
            {
                _quick.UpdateTargets(targets);
                _quick.Focus();
            }
        }

        private List<(ushort addr, string label)> GetRxQuickViewTargets()
        {
            var list = new List<(ushort addr, string label)>();

            int selColIndex = gridRx.Columns["colRxQuickView"]?.Index ?? -1;
            if (selColIndex < 0) return list;

            foreach (DataGridViewRow r in gridRx.Rows)
            {
                if (r.IsNewRow) continue;

                bool isChecked = r.Cells[selColIndex].Value is bool b && b;
                if (!isChecked) continue;

                string regText = Convert.ToString(r.Cells[COL_REG].Value) ?? "";
                if (!TryParseUShortFromRegText(regText, out ushort addr))
                    continue;

                string name = Convert.ToString(r.Cells[COL_NAME].Value) ?? "";
                string label = !string.IsNullOrWhiteSpace(name)
                               ? name.Trim()
                               : $"{addr:X4}h";

                list.Add((addr, label));
            }

            return list;
        }

        private static bool TryParseUShortFromRegText(string s, out ushort value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;

            s = s.Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                s = s[2..];
            if (s.EndsWith("h", StringComparison.OrdinalIgnoreCase))
                s = s[..^1];

            return ushort.TryParse(s, NumberStyles.HexNumber, null, out value);
        }
    }
}
