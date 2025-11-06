using System;
using System.Runtime.Intrinsics.Arm;

namespace ModbusTester.Modbus
{
    public static class ModbusRtu
    {
        /// <summary>
        /// 03h/04h 읽기 요청 프레임 생성: [slave][fc][startHi][startLo][cntHi][cntLo][crcLo][crcHi]
        /// </summary>
        public static byte[] BuildReadFrame(byte slave, FunctionCode fc, ushort start, ushort count)
        {
            if (fc != FunctionCode.ReadHoldingRegisters && fc != FunctionCode.ReadInputRegisters)
                throw new ArgumentException("Use 03h or 04h for read.");

            var frame = new byte[8];
            frame[0] = slave;
            frame[1] = (byte)fc;
            frame[2] = (byte)(start >> 8);
            frame[3] = (byte)(start & 0xFF);
            frame[4] = (byte)(count >> 8);
            frame[5] = (byte)(count & 0xFF);

            var crc = Crc16.Compute(frame, 6);
            Crc16.AppendLittleEndian(frame, 6, crc);
            return frame;
        }

        /// <summary>
        /// 06h 단일 쓰기: [slave][06][addrHi][addrLo][valHi][valLo][crcLo][crcHi]
        /// </summary>
        public static byte[] BuildWriteSingleFrame(byte slave, ushort address, ushort value)
        {
            var frame = new byte[8];
            frame[0] = slave;
            frame[1] = (byte)FunctionCode.WriteSingleRegister;
            frame[2] = (byte)(address >> 8);
            frame[3] = (byte)(address & 0xFF);
            frame[4] = (byte)(value >> 8);
            frame[5] = (byte)(value & 0xFF);

            var crc = Crc16.Compute(frame, 6);
            Crc16.AppendLittleEndian(frame, 6, crc);
            return frame;
        }

        /// <summary>
        /// 10h 다중 쓰기
        /// </summary>
        public static byte[] BuildWriteMultipleFrame(byte slave, ushort start, ushort[] values)
        {
            ushort count = (ushort)values.Length;
            int byteCount = count * 2;
            var frame = new byte[7 + 1 + byteCount + 2]; // header6 + byteCount1 + data + crc2

            frame[0] = slave;
            frame[1] = (byte)FunctionCode.WriteMultipleRegisters;
            frame[2] = (byte)(start >> 8);
            frame[3] = (byte)(start & 0xFF);
            frame[4] = (byte)(count >> 8);
            frame[5] = (byte)(count & 0xFF);
            frame[6] = (byte)byteCount;

            int p = 7;
            foreach (var v in values)
            {
                frame[p++] = (byte)(v >> 8);
                frame[p++] = (byte)(v & 0xFF);
            }

            var crc = Crc16.Compute(frame, frame.Length - 2);
            Crc16.AppendLittleEndian(frame, frame.Length - 2, crc);
            return frame;
        }
    }
}
