using System;
using System.Runtime.Intrinsics.Arm;

namespace ModbusTester.Modbus
{
    public static class ModbusRtu
    {
        /// 03h/04h 읽기 요청 프레임 생성: [slave][fc][startHi][startLo][cntHi][cntLo][crcLo][crcHi]
        public static byte[] BuildReadFrame(byte slave, FunctionCode fc, ushort start, ushort count)
        {
            if (fc != FunctionCode.ReadHoldingRegisters && fc != FunctionCode.ReadInputRegisters)
                throw new ArgumentException("Use 03h or 04h for read.");

            var frame = new byte[8];
            frame[0] = slave;                // 슬레이브 주소
            frame[1] = (byte)fc;             // 0x03 또는 0x04
            frame[2] = (byte)(start >> 8);   // 시작 주소 상위바이트
            frame[3] = (byte)(start & 0xFF); // 시작 주소 하위바이트
            frame[4] = (byte)(count >> 8);   // 레지스터 개수 상위바이트
            frame[5] = (byte)(count & 0xFF); // 레지스터 개수 하위바이트

            var crc = Crc16.Compute(frame, 6);                 // 앞 6바이트에 대해 CRC 계산
            Crc16.AppendLittleEndian(frame, 6, crc);           // frame[6]=crcLo, frame[7]=crcHi 로 기록
            return frame;
        }


        /// 06h 단일 쓰기: [slave][06][addrHi][addrLo][valHi][valLo][crcLo][crcHi]
        public static byte[] BuildWriteSingleFrame(byte slave, ushort address, ushort value)
        {
            var frame = new byte[8];
            frame[0] = slave;
            frame[1] = (byte)FunctionCode.WriteSingleRegister; // 0x06
            frame[2] = (byte)(address >> 8);                   // 주소 Hi
            frame[3] = (byte)(address & 0xFF);                 // 주소 Lo
            frame[4] = (byte)(value >> 8);                     // 값 Hi
            frame[5] = (byte)(value & 0xFF);                   // 값 Lo

            var crc = Crc16.Compute(frame, 6);
            Crc16.AppendLittleEndian(frame, 6, crc);
            return frame;
        }


        /// 10h 다중 쓰기
        public static byte[] BuildWriteMultipleFrame(byte slave, ushort start, ushort[] values)
        {
            ushort count = (ushort)values.Length;       // 레지스터 개수
            int byteCount = count * 2;                  // 실제 데이터 바이트 수
            var frame = new byte[7 + 1 + byteCount + 2]; // header6 + byteCount1 + data + crc2

            frame[0] = slave;
            frame[1] = (byte)FunctionCode.WriteMultipleRegisters; // 0x10
            frame[2] = (byte)(start >> 8);                        // 시작 주소 Hi
            frame[3] = (byte)(start & 0xFF);                      // 시작 주소 Lo
            frame[4] = (byte)(count >> 8);                        // 개수 Hi
            frame[5] = (byte)(count & 0xFF);                      // 개수 Lo
            frame[6] = (byte)byteCount;                           // 데이터 바이트 수

            int p = 7;                                            // 데이터 시작 인덱스
            foreach (var v in values)
            {
                frame[p++] = (byte)(v >> 8);                      // 값 Hi
                frame[p++] = (byte)(v & 0xFF);                    // 값 Lo
            }

            var crc = Crc16.Compute(frame, frame.Length - 2);     // CRC는 마지막 두 바이트를 제외하고 계산
            Crc16.AppendLittleEndian(frame, frame.Length - 2, crc); // 끝 두 바이트에 crcLo/Hi
            return frame;
        }

    }
}
