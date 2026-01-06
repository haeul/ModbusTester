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
            public byte Slave;
            public byte FunctionCode;
            public ushort Start;
            public ushort Count;
        }

        private void btnPollStart_Click(object sender, EventArgs e)
        {
            if (_slaveMode)
            {
                MessageBox.Show("Slave 모드에서는 폴링을 사용할 수 없습니다.");
                return;
            }

            byte fc = GetFunctionCode();
            if (!(fc == 0x03 || fc == 0x04))
            {
                MessageBox.Show("폴링은 Function Code 03/04(Read)만 지원합니다.");
                return;
            }

            _pollingConfig = new PollingConfig
            {
                Slave = (byte)numSlave.Value,
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
            try
            {
                if (!_isOpen) return;
                if (_poller == null) return;

                if (_pollingConfig == null) return;

                var cfg = _pollingConfig.Value;
                byte slave = cfg.Slave;
                ushort start = cfg.Start;
                ushort count = cfg.Count;
                byte fc = cfg.FunctionCode;

                var result = _poller.Poll(slave, fc, start, count);

                Log("TX: " + result.Request.ToHex());
                Log("RX: " + result.Response.ToHex());

                if (result.IsException)
                    return;

                UpdateReceiveHeader(result.Response, slave, fc, start, count);
                _gridController.FillRxGrid(start, result.Values);

                RegisterCache.UpdateRange(start, result.Values);
                _recService.AppendSnapshotIfDue(DateTime.Now, result.Values);
            }
            catch
            {
                // 폴링 중 예외는 조용히 무시
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

            byte slave = (byte)numSlave.Value;
            byte fc = GetFunctionCode();
            ushort start = (ushort)numStartRegister.Value;
            ushort count = (ushort)numCount.Value;

            if (_pollingConfig != null && pollTimer.Enabled)
            {
                var cfg = _pollingConfig.Value;
                slave = cfg.Slave;
                fc = cfg.FunctionCode;
                start = cfg.Start;
                count = cfg.Count;
            }

            _recService.Start(slave, fc, start, count, seconds);
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
