using System;
using System.IO.Ports;

namespace ModbusTester.Services
{
    public class SerialPortService : IDisposable
    {
        private readonly SerialPort _port;

        public event EventHandler<byte[]>? DataReceived;
        public bool IsOpen => _port.IsOpen;

        public SerialPortService(string portName, int baud = 38400, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One)
        {
            _port = new SerialPort(portName, baud, parity, dataBits, stopBits)
            {
                ReadTimeout = 500,
                WriteTimeout = 500
            };
            _port.DataReceived += OnDataReceived;
        }

        public void Open()
        {
            if (!_port.IsOpen) _port.Open();
        }

        public void Close()
        {
            if (_port.IsOpen) _port.Close();
        }

        public void Write(byte[] data) => _port.Write(data, 0, data.Length);

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int toRead = _port.BytesToRead;
                if (toRead <= 0) return;
                var buf = new byte[toRead];
                _port.Read(buf, 0, toRead);
                DataReceived?.Invoke(this, buf);
            }
            catch { /* swallow & surface via event if needed */ }
        }

        public void Dispose()
        {
            try { _port.DataReceived -= OnDataReceived; } catch { }
            try { if (_port.IsOpen) _port.Close(); } catch { }
            _port.Dispose();
        }
    }
}
