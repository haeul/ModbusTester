using ModbusTester.Modbus;

namespace ModbusTester.Services
{
    public interface IModbusMasterService
    {
        ModbusMasterService.ModbusReadResult ReadRegisters(byte slave, FunctionCode fc, ushort start, ushort count);
        ModbusMasterService.ModbusWriteResult WriteSingleRegister(byte slave, ushort addr, ushort value);
        ModbusMasterService.ModbusWriteResult WriteMultipleRegisters(byte slave, ushort start, ushort[] values);
    }
}
