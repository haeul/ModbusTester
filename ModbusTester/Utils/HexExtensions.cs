using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ModbusTester.Utils
{
    public static class HexExtensions
    {
        // byte[] → "01 03 00 00 ..."
        public static string ToHex(this byte[] data, string sep = " ")
        {
            if (data == null) return string.Empty;
            var sb = new StringBuilder(data.Length * 3);
            foreach (var b in data)
                sb.Append(b.ToString("X2")).Append(sep);
            return sb.ToString().TrimEnd();
        }

        // ushort → "000Ah" 같은 4자리 HEX + h
        public static string ToHex4(this ushort v)
        {
            return $"{v:X4}h";
        }

        // ushort → "0000 0000 0000 0000"
        public static string ToBitString16(this ushort v)
        {
            string s = Convert.ToString(v, 2).PadLeft(16, '0');
            return string.Join(" ", Enumerable.Range(0, 4)
                                              .Select(i => s.Substring(i * 4, 4)));
        }

        // (필요시) 문자열 HEX → ushort
        public static bool TryParseUShortFromHex(string s, out ushort value)
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

        // (필요시) BIT 문자열 → ushort
        public static bool TryParseUShortFromBitString(string s, out ushort value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false;

            s = new string(s.Where(ch => ch == '0' || ch == '1').ToArray());
            if (s.Length == 0) return false;

            if (s.Length > 16)
                s = s[^16..];

            s = s.PadLeft(16, '0');

            try
            {
                value = Convert.ToUInt16(s, 2);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
