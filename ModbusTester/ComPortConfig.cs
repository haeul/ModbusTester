// ComPortConfig.cs
using System.IO.Ports;

namespace ModbusTester
{
    public class ComPortConfig
    {
        public string PortName { get; set; } = "";
        public int BaudRate { get; set; } = 115200;
        public Parity Parity { get; set; } = Parity.None;
        public int DataBits { get; set; } = 8;
        public StopBits StopBits { get; set; } = StopBits.One;

        public bool SlaveMode { get; set; } = false;
    }
}
