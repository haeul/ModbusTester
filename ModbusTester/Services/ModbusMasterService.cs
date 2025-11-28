using System;
using ModbusTester.Modbus;

namespace ModbusTester.Services
{
    /// <summary>
    /// FormMain 대신 Modbus 송수신을 책임지는 서비스.
    /// - 프레임 구성: ModbusRtu
    /// - 실제 전송: SerialModbusClient
    /// </summary>
    public class ModbusMasterService
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

            if (resp == null || resp.Length < 5)
                throw new Exception("응답 프레임 길이가 너무 짧습니다.");

            if (resp[0] != slave)
                throw new Exception($"슬레이브 주소 불일치 (요청: {slave}, 응답: {resp[0]})");

            if (resp[1] == (byte)(fcByte | 0x80))
                throw new Exception($"장치 예외 응답 (예외코드: {resp[2]})");

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

            return new ModbusWriteResult
            {
                Request = req,
                Response = resp
            };
        }
    }
}
