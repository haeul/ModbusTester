using ModbusTester.Core;
using ModbusTester.Modbus;
using ModbusTester.Services;
using ModbusTester.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModbusTester.Presets
{
    /// <summary>
    /// Preset 실행(UseCase) 담당
    /// - FormMain에서 분리한 "프리셋 이름으로 실행" 로직
    /// - UI 갱신은 콜백(Action)으로 위임
    /// </summary>
    public sealed class PresetRunner
    {
        private readonly IModbusMasterService _master;
        private readonly RegisterGridController _gridController;

        private readonly Action<string> _log;
        private readonly Action<byte[], byte, byte, ushort, ushort> _updateReceiveHeader;
        private readonly Action<ushort, ushort[]> _updateCache;

        public PresetRunner(
            IModbusMasterService master,
            RegisterGridController gridController,
            Action<string> log,
            Action<byte[], byte, byte, ushort, ushort> updateReceiveHeader,
            Action<ushort, ushort[]> updateCache)
        {
            _master = master ?? throw new ArgumentNullException(nameof(master));
            _gridController = gridController ?? throw new ArgumentNullException(nameof(gridController));
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _updateReceiveHeader = updateReceiveHeader ?? throw new ArgumentNullException(nameof(updateReceiveHeader));
            _updateCache = updateCache ?? throw new ArgumentNullException(nameof(updateCache));
        }

        public bool TryRunPresetByName(string presetName, bool slaveMode, bool isOpen, out string error)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(presetName))
            {
                error = "Preset 이름이 비어 있습니다.";
                return false;
            }

            if (slaveMode)
            {
                error = "Slave 모드에서는 실행할 수 없습니다.";
                return false;
            }

            if (!isOpen)
            {
                error = "포트가 OPEN 상태가 아닙니다.";
                return false;
            }

            // 1) Preset 찾기
            var preset = FunctionPresetManager.Items
                .FirstOrDefault(p => string.Equals(p.Name, presetName, StringComparison.OrdinalIgnoreCase));

            if (preset == null)
            {
                error = $"Preset을 찾을 수 없습니다: {presetName}";
                return false;
            }

            try
            {
                byte slave = preset.SlaveId;
                ushort start = preset.StartAddress;
                ushort count = preset.RegisterCount;
                byte fc = preset.FunctionCode;

                if (!(fc == 0x03 || fc == 0x04 || fc == 0x06 || fc == 0x10))
                {
                    error = $"지원하지 않는 Function Code: 0x{fc:X2}";
                    return false;
                }

                if (count == 0)
                    count = (ushort)Math.Max(1, preset.TxRows?.Count ?? 1);

                if (fc == 0x03 || fc == 0x04)
                {
                    var result = _master.ReadRegisters(slave, (FunctionCode)fc, start, count);

                    _log("TX: " + result.Request.ToHex());
                    _log("RX: " + result.Response.ToHex());

                    _updateReceiveHeader(result.Response, slave, fc, start, count);
                    _gridController.FillRxGrid(start, result.Values);
                    _updateCache(start, result.Values);
                }
                else if (fc == 0x06)
                {
                    ushort val = ReadPresetValue(preset, start);
                    var result = _master.WriteSingleRegister(slave, start, val);

                    _log("TX: " + result.Request.ToHex());
                    _log("RX: " + result.Response.ToHex());

                    _updateReceiveHeader(result.Response, slave, fc, start, 1);
                    _updateCache(start, new ushort[] { val });
                }
                else if (fc == 0x10)
                {
                    ushort[] vals = ReadPresetValues(preset, count);
                    var result = _master.WriteMultipleRegisters(slave, start, vals);

                    _log("TX: " + result.Request.ToHex());
                    _log("RX: " + result.Response.ToHex());

                    _updateReceiveHeader(result.Response, slave, fc, start, (ushort)vals.Length);
                    _updateCache(start, vals);
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static ushort ReadPresetValue(FunctionPreset preset, ushort address)
        {
            var found = preset.TxRows?.FirstOrDefault(r => r.Address == address);
            if (found != null && found.Value.HasValue)
                return found.Value.Value;

            return 0;
        }

        private static ushort[] ReadPresetValues(FunctionPreset preset, ushort count)
        {
            ushort start = preset.StartAddress;
            var arr = new ushort[count];

            if (preset.TxRows != null && preset.TxRows.Count > 0)
            {
                var map = preset.TxRows.ToDictionary(r => r.Address, r => r.Value);
                for (int i = 0; i < count; i++)
                {
                    ushort addr = (ushort)(start + i);
                    if (map.TryGetValue(addr, out var value) && value.HasValue)
                        arr[i] = value.Value;
                    else
                        arr[i] = 0;
                }
            }

            return arr;
        }
    }
}
