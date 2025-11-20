using System;
using ModbusTester.Modbus;

namespace ModbusTester.Services
{
    /// <summary>
    /// 주기적으로 Modbus Read를 수행하는 폴링 전담 클래스.
    /// - 프레임 생성은 ModbusRtu 사용
    /// - 실제 송수신은 SerialModbusClient 사용
    /// - 결과(요청/응답/값/예외 여부)는 PollResult로 반환
    /// </summary>
    public class ModbusPoller
    {
        private readonly SerialModbusClient _client;

        public ModbusPoller(SerialModbusClient client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        /// <summary>
        /// 한 번의 Read 폴링 수행.
        /// FC=03,04만 대상으로 가정.
        /// </summary>
        public PollResult Poll(byte slaveId, byte functionCode, ushort startAddress, ushort registerCount)
        {
            if (!(functionCode == 0x03 || functionCode == 0x04))
                throw new NotSupportedException("폴링은 FC=03,04만 지원합니다.");

            // 1) 요청 프레임 생성
            var req = ModbusRtu.BuildReadFrame(slaveId, functionCode, startAddress, registerCount);

            // 2) 전송 및 응답 수신
            var resp = _client.SendAndReceive(req);

            // 3) 예외 응답인지 확인
            bool isException = false;
            byte? exCode = null;
            ushort[] values = Array.Empty<ushort>();

            if (resp.Length >= 3 && resp[1] == (byte)(functionCode | 0x80))
            {
                isException = true;
                exCode = resp[2];
            }
            else
            {
                values = ModbusRtu.ParseReadResponse(resp);
            }

            return new PollResult(req, resp, values, isException, exCode);
        }
    }

    /// <summary>
    /// 한 번의 폴링 결과를 담는 DTO.
    /// </summary>
    public class PollResult
    {
        public byte[] Request { get; }
        public byte[] Response { get; }
        public ushort[] Values { get; }
        public bool IsException { get; }
        public byte? ExceptionCode { get; }

        public PollResult(byte[] request, byte[] response, ushort[] values, bool isException, byte? exceptionCode)
        {
            Request = request;
            Response = response;
            Values = values;
            IsException = isException;
            ExceptionCode = exceptionCode;
        }
    }
}
