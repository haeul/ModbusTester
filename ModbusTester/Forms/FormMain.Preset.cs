using ModbusTester.Presets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormMain
    {
        private void ApplyPresetTxRows(FunctionPreset preset)
        {
            if (preset.TxRows == null || preset.TxRows.Count == 0)
                return;

            var map = preset.TxRows.ToDictionary(x => x.Address);

            ushort start = preset.StartAddress;
            int count = preset.RegisterCount;

            for (int i = 0; i < count && i < gridTx.Rows.Count; i++)
            {
                ushort addr = (ushort)(start + i);
                if (!map.TryGetValue(addr, out var saved))
                    continue;

                var row = gridTx.Rows[i];
                if (row.IsNewRow) continue;

                // Name
                row.Cells[COL_NAME].Value = saved.Name ?? "";

                // Value가 있으면 DEC/HEX/BIT를 재계산해서 채움
                if (saved.Value.HasValue)
                {
                    ushort v = saved.Value.Value;
                    row.Cells[COL_DEC].Value = v.ToString();
                    row.Cells[COL_HEX].Value = v.ToString("X4") + "h";
                    row.Cells[COL_BIT].Value = Convert.ToString(v, 2).PadLeft(16, '0');
                }
            }

            // 프리셋 적용은 대량 변경이므로 마지막에 1회 전체 동기화가 가장 깔끔/확실
            _gridController.SyncAllTxNamesToRx();
        }

        private ushort? TryReadUShortFromCells(DataGridViewRow row)
        {
            string decText = Convert.ToString(row.Cells[COL_DEC].Value) ?? "";
            if (ushort.TryParse(decText.Trim(), out ushort dec))
                return dec;

            string hexText = Convert.ToString(row.Cells[COL_HEX].Value) ?? "";
            hexText = hexText.Trim()
                             .Replace("0x", "", StringComparison.OrdinalIgnoreCase)
                             .Replace("h", "", StringComparison.OrdinalIgnoreCase);

            if (ushort.TryParse(hexText, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out ushort hex))
                return hex;

            return null;
        }

        private List<TxRowPreset> CaptureTxRows(ushort start, int count)
        {
            var list = new List<TxRowPreset>();

            for (int i = 0; i < count && i < gridTx.Rows.Count; i++)
            {
                var row = gridTx.Rows[i];
                if (row.IsNewRow) continue;

                string name = Convert.ToString(row.Cells[COL_NAME].Value) ?? "";
                name = name.Trim();

                ushort? value = TryReadUShortFromCells(row);

                bool hasName = !string.IsNullOrWhiteSpace(name);
                bool hasValue = value.HasValue;

                if (!hasName && !hasValue)
                    continue; // 완전 빈 행은 저장 안 함

                list.Add(new TxRowPreset
                {
                    Address = (ushort)(start + i),
                    Name = name,
                    Value = value
                });
            }

            return list;
        }

        private void RefreshPresetCombo()
        {
            cmbPreset.Items.Clear();

            foreach (var p in FunctionPresetManager.Items)
                cmbPreset.Items.Add(p);

            cmbPreset.SelectedIndex = -1;
        }

        private FunctionPreset? CreatePresetFromUi(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            try
            {
                byte slaveId = (byte)numSlave.Value;

                byte fc = 0x03;
                if (cmbFunctionCode.SelectedItem is string s && s.Length >= 2)
                {
                    string hexPart = s.Substring(0, 2);
                    fc = Convert.ToByte(hexPart, 16);
                }

                ushort startAddr = (ushort)numStartRegister.Value;
                ushort regCount = (ushort)numCount.Value;

                return new FunctionPreset
                {
                    Name = name.Trim(),
                    SlaveId = slaveId,
                    FunctionCode = fc,
                    StartAddress = startAddr,
                    RegisterCount = regCount,

                    TxGridStartAddress = _gridController.TxStartAddress, 
                    RxGridStartAddress = _gridController.RxStartAddress, 

                    TxRows = CaptureTxRows(startAddr, regCount)
                };
            }
            catch
            {
                MessageBox.Show(this,
                    "현재 설정 값에서 프리셋을 만들 수 없습니다.\r\n" +
                    "Slave, Function Code, Start Register, Register Count를 확인해 주세요.",
                    "Preset 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return null;
            }
        }

        private void ApplyPresetToUi(FunctionPreset preset)
        {
            numSlave.Value = Math.Max(numSlave.Minimum,
                                Math.Min(numSlave.Maximum, preset.SlaveId));

            string key = preset.FunctionCode.ToString("X2");
            int foundIndex = -1;
            for (int i = 0; i < cmbFunctionCode.Items.Count; i++)
            {
                if (cmbFunctionCode.Items[i] is string s &&
                    s.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                {
                    foundIndex = i;
                    break;
                }
            }
            if (foundIndex >= 0)
                cmbFunctionCode.SelectedIndex = foundIndex;

            ushort gridStart = preset.TxGridStartAddress;
            if (gridStart == 0 && preset.RxGridStartAddress != 0)
                gridStart = preset.RxGridStartAddress;
            if (gridStart == 0 && preset.StartAddress != 0)
                gridStart = preset.StartAddress;

            if (preset.RegisterCount >= numCount.Minimum &&
                preset.RegisterCount <= numCount.Maximum)
            {
                numCount.Value = preset.RegisterCount;
            }
            else
            {
                numCount.Value = numCount.Minimum;
            }

            // Grid 시작 주소 복원 (TX/RX 동기화 포함)
            _gridController.SetStartAddressBoth(gridStart);

            _gridController.ClearTxAll();     // (이 안에서 TX->RX Name 1회 동기화도 수행)
            ApplyPresetTxRows(preset);        // 프리셋 복원 (마지막에 다시 1회 전체 동기화)
        }

        private string PromptForPresetName(string? defaultName = null)
        {
            using (var dlg = new Form())
            using (var txt = new TextBox())
            using (var ok = new Button())
            using (var cancel = new Button())
            {
                dlg.Text = "프리셋 이름 입력";
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MinimizeBox = false;
                dlg.MaximizeBox = false;
                dlg.ClientSize = new Size(300, 120);

                txt.Left = 10;
                txt.Top = 10;
                txt.Width = dlg.ClientSize.Width - 20;
                txt.Text = defaultName ?? "";

                ok.Text = "OK";
                ok.DialogResult = DialogResult.OK;
                ok.Width = 80;
                ok.Left = dlg.ClientSize.Width - 180;
                ok.Top = 50;

                cancel.Text = "Cancel";
                cancel.DialogResult = DialogResult.Cancel;
                cancel.Width = 80;
                cancel.Left = dlg.ClientSize.Width - 90;
                cancel.Top = 50;

                dlg.Controls.Add(txt);
                dlg.Controls.Add(ok);
                dlg.Controls.Add(cancel);
                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                var result = dlg.ShowDialog(this);
                if (result == DialogResult.OK)
                    return txt.Text.Trim();

                return "";
            }
        }

        // Preset 실행은 PresetRunner로 위임
        public bool TryRunPresetByName(string presetName, out string error)
        {
            error = "";

            if (_presetRunner == null)
            {
                error = "PresetRunner가 초기화되지 않았습니다.";
                return false;
            }

            return _presetRunner.TryRunPresetByName(presetName, _slaveMode, _isOpen, out error);
        }

        private void btnPresetSave_Click(object sender, EventArgs e)
        {
            string defaultName = (cmbPreset.SelectedItem as FunctionPreset)?.Name ?? "";

            string name = PromptForPresetName(defaultName);
            if (string.IsNullOrWhiteSpace(name))
                return;

            var preset = CreatePresetFromUi(name);
            if (preset == null)
                return;

            FunctionPresetManager.AddOrUpdate(preset);
            RefreshPresetCombo();

            foreach (var item in cmbPreset.Items)
            {
                if (item is FunctionPreset fp && fp.Name == preset.Name)
                {
                    cmbPreset.SelectedItem = fp;
                    break;
                }
            }
        }

        private void btnPresetDelete_Click(object sender, EventArgs e)
        {
            if (cmbPreset.SelectedItem is not FunctionPreset preset)
            {
                MessageBox.Show(this, "삭제할 프리셋을 먼저 선택해 주세요.",
                    "Preset", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var result = MessageBox.Show(this,
                $"'{preset.Name}' 프리셋을 삭제할까요?",
                "Preset 삭제",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            FunctionPresetManager.Delete(preset.Name);
            RefreshPresetCombo();
        }

        private void cmbPreset_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbPreset.SelectedItem is not FunctionPreset preset)
                return;

            ApplyPresetToUi(preset);
        }
    }
}
