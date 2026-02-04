using ModbusTester.Modbus;
using ModbusTester.Utils;
using NModbus;                 // TCP 분기에서 사용 (IModbusMaster)
using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;       // TcpClient Dispose
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormMain
    {
        private void ApplyCommModeUiState()
        {
            if (_tcpMode)
            {
                numSlave.Enabled = false;
                numSlave.Value = _tcpUnitId;

                lblTxSlaveAddress.Text = "Unit ID"; // Label 이름이 lblSlave라고 가정
            }
            else
            {
                numSlave.Enabled = true;

                lblTxSlaveAddress.Text = "Slave ID";
            }
        }

        // ───────────────────── CRC 계산 버튼 ─────────────────────

        private void btnCalcCrc_Click(object sender, EventArgs e)
        {
            try
            {
                // TCP 모드에서는 RTU CRC 자체가 의미 없음 (MBAP 헤더 사용)
                if (_tcpMode)
                {
                    MessageBox.Show("Modbus TCP 모드에서는 CRC를 사용하지 않습니다.");
                    return;
                }

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

            try
            {
                byte slave = (byte)numSlave.Value;
                ushort start = (ushort)numStartRegister.Value;
                ushort count = (ushort)numCount.Value;
                byte fc = GetFunctionCode();

                // ===== TCP Mode =====
                if (_tcpMode)
                {
                    if (_tcpMaster == null)
                    {
                        MessageBox.Show("TCP Master가 초기화되지 않았습니다.");
                        return;
                    }

                    byte unitId = _tcpUnitId != 0 ? _tcpUnitId : slave;

                    if (fc == 0x03)
                    {
                        ushort[] values = _tcpMaster.ReadHoldingRegisters(unitId, start, count);

                        Log($"TX: FC={fc:X2} (TCP) Unit={unitId}, Start=0x{start:X4}, Count={count}");
                        Log($"RX: OK (TCP) DataCount={values.Length * 2}, Regs={values.Length}");

                        UpdateReceiveHeaderTcp(unitId, fc, start, count, values.Length);
                        _gridController.FillRxGrid(start, values);
                        RegisterCache.UpdateRange(start, values);
                    }
                    else if (fc == 0x04)
                    {
                        ushort[] values = _tcpMaster.ReadInputRegisters(unitId, start, count);

                        Log($"TX: FC={fc:X2} (TCP) Unit={unitId}, Start=0x{start:X4}, Count={count}");
                        Log($"RX: OK (TCP) DataCount={values.Length * 2}, Regs={values.Length}");

                        UpdateReceiveHeaderTcp(unitId, fc, start, count, values.Length);
                        _gridController.FillRxGrid(start, values);
                        RegisterCache.UpdateRange(start, values);
                    }
                    else if (fc == 0x06)
                    {
                        ushort val = _gridController.ReadTxValueOrZero(0);
                        _tcpMaster.WriteSingleRegister(unitId, start, val);

                        Log($"TX: FC=06 (TCP) Unit={unitId}, Addr=0x{start:X4}, Value=0x{val:X4}");
                        Log("RX: OK (TCP)");
                        UpdateReceiveHeaderTcp(unitId, fc, start, 1, 0);
                        RegisterCache.UpdateRange(start, new ushort[] { val });
                    }
                    else if (fc == 0x10)
                    {
                        ushort[] vals = _gridController.ReadTxValues(count);
                        _tcpMaster.WriteMultipleRegisters(unitId, start, vals);

                        Log($"TX: FC=10 (TCP) Unit={unitId}, Start=0x{start:X4}, Count={vals.Length}, ByteCount={vals.Length * 2}");
                        Log("RX: OK (TCP)");
                        UpdateReceiveHeaderTcp(unitId, fc, start, (ushort)vals.Length, 0);
                        RegisterCache.UpdateRange(start, vals);
                    }
                    else
                    {
                        throw new NotSupportedException("지원하지 않는 Function Code");
                    }

                    // 성공 훅 (TCP)
                    NotifyCommSuccess();
                    return;
                }

                // ===== RTU Mode (기존 로직 그대로) =====
                if (_master == null)
                {
                    MessageBox.Show("통신 클라이언트가 초기화되지 않았습니다.");
                    return;
                }

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

                // 성공 훅 (RTU)
                NotifyCommSuccess();
            }
            catch (Exception ex)
            {
                // 실패 훅 (RTU/TCP 공통)
                NotifyCommFailure(ex);

                MessageBox.Show("전송 실패: " + ex.Message);
            }
        }

        private void UpdateReceiveHeaderTcp(byte unitId, byte fc, ushort start, ushort count, int valueCount)
        {
            txtRxSlave.Text = unitId.ToString();
            txtRxFc.Text = $"{fc:X2}h";
            txtRxStart.Text = $"{start:X4}h";
            txtRxCount.Text = count.ToString();

            if (fc == 0x03 || fc == 0x04)
                txtRxDataCount.Text = (valueCount * 2).ToString();
            else
                txtRxDataCount.Text = "0";

            txtRxCrc.Text = "";
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

            // RTU 포트 정리
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

            // TCP 정리 (Main이 들고 있으면 여기서도 안전하게 정리)
            try
            {
                if (_tcpClient != null)
                {
                    try { _tcpClient.Close(); } catch { }
                    try { _tcpClient.Dispose(); } catch { }
                }
            }
            catch { }
        }
    }
}
