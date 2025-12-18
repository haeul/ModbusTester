using System.Collections.Generic;

namespace ModbusTester.Macros
{
    public class MacroDefinition
    {
        public string Name { get; set; } = "";
        public int Repeat { get; set; } = 1;
        public List<MacroStep> Steps { get; set; } = new List<MacroStep>();
    }

    public class MacroStep
    {
        public string PresetName { get; set; } = "";
        public int DelayMs { get; set; } = 0;
    }
}
