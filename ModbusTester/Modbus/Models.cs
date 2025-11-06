namespace ModbusTester.Modbus
{
    public enum FunctionCode : byte
    {
        ReadHoldingRegisters = 0x03,
        ReadInputRegisters = 0x04,
        WriteSingleRegister = 0x06,
        WriteMultipleRegisters = 0x10,
    }

    public record RtuRequest(byte Slave, FunctionCode Function, ushort Start, ushort CountOrValue, byte[]? Payload);
}
