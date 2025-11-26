using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormComSetting : Form
    {
        private SerialPort? _sp;
        private ModbusSlave? _slave;
        private readonly LayoutScaler _layoutScaler;


        public FormComSetting()
        {
            InitializeComponent();

            _layoutScaler = new LayoutScaler(this);
            _layoutScaler.ApplyInitialScale(1.3f);

            this.StartPosition = FormStartPosition.CenterScreen;
            Load += FormComSetting_Load;
        }

        private void FormComSetting_Load(object sender, EventArgs e)
        {
            // 포트 목록
            cmbPort.Items.AddRange(SerialPort.GetPortNames());
            if (cmbPort.Items.Count > 0) cmbPort.SelectedIndex = 0;

            // Baud/Parity/DataBits/StopBits 초기화
            cmbBaud.Items.AddRange(new object[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 });
            cmbBaud.SelectedItem = 115200;

            cmbParity.DataSource = Enum.GetValues(typeof(Parity));
            cmbDataBits.Items.AddRange(new object[] { 5, 6, 7, 8 });
            cmbDataBits.SelectedItem = 8;
            cmbStopBits.DataSource = Enum.GetValues(typeof(StopBits));
            cmbStopBits.SelectedItem = StopBits.One;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                CloseCurrent();   // 혹시 열려 있던거 정리

                bool slaveMode = chkSlaveMode.Checked;
                string port = cmbPort.Text;
                int baud = (int)cmbBaud.SelectedItem;
                Parity parity = (Parity)cmbParity.SelectedItem;
                int data = (int)cmbDataBits.SelectedItem;
                StopBits stop = (StopBits)cmbStopBits.SelectedItem;
                byte slaveId = 1;  // 필요하면 NumericUpDown 하나 더 두고 거기서 가져오기

                FormMain main;

                if (slaveMode)
                {
                    _slave = new ModbusSlave();
                    _slave.InitDemoData();
                    _slave.Open(port, baud, parity, data, stop, slaveId);

                    main = new FormMain(null, _slave, true, slaveId);
                }
                else
                {
                    _sp = new SerialPort(port, baud, parity, data, stop)
                    {
                        ReadTimeout = 500,
                        WriteTimeout = 500
                    };
                    _sp.Open();

                    main = new FormMain(_sp, null, false, slaveId);
                }

                // 메인 폼 닫힐 때 다시 COM Setting 복귀
                main.FormClosed += (_, __) =>
                {
                    CloseCurrent();
                    this.Show();
                };

                this.Hide();
                main.Show();   // 모델리스로 띄우는 방식
            }
            catch (Exception ex)
            {
                MessageBox.Show("포트 열기 실패: " + ex.Message);
            }
        }

        private void CloseCurrent()
        {
            try
            {
                if (_sp != null)
                {
                    if (_sp.IsOpen) _sp.Close();
                    _sp.Dispose();
                    _sp = null;
                }

                if (_slave != null)
                {
                    _slave.Close();
                    _slave = null;
                }
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            CloseCurrent();
            base.OnFormClosing(e);
        }
    }
}
