using System;
using System.Globalization;
using System.Text;

namespace ModbusTester.Utils
{
    public static class HexExtensions
    {
        public static string ToHex(this byte[] data, string sep = " ")
        {
            if (data == null) return string.Empty;
            var sb = new StringBuilder(data.Length * 3);
            foreach (var b in data) sb.Append(b.ToString("X2")).Append(sep);
            return sb.ToString().TrimEnd();
        }

        public static byte[] HexToBytes(string hex)
        {
            // "01 03 00 00 00 02 C4 0B" 형태 허용
            hex = hex.Replace(" ", "").Replace("-", "");
            if (hex.Length % 2 != 0) throw new FormatException("Hex length must be even.");
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);
            return bytes;
        }
    }
}
