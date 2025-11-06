using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormMain : Form
    {
        private void FormMain_Load(object sender, EventArgs e)
        {
            InitializeComponent();
            Controls.Add(new Label { Text = "HELLO", AutoSize = true, Location = new Point(10, 10) });


            // 초기 콤보 세팅
            if (cmbPort.Items.Count == 0)
                cmbPort.Items.AddRange(SerialPort.GetPortNames());
            if (cmbPort.Items.Count > 0) cmbPort.SelectedIndex = 0;

            cmbBaud.Items.Clear();
            cmbBaud.Items.AddRange(new object[] { 9600, 19200, 38400, 57600, 115200 });
            cmbBaud.SelectedItem = 38400;

            cmbParity.DataSource = Enum.GetValues(typeof(Parity));
            cmbDataBits.Items.Clear();
            cmbDataBits.Items.AddRange(new object[] { 7, 8 });
            cmbDataBits.SelectedItem = 8;
            cmbStopBits.DataSource = Enum.GetValues(typeof(StopBits));

            // Tx/Rx 그리드 기본 설정
            foreach (var grid in new[] { gridTx, gridRx })
            {
                grid.AllowUserToAddRows = false;
                grid.RowHeadersVisible = false;
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }

            // Tx 레지스터 기본 행(예: 0x0000~0x0010)
            gridTx.Rows.Clear();
            for (int i = 0; i <= 0x10; i++)
                gridTx.Rows.Add($"{i:X4}h", "", "");
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            // 구현: 시리얼 오픈
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            // 구현: 시리얼 클로즈
        }

        private void btnCalcCrc_Click(object sender, EventArgs e)
        {
            // 구현: 현재 입력값으로 프레임 생성 → CRC 계산 → txtCrc, txtDataCount 채우기
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            // 구현: FC에 따라 프레임 생성 → 시리얼 Write → 로그
        }

        private void btnRxClear_Click(object sender, EventArgs e)
        {
            gridRx.Rows.Clear();
            txtRxSlave.Clear();
            txtRxFc.Clear();
            txtRxStart.Clear();
            txtRxCount.Clear();
            txtRxDataCount.Clear();
            txtRxCrc.Clear();
        }

        private void btnCopyToTx_Click(object sender, EventArgs e)
        {
            // 구현: gridRx 값 → gridTx로 복사
        }

        private void btnPollStart_Click(object sender, EventArgs e)
        {
            pollTimer.Interval = (int)numInterval.Value;
            pollTimer.Start();
        }

        private void btnPollStop_Click(object sender, EventArgs e)
        {
            pollTimer.Stop();
        }

        private void pollTimer_Tick(object sender, EventArgs e)
        {
            // 구현: 주기적 요청 수행(현재 입력 기준)
        }

        private void btnLogClear_Click(object sender, EventArgs e)
        {
            txtLog.Clear();
        }

        private void btnSaveLog_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Filter = "Text|*.txt", FileName = $"modbus_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt" };
            if (sfd.ShowDialog(this) == DialogResult.OK)
                System.IO.File.WriteAllText(sfd.FileName, txtLog.Text);
        }
    }
}
