using System;
using System.IO.Ports;

namespace ModbusTester.Services
{
    /// <summary>
    /// SerialPort를 통해 Modbus RTU 요청을 보내고 응답을 읽어오는 전담 클래스.
    /// - 포트는 Form에서 Open/Close 하고
    /// - 이 클래스는 "열려 있는 포트"를 사용하기만 한다.
    /// </summary>
    public class SerialModbusClient
    {
        private readonly SerialPort _sp;
        private readonly object _ioLock = new object();

        public SerialModbusClient(SerialPort sp)
        {
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));
        }

        /// <summary>
        /// Modbus RTU 프레임(TX)을 쓰고, 응답(RX)을 읽어서 그대로 돌려준다.
        /// CRC 검증/파싱은 상위(ModbusRtu, FormMain)에서 처리.
        /// </summary>
        public byte[] SendAndReceive(byte[] request)
        {
            if (request == null || request.Length == 0)
                throw new ArgumentException("요청 프레임이 비어 있습니다.", nameof(request));

            if (!_sp.IsOpen)
                throw new InvalidOperationException("SerialPort가 열려 있지 않습니다.");

            lock (_ioLock)
            {
                _sp.DiscardInBuffer();
                _sp.Write(request, 0, request.Length);

                // 간단한 딜레이 후 1차 읽기
                System.Threading.Thread.Sleep(30);

                var buf = new byte[512];
                int read = 0;

                try
                {
                    read += _sp.Read(buf, 0, buf.Length);
                }
                catch (TimeoutException)
                {
                    // 타임아웃은 조용히 무시
                }

                // 남은 데이터 추가로 읽기
                System.Threading.Thread.Sleep(30);
                try
                {
                    if (_sp.BytesToRead > 0)
                    {
                        int remain = Math.Min(buf.Length - read, _sp.BytesToRead);
                        read += _sp.Read(buf, read, remain);
                    }
                }
                catch (TimeoutException)
                {
                }

                var resp = new byte[read];
                Buffer.BlockCopy(buf, 0, resp, 0, read);
                return resp;
            }
        }
    }
}
