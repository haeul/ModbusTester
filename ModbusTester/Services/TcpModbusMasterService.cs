using ModbusTester.Modbus;
using NModbus;
using System;

namespace ModbusTester.Services
{
    /// <summary>
    /// NModbus(I​ModbusMaster)를 이용한 Modbus TCP용 Master 서비스
    /// - PresetRunner/Macro에서 RTU와 동일한 호출로 쓰기 위해 어댑터 형태로 제공
    /// - Request/Response는 RTU처럼 CRC 포함 HEX를 얻기 어려워, TCP PDU 형태로 구성해 넣는다.
    /// </summary>
    public sealed class TcpModbusMasterService : IModbusMasterService
    {
        private readonly IModbusMaster _tcpMaster;

        public TcpModbusMasterService(IModbusMaster tcpMaster)
        {
            _tcpMaster = tcpMaster ?? throw new ArgumentNullException(nameof(tcpMaster));
        }

        public ModbusMasterService.ModbusReadResult ReadRegisters(byte slave, FunctionCode fc, ushort start, ushort count)
        {
            byte fcByte = (byte)fc;
            if (fcByte != 0x03 && fcByte != 0x04)
                throw new NotSupportedException("ReadRegisters는 FC=03,04만 지원합니다.");

            ushort[] values =
                (fcByte == 0x03)
                ? _tcpMaster.ReadHoldingRegisters(slave, start, count)
                : _tcpMaster.ReadInputRegisters(slave, start, count);

            // TCP는 RTU HEX 프레임을 그대로 얻기 어려움 → PDU 형태로 구성
            byte[] reqPdu = BuildReadRequestPdu(fcByte, start, count);
            byte[] respPdu = BuildReadResponsePdu(fcByte, values);

            return new ModbusMasterService.ModbusReadResult
            {
                Request = reqPdu,
                Response = respPdu,
                Values = values
            };
        }

        public ModbusMasterService.ModbusWriteResult WriteSingleRegister(byte slave, ushort addr, ushort value)
        {
            // 장비가 FC06 미지원이면 호출 시 IOException/Timeout 등 발생 가능
            _tcpMaster.WriteSingleRegister(slave, addr, value);

            byte[] reqPdu = BuildWriteSingleRequestPdu(addr, value);
            // 정상 응답은 요청 에코(Addr/Value) 형태
            byte[] respPdu = BuildWriteSingleResponsePdu(addr, value);

            return new ModbusMasterService.ModbusWriteResult
            {
                Request = reqPdu,
                Response = respPdu
            };
        }

        public ModbusMasterService.ModbusWriteResult WriteMultipleRegisters(byte slave, ushort start, ushort[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("values가 비어 있습니다.", nameof(values));

            _tcpMaster.WriteMultipleRegisters(slave, start, values);

            byte[] reqPdu = BuildWriteMultipleRequestPdu(start, values);
            // 정상 응답은 (Start, Count) 에코
            byte[] respPdu = BuildWriteMultipleResponsePdu(start, (ushort)values.Length);

            return new ModbusMasterService.ModbusWriteResult
            {
                Request = reqPdu,
                Response = respPdu
            };
        }

        // ===== PDU Builders =====

        private static byte[] BuildReadRequestPdu(byte fc, ushort start, ushort count)
        {
            // FC + StartHi/Lo + CountHi/Lo
            return new byte[]
            {
                fc,
                (byte)(start >> 8), (byte)(start & 0xFF),
                (byte)(count >> 8), (byte)(count & 0xFF)
            };
        }

        private static byte[] BuildReadResponsePdu(byte fc, ushort[] values)
        {
            // FC + ByteCount + Data...
            int byteCount = values.Length * 2;
            byte[] resp = new byte[2 + byteCount];
            resp[0] = fc;
            resp[1] = (byte)byteCount;

            int idx = 2;
            for (int i = 0; i < values.Length; i++)
            {
                ushort v = values[i];
                resp[idx++] = (byte)(v >> 8);
                resp[idx++] = (byte)(v & 0xFF);
            }
            return resp;
        }

        private static byte[] BuildWriteSingleRequestPdu(ushort addr, ushort value)
        {
            // FC06 + Addr + Value
            return new byte[]
            {
                0x06,
                (byte)(addr >> 8), (byte)(addr & 0xFF),
                (byte)(value >> 8), (byte)(value & 0xFF)
            };
        }

        private static byte[] BuildWriteSingleResponsePdu(ushort addr, ushort value)
            => BuildWriteSingleRequestPdu(addr, value);

        private static byte[] BuildWriteMultipleRequestPdu(ushort start, ushort[] values)
        {
            // FC10 + Start + Count + ByteCount + Data...
            ushort count = (ushort)values.Length;
            byte byteCount = (byte)(count * 2);

            byte[] req = new byte[6 + byteCount];
            req[0] = 0x10;
            req[1] = (byte)(start >> 8);
            req[2] = (byte)(start & 0xFF);
            req[3] = (byte)(count >> 8);
            req[4] = (byte)(count & 0xFF);
            req[5] = byteCount;

            int idx = 6;
            for (int i = 0; i < values.Length; i++)
            {
                ushort v = values[i];
                req[idx++] = (byte)(v >> 8);
                req[idx++] = (byte)(v & 0xFF);
            }
            return req;
        }

        private static byte[] BuildWriteMultipleResponsePdu(ushort start, ushort count)
        {
            // FC10 + Start + Count
            return new byte[]
            {
                0x10,
                (byte)(start >> 8), (byte)(start & 0xFF),
                (byte)(count >> 8), (byte)(count & 0xFF)
            };
        }
    }
}
