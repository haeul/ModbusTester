using System;

namespace ModbusTester.Modbus
{
    /// <summary>
    /// Modbus RTU 프레임 구성/파싱 + CRC 계산 담당
    /// (SerialPort, UI 전혀 모름)
    /// </summary>
    public static class ModbusRtu
    {
        // ───────── CRC ─────────
        public static ushort Crc16(byte[] data, int len)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < len; i++)
            {
                crc ^= data[i];
                for (int b = 0; b < 8; b++)
                {
                    bool lsb = (crc & 0x0001) != 0;
                    crc >>= 1;
                    if (lsb) crc ^= 0xA001;
                }
            }
            return crc;
        }

        /// <summary>프레임 바디에 CRC 두 바이트를 붙인 전체 프레임 반환</summary>
        public static byte[] WithCrc(byte[] frameBody)
        {
            var crc = Crc16(frameBody, frameBody.Length);
            var arr = new byte[frameBody.Length + 2];
            Buffer.BlockCopy(frameBody, 0, arr, 0, frameBody.Length);
            arr[^2] = (byte)(crc & 0xFF);
            arr[^1] = (byte)((crc >> 8) & 0xFF);
            return arr;
        }

        // ───────── 프레임 빌더 ─────────

        public static byte[] BuildReadFrame(byte slave, byte fc, ushort start, ushort count)
        {
            var raw = new byte[]
            {
                slave, fc,
                (byte)(start >> 8), (byte)(start & 0xFF),
                (byte)(count >> 8), (byte)(count & 0xFF)
            };
            return WithCrc(raw);
        }

        public static byte[] BuildWriteSingleFrame(byte slave, ushort addr, ushort value)
        {
            var raw = new byte[]
            {
                slave, 0x06,
                (byte)(addr  >> 8), (byte)(addr  & 0xFF),
                (byte)(value >> 8), (byte)(value & 0xFF)
            };
            return WithCrc(raw);
        }

        public static byte[] BuildWriteMultipleFrame(byte slave, ushort start, ushort[] values)
        {
            ushort count = (ushort)values.Length;
            byte byteCount = (byte)(count * 2);

            var raw = new byte[7 + byteCount];
            raw[0] = slave;
            raw[1] = 0x10;
            raw[2] = (byte)(start >> 8);
            raw[3] = (byte)(start & 0xFF);
            raw[4] = (byte)(count >> 8);
            raw[5] = (byte)(count & 0xFF);
            raw[6] = byteCount;

            for (int i = 0; i < values.Length; i++)
            {
                raw[7 + i * 2] = (byte)(values[i] >> 8);
                raw[8 + i * 2] = (byte)(values[i] & 0xFF);
            }

            return WithCrc(raw);
        }

        // ───────── 응답 파싱 ─────────

        public static ushort[] ParseReadResponse(byte[] resp)
        {
            if (resp == null || resp.Length < 5)
                return Array.Empty<ushort>();

            int bc = resp[2];
            int n = bc / 2;
            var arr = new ushort[n];

            for (int i = 0; i < n; i++)
            {
                int idx = 3 + i * 2;
                if (idx + 1 >= resp.Length) break;
                arr[i] = (ushort)((resp[idx] << 8) | resp[idx + 1]);
            }

            return arr;
        }
    }
}
