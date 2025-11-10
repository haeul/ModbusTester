using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace ModbusTester
{
    /// <summary>
    /// 간단한 RTU 슬레이브 에뮬레이터 (포트를 직접 열어 응답)
    /// - FC: 0x03(Read Holding), 0x04(Read Input), 0x06(Write Single), 0x10(Write Multiple)
    /// - Holding/Input 레지스터 각각 0~65535
    /// </summary>
    public sealed class ModbusSlave : IDisposable
    {
        private readonly SerialPort _sp = new SerialPort();
        private Thread _loop;
        private volatile bool _run;

        public byte SlaveId { get; private set; } = 1;

        private readonly ushort[] _hr = new ushort[65536]; // Holding Registers
        private readonly ushort[] _ir = new ushort[65536]; // Input Registers

        public bool IsOpen => _sp.IsOpen;

        public void InitDemoData()
        {
            for (int i = 0; i < 256; i++)
            {
                _hr[i] = (ushort)(0x1000 + i);
                _ir[i] = (ushort)(0x2000 + i);
            }
        }

        public void Open(string portName, int baud, Parity parity, int dataBits, StopBits stopBits, byte slaveId)
        {
            if (_sp.IsOpen) Close();

            SlaveId = slaveId;

            _sp.PortName = portName;
            _sp.BaudRate = baud;
            _sp.Parity = parity;
            _sp.DataBits = dataBits;
            _sp.StopBits = stopBits;
            _sp.ReadTimeout = 100;
            _sp.WriteTimeout = 100;

            _sp.Open();

            _run = true;
            _loop = new Thread(Worker) { IsBackground = true, Name = "ModbusSlaveLoop" };
            _loop.Start();
        }

        public void Close()
        {
            _run = false;
            try { _loop?.Join(300); } catch { }
            _loop = null;

            if (_sp.IsOpen) _sp.Close();
        }

        public void Dispose() => Close();

        // ───────────────────────── Core Loop ─────────────────────────
        private void Worker()
        {
            byte[] buf = new byte[1024];
            int len = 0;

            while (_run)
            {
                try
                {
                    int toRead = Math.Min(_sp.BytesToRead, buf.Length - len);
                    if (toRead > 0)
                    {
                        int got = _sp.Read(buf, len, toRead);
                        len += got;
                    }
                    else
                    {
                        Thread.Sleep(5);
                    }

                    while (true)
                    {
                        int used = TryParseAndRespond(buf, len);
                        if (used > 0)
                        {
                            Array.Copy(buf, used, buf, 0, len - used);
                            len -= used;
                        }
                        else break;
                    }
                }
                catch (TimeoutException) { /* ignore */ }
                catch
                {
                    len = 0; // 문제시 버퍼 리셋
                }
            }
        }

        /// <summary>버퍼 앞부분의 1프레임을 처리하고 응답했으면 소비 바이트 수 반환(없으면 0)</summary>
        private int TryParseAndRespond(byte[] buf, int len)
        {
            if (len < 4) return 0;

            byte addr = buf[0];
            if (addr != SlaveId) return 1; // 다른 주소면 한 바이트 버려 재동기화

            byte fc = buf[1];

            if (fc == 0x03 || fc == 0x04 || fc == 0x06)
            {
                if (len < 8) return 0; // 고정 8바이트
                int frameLen = 8;
                if (!CheckCrc(buf, frameLen)) return 1;

                ushort p1 = (ushort)((buf[2] << 8) | buf[3]);
                ushort p2 = (ushort)((buf[4] << 8) | buf[5]);

                if (fc == 0x03 || fc == 0x04)
                {
                    ushort start = p1;
                    ushort count = p2;
                    if (count == 0 || start + count > 65536) return frameLen;

                    ushort[] src = (fc == 0x03) ? _hr : _ir;

                    int byteCount = count * 2;
                    byte[] resp = new byte[3 + byteCount];
                    resp[0] = addr;
                    resp[1] = fc;
                    resp[2] = (byte)byteCount;

                    for (int i = 0; i < count; i++)
                    {
                        ushort v = src[start + i];
                        resp[3 + i * 2] = (byte)(v >> 8);
                        resp[4 + i * 2] = (byte)(v & 0xFF);
                    }

                    var withCrc = WithCrc(resp);
                    _sp.Write(withCrc, 0, withCrc.Length);
                }
                else // 0x06
                {
                    ushort regAddr = p1;
                    ushort value = p2;
                    if (regAddr < 65536)
                        _hr[regAddr] = value;

                    byte[] echo = buf.Take(frameLen - 2).ToArray(); // CRC 제외
                    var withCrc = WithCrc(echo);
                    _sp.Write(withCrc, 0, withCrc.Length);
                }

                return frameLen;
            }
            else if (fc == 0x10)
            {
                if (len < 9) return 0;

                ushort start = (ushort)((buf[2] << 8) | buf[3]);
                ushort count = (ushort)((buf[4] << 8) | buf[5]);
                byte byteCount = buf[6];

                int frameLen = 7 + byteCount + 2;
                if (len < frameLen) return 0;
                if (!CheckCrc(buf, frameLen)) return 1;

                if (count == 0 || count * 2 != byteCount || start + count > 65536)
                    return frameLen;

                for (int i = 0; i < count; i++)
                {
                    int idx = 7 + i * 2;
                    ushort v = (ushort)((buf[idx] << 8) | buf[idx + 1]);
                    _hr[start + i] = v;
                }

                byte[] resp = new byte[6];
                resp[0] = addr;
                resp[1] = 0x10;
                resp[2] = (byte)(start >> 8);
                resp[3] = (byte)(start & 0xFF);
                resp[4] = (byte)(count >> 8);
                resp[5] = (byte)(count & 0xFF);

                var withCrc2 = WithCrc(resp);
                _sp.Write(withCrc2, 0, withCrc2.Length);

                return frameLen;
            }
            else
            {
                // 미지원 FC → 예외응답(ILLEGAL FUNCTION=0x01)
                if (len < 8) return 1;

                byte[] ex = new byte[] { addr, (byte)(fc | 0x80), 0x01 };
                var withCrc = WithCrc(ex);
                _sp.Write(withCrc, 0, withCrc.Length);
                return 8;
            }
        }

        // ───────────────────────── Helpers ─────────────────────────
        private static bool CheckCrc(byte[] frame, int len)
        {
            if (len < 3) return false;
            ushort calc = Crc16(frame, len - 2);
            ushort got = (ushort)(frame[len - 2] | (frame[len - 1] << 8));
            return calc == got;
        }

        private static byte[] WithCrc(byte[] frame)
        {
            ushort crc = Crc16(frame, frame.Length);
            byte[] arr = new byte[frame.Length + 2];
            Buffer.BlockCopy(frame, 0, arr, 0, frame.Length);
            arr[^2] = (byte)(crc & 0xFF);         // Lo
            arr[^1] = (byte)((crc >> 8) & 0xFF);  // Hi
            return arr;
        }

        private static ushort Crc16(byte[] data, int len)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < len; i++)
            {
                crc ^= data[i];
                for (int b = 0; b < 8; b++)
                {
                    bool lsb = (crc & 1) != 0;
                    crc >>= 1;
                    if (lsb) crc ^= 0xA001;
                }
            }
            return crc;
        }
    }
}
