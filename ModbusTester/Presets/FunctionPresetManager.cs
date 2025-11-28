using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace ModbusTester.Presets
{
    public static class FunctionPresetManager
    {
        private static readonly string FilePath =
            Path.Combine(Application.StartupPath, "FunctionPresets.json");

        private static List<FunctionPreset> _items = new List<FunctionPreset>();
        public static IReadOnlyList<FunctionPreset> Items => _items;

        public static void Load()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    _items = new List<FunctionPreset>();
                    return;
                }

                string json = File.ReadAllText(FilePath);
                _items = JsonSerializer.Deserialize<List<FunctionPreset>>(json)
                         ?? new List<FunctionPreset>();
            }
            catch
            {
                _items = new List<FunctionPreset>();
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
            catch
            {
            }
        }

        public static void AddOrUpdate(FunctionPreset preset)
        {
            var existing = _items.Find(p => p.Name == preset.Name);
            if (existing != null)
            {
                existing.SlaveId = preset.SlaveId;
                existing.FunctionCode = preset.FunctionCode;
                existing.StartAddress = preset.StartAddress;
                existing.RegisterCount = preset.RegisterCount;
            }
            else
            {
                _items.Add(preset);
            }

            Save();
        }

        public static void Delete(string name)
        {
            _items.RemoveAll(p => p.Name == name);
            Save();
        }
    }
}
