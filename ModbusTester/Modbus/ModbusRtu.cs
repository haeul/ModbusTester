using System;

namespace ModbusTester.Modbus
{
    /// <summary>
    /// Modbus Function Code 정의.
    /// </summary>
    public enum FunctionCode : byte
    {
        ReadHoldingRegisters = 0x03,
        ReadInputRegisters = 0x04,
        WriteSingleRegister = 0x06,
        WriteMultipleRegisters = 0x10,
    }

    /// <summary>
    /// Modbus RTU 요청 메타 정보 DTO.
    /// </summary>
    public record RtuRequest(
        byte Slave,
        FunctionCode Function,
        ushort Start,
        ushort CountOrValue,
        byte[]? Payload
    );

    // ───────────────── CRC Helper ─────────────────
    internal static class ModbusCrc16
    {
        /// <summary>
        /// Modbus RTU CRC-16 계산 (poly 0xA001, init 0xFFFF).
        /// </summary>
        public static ushort Compute(byte[] data, int length)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (length < 0 || length > data.Length)
                throw new ArgumentOutOfRangeException(nameof(length));

            ushort crc = 0xFFFF;

            for (int i = 0; i < length; i++)
            {
                crc ^= data[i];
                for (int b = 0; b < 8; b++)
                {
                    bool lsb = (crc & 0x0001) != 0;
                    crc >>= 1;
                    if (lsb)
                    {
                        crc ^= 0xA001;
                    }
                }
            }

            return crc;
        }

        /// <summary>
        /// CRC 값을 Modbus 규격에 맞게 (Lo, Hi) 순서로 버퍼에 기록.
        /// </summary>
        public static void AppendLittleEndian(byte[] buffer, int offset, ushort crc) // offset : CRC를 쓰기 시작할 위치 인덱스
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));
            if (offset < 0 || offset + 1 >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            buffer[offset + 0] = (byte)(crc & 0xFF);        // CRC Lo
            buffer[offset + 1] = (byte)((crc >> 8) & 0xFF); // CRC Hi
        }
    }

    /// <summary>
    /// Modbus RTU 프레임 구성/파싱 + CRC 계산 담당
    /// (SerialPort, UI 전혀 모름)
    /// </summary>
    public static class ModbusRtu
    {
        // ───────── CRC ─────────

        /// <summary>
        /// 기존 코드 호환용: FormMain 등에서 사용하는 메서드 그대로 유지.
        /// 내부적으로 ModbusCrc16.Compute를 호출한다.
        /// </summary>
        public static ushort Crc16(byte[] data, int len)
            => ModbusCrc16.Compute(data, len);

        /// <summary>
        /// 프레임 바디에 CRC 두 바이트를 붙인 전체 프레임 반환.
        /// </summary>
        public static byte[] WithCrc(byte[] frameBody)
        {
            if (frameBody == null) throw new ArgumentNullException(nameof(frameBody));

            var crc = ModbusCrc16.Compute(frameBody, frameBody.Length);
            var arr = new byte[frameBody.Length + 2];

            Buffer.BlockCopy(frameBody, 0, arr, 0, frameBody.Length); // Buffer.BlockCopy(sourceArray, sourceOffset, destinationArray, destinationOffset, count)
            ModbusCrc16.AppendLittleEndian(arr, frameBody.Length, crc);

            return arr;
        }

        // ───────── 프레임 빌더 ─────────

        /// <summary>
        /// FC=03,04 Read 프레임 생성.
        /// </summary>
        public static byte[] BuildReadFrame(byte slave, byte fc, ushort start, ushort count)
        {
            var raw = new byte[6];
            raw[0] = slave;
            raw[1] = fc;
            raw[2] = (byte)(start >> 8);
            raw[3] = (byte)(start & 0xFF);
            raw[4] = (byte)(count >> 8);
            raw[5] = (byte)(count & 0xFF);

            return WithCrc(raw);
        }

        /// <summary>
        /// FC=06 Write Single Register 프레임 생성.
        /// </summary>
        public static byte[] BuildWriteSingleFrame(byte slave, ushort addr, ushort value)
        {
            var raw = new byte[6];
            raw[0] = slave;
            raw[1] = 0x06;
            raw[2] = (byte)(addr >> 8);
            raw[3] = (byte)(addr & 0xFF);
            raw[4] = (byte)(value >> 8);
            raw[5] = (byte)(value & 0xFF);

            return WithCrc(raw);
        }

        /// <summary>
        /// FC=16(0x10) Write Multiple Registers 프레임 생성.
        /// </summary>
        public static byte[] BuildWriteMultipleFrame(byte slave, ushort start, ushort[] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

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

        /// <summary>
        /// Read 응답(FC=03,04)의 Data 영역을 ushort 배열로 파싱.
        /// </summary>
        public static ushort[] ParseReadResponse(byte[] resp)
        {
            if (resp == null || resp.Length < 5)
                return Array.Empty<ushort>();

            int bc = resp[2];    // Byte count
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
