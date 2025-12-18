using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace ModbusTester.Macros
{
    public static class MacroManager
    {
        private static readonly string FilePath =
            Path.Combine(Application.StartupPath, "Macros.json");

        private static List<MacroDefinition> _items = new List<MacroDefinition>();
        public static IReadOnlyList<MacroDefinition> Items => _items;

        public static void Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    _items = new List<MacroDefinition>();
                    return;
                }

                string json = File.ReadAllText(FilePath);
                _items = JsonSerializer.Deserialize<List<MacroDefinition>>(json)
                         ?? new List<MacroDefinition>();
            }
            catch
            {
                _items = new List<MacroDefinition>();
            }
        }

        public static void Save()
        {
            try
            {
                var json = JsonSerializer.Serialize(_items, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(FilePath, json);
            }
            catch { }
        }

        public static MacroDefinition? Find(string name)
        {
            return _items.FirstOrDefault(m => m.Name == name);
        }

        public static void AddOrUpdate(MacroDefinition macro)
        {
            var existing = _items.FirstOrDefault(m => m.Name == macro.Name);
            if (existing != null)
            {
                existing.Repeat = macro.Repeat;
                existing.Steps = macro.Steps;
            }
            else
            {
                _items.Add(macro);
            }

            Save();
        }

        public static void Delete(string name)
        {
            _items.RemoveAll(m => m.Name == name);
            Save();
        }

        public static string CreateUniqueName(string baseName = "Macro")
        {
            int i = 1;
            string name = $"{baseName}{i}";
            while (_items.Any(m => m.Name == name))
            {
                i++;
                name = $"{baseName}{i}";
            }
            return name;
        }
    }
}
