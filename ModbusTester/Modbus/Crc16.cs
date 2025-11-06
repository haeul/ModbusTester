namespace ModbusTester.Modbus
{
    // Modbus RTU CRC-16 (poly 0xA001, init 0xFFFF, little-endian output)
    public static class Crc16
    {
        public static ushort Compute(byte[] data, int length)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < length; i++)
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

        public static void AppendLittleEndian(byte[] buffer, int offset, ushort crc)
        {
            buffer[offset + 0] = (byte)(crc & 0xFF);       // CRC Lo
            buffer[offset + 1] = (byte)((crc >> 8) & 0xFF); // CRC Hi
        }
    }
}
