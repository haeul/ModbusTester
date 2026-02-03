using System;
using ModbusTester.Modbus;
using System.Text;

namespace ModbusTester.Services
{
    /// <summary>
    /// FormMain 대신 Modbus 송수신을 책임지는 서비스.
    /// - 프레임 구성: ModbusRtu
    /// - 실제 전송: SerialModbusClient
    /// </summary>
    public class ModbusMasterService : IModbusMasterService
    {
        private readonly SerialModbusClient _client;

        public ModbusMasterService(SerialModbusClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public class ModbusReadResult
        {
            public byte[] Request { get; set; } = Array.Empty<byte>();
            public byte[] Response { get; set; } = Array.Empty<byte>();
            public ushort[] Values { get; set; } = Array.Empty<ushort>();
        }

        public class ModbusWriteResult
        {
            public byte[] Request { get; set; } = Array.Empty<byte>();
            public byte[] Response { get; set; } = Array.Empty<byte>();
        }

        public ModbusReadResult ReadRegisters(byte slave, FunctionCode fc, ushort start, ushort count)
        {
            if (count == 0)
                throw new ArgumentOutOfRangeException(nameof(count), "count는 0보다 커야 합니다.");

            byte fcByte = (byte)fc;
            if (fcByte != 0x03 && fcByte != 0x04)
                throw new NotSupportedException("ReadRegisters는 FC=03,04만 지원합니다.");

            var req = ModbusRtu.BuildReadFrame(slave, fcByte, start, count);
            var resp = _client.SendAndReceive(req);

            // ── 응답 기본 진단 ──
            if (resp == null || resp.Length == 0)
                throw new Exception("응답이 없습니다. (타임아웃/배선/포트 설정/Slave ID 확인)");

            if (resp.Length < 5)
                throw new Exception($"응답 프레임이 너무 짧습니다. (len={resp.Length}) RX={ToHex(resp)}");

            // CRC 진단 (노이즈/잘림/설정 불일치 시 도움)
            if (!HasValidCrc(resp))
                throw new Exception($"CRC 오류로 보이는 응답입니다. RX={ToHex(resp)}");

            if (resp[0] != slave)
                throw new Exception($"슬레이브 주소 불일치 (요청:{slave} / 응답:{resp[0]}) RX={ToHex(resp)}");

            // 예외 응답: FC에 0x80 OR
            if (resp[1] == (byte)(fcByte | 0x80))
                throw new Exception($"장치 예외 응답 (FC=0x{resp[1]:X2}, EX=0x{resp[2]:X2}) RX={ToHex(resp)}");

            // 정상 Read 응답 길이 검증: [Slave][FC][ByteCount][Data...][CRC(2)]
            // 최소 5바이트는 위에서 체크함. 이제 ByteCount 기반으로 기대 길이 확인.
            int byteCount = resp[2];
            int expectedLen = 3 + byteCount + 2;
            if (resp.Length < expectedLen)
                throw new Exception($"응답 길이가 부족합니다. (expected>={expectedLen}, actual={resp.Length}) RX={ToHex(resp)}");

            var values = ModbusRtu.ParseReadResponse(resp);

            return new ModbusReadResult
            {
                Request = req,
                Response = resp,
                Values = values
            };
        }

        public ModbusWriteResult WriteSingleRegister(byte slave, ushort addr, ushort value)
        {
            var req = ModbusRtu.BuildWriteSingleFrame(slave, addr, value);
            var resp = _client.SendAndReceive(req);

            if (resp == null || resp.Length == 0)
                throw new Exception("응답이 없습니다. (타임아웃/배선/포트 설정/Slave ID 확인)");

            if (resp.Length < 5)
                throw new Exception($"응답 프레임이 너무 짧습니다. (len={resp.Length}) RX={ToHex(resp)}");

            if (!HasValidCrc(resp))
                throw new Exception($"CRC 오류로 보이는 응답입니다. RX={ToHex(resp)}");

            if (resp[0] != slave)
                throw new Exception($"슬레이브 주소 불일치 (요청:{slave} / 응답:{resp[0]}) RX={ToHex(resp)}");

            // 예외 응답(5바이트): [Slave][FC|0x80][EX][CRC]
            if (resp[1] == (byte)(0x06 | 0x80))
                throw new Exception($"장치 예외 응답 (FC=0x{resp[1]:X2}, EX=0x{resp[2]:X2}) RX={ToHex(resp)}");

            // 정상 응답(8바이트): [Slave][06][AddrHi][AddrLo][ValHi][ValLo][CRC]
            if (resp.Length < 8)
                throw new Exception($"응답 길이가 부족합니다. (expected>=8, actual={resp.Length}) RX={ToHex(resp)}");

            return new ModbusWriteResult
            {
                Request = req,
                Response = resp
            };
        }

        public ModbusWriteResult WriteMultipleRegisters(byte slave, ushort start, ushort[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("values가 비어 있습니다.", nameof(values));

            if (values.Length > 125)
                throw new ArgumentOutOfRangeException(nameof(values), "한 번에 쓸 수 있는 레지스터 개수를 초과했습니다.");

            var req = ModbusRtu.BuildWriteMultipleFrame(slave, start, values);
            var resp = _client.SendAndReceive(req);

            if (resp == null || resp.Length == 0)
                throw new Exception("응답이 없습니다. (타임아웃/배선/포트 설정/Slave ID 확인)");

            if (resp.Length < 5)
                throw new Exception($"응답 프레임이 너무 짧습니다. (len={resp.Length}) RX={ToHex(resp)}");

            if (!HasValidCrc(resp))
                throw new Exception($"CRC 오류로 보이는 응답입니다. RX={ToHex(resp)}");

            if (resp[0] != slave)
                throw new Exception($"슬레이브 주소 불일치 (요청:{slave} / 응답:{resp[0]}) RX={ToHex(resp)}");

            // 예외 응답(5바이트): [Slave][FC|0x80][EX][CRC]
            if (resp[1] == (byte)(0x10 | 0x80))
                throw new Exception($"장치 예외 응답 (FC=0x{resp[1]:X2}, EX=0x{resp[2]:X2}) RX={ToHex(resp)}");

            // 정상 응답(8바이트): [Slave][10][StartHi][StartLo][QtyHi][QtyLo][CRC]
            if (resp.Length < 8)
                throw new Exception($"응답 길이가 부족합니다. (expected>=8, actual={resp.Length}) RX={ToHex(resp)}");

            return new ModbusWriteResult
            {
                Request = req,
                Response = resp
            };
        }

        private static bool HasValidCrc(byte[] frame)
        {
            if (frame == null || frame.Length < 3) return false;

            int len = frame.Length;
            ushort crc = ModbusRtu.Crc16(frame, len - 2);
            byte crcLo = (byte)(crc & 0xFF);
            byte crcHi = (byte)((crc >> 8) & 0xFF);

            return frame[len - 2] == crcLo && frame[len - 1] == crcHi;
        }

        private static string ToHex(byte[] data)
        {
            if (data == null || data.Length == 0) return "";
            var sb = new StringBuilder(data.Length * 3);
            for (int i = 0; i < data.Length; i++)
            {
                if (i > 0) sb.Append(' ');
                sb.Append(data[i].ToString("X2"));
            }
            return sb.ToString();
        }
    }
}
