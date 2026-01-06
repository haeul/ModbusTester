using ModbusTester.Modbus;
using ModbusTester.Utils;
using System;
using System.IO;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormMain
    {
        // ───────────────────── CRC 계산 버튼 ─────────────────────

        private void btnCalcCrc_Click(object sender, EventArgs e)
        {
            try
            {
                byte slave = (byte)numSlave.Value;
                ushort start = (ushort)numStartRegister.Value;
                ushort count = (ushort)numCount.Value;
                byte fc = GetFunctionCode();

                byte[] frame;

                if (fc == 0x03 || fc == 0x04)
                {
                    frame = ModbusRtu.BuildReadFrame(slave, fc, start, count);
                    txtDataCount.Text = "0";
                }
                else if (fc == 0x06)
                {
                    ushort val = _gridController.ReadTxValueOrZero(0);
                    frame = ModbusRtu.BuildWriteSingleFrame(slave, start, val);
                    txtDataCount.Text = "2";
                }
                else if (fc == 0x10)
                {
                    ushort[] vals = _gridController.ReadTxValues(count);
                    frame = ModbusRtu.BuildWriteMultipleFrame(slave, start, vals);
                    byte byteCount = (byte)(vals.Length * 2);
                    txtDataCount.Text = byteCount.ToString();
                }
                else
                {
                    throw new NotSupportedException("지원하지 않는 Function Code");
                }

                if (frame.Length < 4)
                    throw new InvalidOperationException("프레임 길이가 너무 짧습니다.");

                // RTU 프레임 끝 2바이트가 CRC (Low, High)
                byte crcLo = frame[^2];
                byte crcHi = frame[^1];

                txtCrc.Text = $"{crcHi:X2}{crcLo:X2}h";
                Log($"[CRC CALC] {frame.ToHex()}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("CRC 계산 실패: " + ex.Message);
            }
        }

        // ───────────────────── 수동 전송 버튼 ─────────────────────

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (_slaveMode)
            {
                MessageBox.Show("지금은 Slave 모드입니다. Master 모드에서 전송하세요.");
                return;
            }
            if (!_isOpen)
            {
                MessageBox.Show("먼저 포트를 OPEN 하세요.");
                return;
            }
            if (_master == null)
            {
                MessageBox.Show("통신 클라이언트가 초기화되지 않았습니다.");
                return;
            }

            try
            {
                byte slave = (byte)numSlave.Value;
                ushort start = (ushort)numStartRegister.Value;
                ushort count = (ushort)numCount.Value;
                byte fc = GetFunctionCode();

                if (fc == 0x03 || fc == 0x04)
                {
                    var result = _master.ReadRegisters(slave, (FunctionCode)fc, start, count);

                    Log("TX: " + result.Request.ToHex());
                    Log("RX: " + result.Response.ToHex());

                    UpdateReceiveHeader(result.Response, slave, fc, start, count);
                    _gridController.FillRxGrid(start, result.Values);

                    RegisterCache.UpdateRange(start, result.Values);
                }
                else if (fc == 0x06)
                {
                    ushort val = _gridController.ReadTxValueOrZero(0);
                    var result = _master.WriteSingleRegister(slave, start, val);

                    Log("TX: " + result.Request.ToHex());
                    Log("RX: " + result.Response.ToHex());

                    UpdateReceiveHeader(result.Response, slave, fc, start, 1);
                    RegisterCache.UpdateRange(start, new ushort[] { val });
                }
                else if (fc == 0x10)
                {
                    ushort[] vals = _gridController.ReadTxValues(count);
                    var result = _master.WriteMultipleRegisters(slave, start, vals);

                    Log("TX: " + result.Request.ToHex());
                    Log("RX: " + result.Response.ToHex());

                    UpdateReceiveHeader(result.Response, slave, fc, start, (ushort)vals.Length);
                    RegisterCache.UpdateRange(start, vals);
                }
                else
                {
                    throw new NotSupportedException("지원하지 않는 Function Code");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("전송 실패: " + ex.Message);
            }
        }

        // ───────────────────── TX/RX/Log 버튼들 ─────────────────────

        private void btnRevert_Click(object sender, EventArgs e)
        {
            _gridController.RevertTxSnapshot();

            // Revert는 TX Name도 바뀔 수 있으니 1회 전체 동기화
            _gridController.SyncAllTxNamesToRx();
        }

        private void btnTxClear_Click(object sender, EventArgs e)
        {
            // 닉네임 제외 값 영역이 다 비어있으면 Clear 막기 (스냅샷도 갱신 안 함)
            if (_gridController.IsTxValueAreaEmpty())
                return;

            _gridController.SaveTxSnapshot();
            _gridController.ClearTxValues();
        }

        private void btnRxClear_Click(object sender, EventArgs e)
        {
            _gridController.ClearRxValues();

            txtRxSlave.Clear();
            txtRxFc.Clear();
            txtRxStart.Clear();
            txtRxCount.Clear();
            txtRxDataCount.Clear();
            txtRxCrc.Clear();
        }

        private void btnCopyToTx_Click(object sender, EventArgs e)
        {
            _gridController.CopyRxToTx();
        }

        private void btnLogClear_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }

        private void btnSaveLog_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog
            {
                Filter = "Text|*.txt",
                FileName = $"modbus_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };
            if (sfd.ShowDialog(this) == DialogResult.OK)
                File.WriteAllText(sfd.FileName, txtLog.Text);
        }

        // ───────────────────── 종료 처리 ─────────────────────

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            try { _recService.Dispose(); } catch { }
            try { _slave?.Dispose(); } catch { }

            try
            {
                if (_sp != null)
                {
                    if (_sp.IsOpen)
                        _sp.Close();

                    _sp.Dispose();
                }
            }
            catch { }
        }
    }
}
