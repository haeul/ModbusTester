using System;

namespace ModbusTester
{
    // 프리셋 1개에 대한 정보
    public class FunctionPreset
    {
        public string Name { get; set; } = "";    // 프리셋 이름 (콤보박스에 보이는 이름)

        public byte SlaveId { get; set; }         // 슬레이브 주소
        public byte FunctionCode { get; set; }    // FC (0x03, 0x04, 0x06, 0x10 등)
        public ushort StartAddress { get; set; }  // 시작 레지스터 주소
        public ushort RegisterCount { get; set; } // 레지스터 개수

        // 콤보박스에 표시될 문자열
        public override string ToString()
        {
            return $"{Name} (ID={SlaveId}, FC={FunctionCode:X2}h, Start={StartAddress:X4}h, Count={RegisterCount})";
        }
    }
}
