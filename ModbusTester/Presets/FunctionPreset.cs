using System;
using System.Collections.Generic;

namespace ModbusTester.Presets
{
    public class TxRowPreset
    {
        public ushort Address { get; set; }      // 절대 주소
        public string Name { get; set; } = "";   // Name 컬럼
        public ushort? Value { get; set; }       // 값 있으면 저장
    }

    public class FunctionPreset
    {
        public string Name { get; set; } = "";
        public byte SlaveId { get; set; }
        public byte FunctionCode { get; set; }
        public ushort StartAddress { get; set; }
        public ushort RegisterCount { get; set; }
        public ushort TxGridStartAddress { get; set; }
        public ushort RxGridStartAddress { get; set; }


        // 채워진 TX 행만 저장
        public List<TxRowPreset> TxRows { get; set; } = new List<TxRowPreset>();

        public override string ToString()
        {
            return $"{Name} (ID={SlaveId}, FC={FunctionCode:X2}h, Start={StartAddress:X4}h, Count={RegisterCount})";
        }
    }
}
