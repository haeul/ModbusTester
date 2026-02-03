using ModbusTester.Services;
using ModbusTester.Utils;
using System;
using System.Linq;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormMain
    {
        private struct PollingConfig
        {
            public byte Slave;         // RTU: Slave ID, TCP: Unit ID로 사용
            public byte FunctionCode;  // 0x03 / 0x04만
            public ushort Start;
            public ushort Count;
        }

        private bool _pollingBusy = false;

        private int _pollFailCount = 0;
        private DateTime _lastPollErrLogAt = DateTime.MinValue;
        private string _lastPollErr = "";

        private void btnPollStart_Click(object sender, EventArgs e)
        {
            if (_slaveMode)
            {
                MessageBox.Show("Slave 모드에서는 폴링을 사용할 수 없습니다.");
                return;
            }

            if (!_isOpen)
            {
                MessageBox.Show("먼저 OPEN 하세요.");
                return;
            }

            byte fc = GetFunctionCode();
            if (!(fc == 0x03 || fc == 0x04))
            {
                MessageBox.Show("폴링은 Function Code 03/04(Read)만 지원합니다.");
                return;
            }

            // TCP 모드면 UnitId를 사용, RTU 모드면 numSlave 사용
            byte slaveOrUnit = _tcpMode ? _tcpUnitId : (byte)numSlave.Value;

            _pollingConfig = new PollingConfig
            {
                Slave = slaveOrUnit,
                Start = (ushort)numStartRegister.Value,
                Count = (ushort)numCount.Value,
                FunctionCode = fc
            };

            pollTimer.Interval = (int)numInterval.Value;
            pollTimer.Start();

            UpdateRecordingState();
        }

        private void btnPollStop_Click(object sender, EventArgs e)
        {
            pollTimer.Stop();
            UpdateRecordingState();
        }

        private void pollTimer_Tick(object sender, EventArgs e)
        {
            if (_pollingBusy) return;   // 중복 Tick 방지
            _pollingBusy = true;

            try
            {
                if (!_isOpen) return;
                if (_pollingConfig == null) return;

                var cfg = _pollingConfig.Value;
                byte slaveOrUnit = cfg.Slave;
                ushort start = cfg.Start;
                ushort count = cfg.Count;
                byte fc = cfg.FunctionCode;

                // TCP Polling 분기
                if (_tcpMode)
                {
                    if (_tcpMaster == null) return;

                    ushort[] values;

                    if (fc == 0x03)
                    {
                        values = _tcpMaster.ReadHoldingRegisters(slaveOrUnit, start, count);
                    }
                    else // 0x04
                    {
                        values = _tcpMaster.ReadInputRegisters(slaveOrUnit, start, count);
                    }

                    // TCP는 내부 프레임을 hex로 뽑기 어렵기 때문에 텍스트 로그로 대체
                    Log($"TX: FC={fc:X2} (TCP) Unit={slaveOrUnit}, Start=0x{start:X4}, Count={count}");
                    Log($"RX: OK (TCP) DataCount={values.Length * 2}, Regs={values.Length}");

                    // UI 헤더 업데이트 (TCP는 CRC 없음)
                    txtRxSlave.Text = slaveOrUnit.ToString();
                    txtRxFc.Text = $"{fc:X2}h";
                    txtRxStart.Text = $"{start:X4}h";
                    txtRxCount.Text = count.ToString();
                    txtRxDataCount.Text = (values.Length * 2).ToString();
                    txtRxCrc.Text = "-";

                    _gridController.FillRxGrid(start, values);
                    RegisterCache.UpdateRange(start, values);
                    _recService.AppendSnapshotIfDue(DateTime.Now, values);
                    return;
                }

                // RTU Polling (기존 로직 그대로)
                if (_poller == null) return;

                var result = _poller.Poll(slaveOrUnit, fc, start, count);

                Log("TX: " + result.Request.ToHex());
                Log("RX: " + result.Response.ToHex());

                if (result.IsException)
                    return;

                UpdateReceiveHeader(result.Response, slaveOrUnit, fc, start, count);
                _gridController.FillRxGrid(start, result.Values);

                RegisterCache.UpdateRange(start, result.Values);
                _recService.AppendSnapshotIfDue(DateTime.Now, result.Values);
            }
            catch (Exception ex)
            {
                _pollFailCount++;
                _lastPollErr = ex.GetType().Name + ": " + ex.Message;

                // 1초에 1번만 로그 찍기(스팸 방지)
                var now = DateTime.Now;
                if ((now - _lastPollErrLogAt).TotalMilliseconds >= 1000)
                {
                    _lastPollErrLogAt = now;
                    Log($"[POLL ERR] fail={_pollFailCount}, {_lastPollErr}");
                }

                // (선택) 너무 많이 실패하면 자동 Stop (원하면 켜자)
                // if (_pollFailCount >= 10) { pollTimer.Stop(); Log("[POLL] auto stop (too many fails)"); }
            }
            finally
            {
                _pollingBusy = false;   // 반드시 해제
            }
        }

        private void chkRecording_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateRecordingState();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Recording 설정 실패: " + ex.Message);
                chkRecording.Checked = false;
                UpdateRecordingState();
            }
        }

        private int ParseRecordSeconds()
        {
            string raw = (cmbRecordEvery?.Text ?? "").Trim();
            if (string.IsNullOrEmpty(raw)) return 60;

            string digits = new string(raw.Where(char.IsDigit).ToArray());
            if (int.TryParse(digits, out int s)) return s;

            if (int.TryParse(raw, out s)) return s;

            return 60;
        }

        private void StartRecordingInternal()
        {
            int seconds = ParseRecordSeconds();
            if (seconds <= 0) seconds = 60;

            byte slaveOrUnit = _tcpMode ? _tcpUnitId : (byte)numSlave.Value;
            byte fc = GetFunctionCode();
            ushort start = (ushort)numStartRegister.Value;
            ushort count = (ushort)numCount.Value;

            if (_pollingConfig != null && pollTimer.Enabled)
            {
                var cfg = _pollingConfig.Value;
                slaveOrUnit = cfg.Slave;
                fc = cfg.FunctionCode;
                start = cfg.Start;
                count = cfg.Count;
            }

            RecordingMode mode = _tcpMode ? RecordingMode.Tcp : RecordingMode.Rtu;

            // endpoint 구성
            string endpoint;
            if (_tcpMode)
            {
                // 가능하면 RemoteEndPoint를 사용(실제 연결 대상)
                endpoint = _tcpClient?.Client?.RemoteEndPoint?.ToString() ?? "tcp";
            }
            else
            {
                endpoint = _sp?.PortName ?? "rtu";
            }

            _recService.Start(mode, endpoint, slaveOrUnit, fc, start, count, seconds);
            Log($"[REC] start every {seconds}s → {_recService.CurrentFilePath}");
        }

        private void UpdateRecordingState()
        {
            if (chkRecording.Checked && pollTimer.Enabled)
            {
                if (!_recService.IsRecording)
                    StartRecordingInternal();
            }
            else
            {
                if (_recService.IsRecording)
                {
                    _recService.Stop();
                    Log("[REC] stop");
                }
            }
        }
    }
}
