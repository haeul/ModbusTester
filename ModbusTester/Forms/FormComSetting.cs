using ModbusTester.Core;
using ModbusTester.Modbus;
using ModbusTester.Utils;
using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormComSetting : Form
    {
        private SerialPort? _sp;            // Master 모드에서 실제 통신에 사용할 포트 인스턴스(없으면 null로 유지)
        private ModbusSlave? _slave;        // Slave 모드에서 요청을 받아줄 ModbusSlave 인스턴스(없으면 null)

        private readonly LayoutScaler _layoutScaler;  // 모니터 해상도·DPI에 따라 폼 전체를 한번에 스케일링하기 위한 유틸

        public FormComSetting()
        {
            InitializeComponent();          // 디자이너에서 배치한 컨트롤들을 실제 폼에 올리는 부분

            _layoutScaler = new LayoutScaler(this);    // 이 폼 기준으로 스케일링 로직을 적용하기 위해 생성자에서 한 번만 만든다
            _layoutScaler.ApplyInitialScale(1.3f);     // 기본 UI가 너무 작지 않도록 전체를 1.3배 키워서 가독성 확보

            this.StartPosition = FormStartPosition.CenterScreen;  // COM 설정 창은 항상 화면 중앙에서 시작하도록 고정

            Load += FormComSetting_Load;    // 폼이 실제로 로드될 때 포트 목록/콤보 초기화를 한 번만 실행하도록 이벤트 연결
        }

        private void FormComSetting_Load(object sender, EventArgs e)
        {
            cmbPort.Items.AddRange(SerialPort.GetPortNames());    // 현재 PC에서 사용 가능한 COM 포트 목록을 그대로 콤보에 채움
            if (cmbPort.Items.Count > 0) cmbPort.SelectedIndex = 0;   // 포트가 하나 이상 있으면 첫 번째 포트를 기본값으로 선택

            cmbBaud.Items.AddRange(new object[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200 }); // 현장에서 자주 쓰는 Baudrate만 수동 등록
            cmbBaud.SelectedItem = 115200;                          // 기본값은 가장 많이 쓰는 115200으로 지정

            cmbParity.DataSource = Enum.GetValues(typeof(Parity));  // Parity는 .NET enum 전체를 바인딩해서 값 추가/변경 없이 유지보수 쉽게
            cmbDataBits.Items.AddRange(new object[] { 5, 6, 7, 8 }); // 일반적으로 쓰는 데이터 비트만 노출해서 선택 실수 줄이기
            cmbDataBits.SelectedItem = 8;                           // 통상적인 기본값인 8비트로 세팅

            cmbStopBits.DataSource = Enum.GetValues(typeof(StopBits)); // StopBits도 enum 전체를 바인딩해 UI와 코드 값이 자동 동기화되게
            cmbStopBits.SelectedItem = StopBits.One;                // 가장 일반적인 1 StopBit를 기본으로 설정
        }
        private ComPortConfig BuildConfigFromUi()
        {
            var cfg = new ComPortConfig();

            cfg.PortName = cmbPort.Text;
            cfg.BaudRate = (int)cmbBaud.SelectedItem;
            cfg.Parity = (Parity)cmbParity.SelectedItem;
            cfg.DataBits = (int)cmbDataBits.SelectedItem;
            cfg.StopBits = (StopBits)cmbStopBits.SelectedItem;
            cfg.SlaveMode = chkSlaveMode.Checked;

            return cfg;
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                CloseCurrent();    // 이전 리소스 정리

                // 1) UI → 설정 객체
                ComPortConfig cfg = BuildConfigFromUi();
                byte slaveId = 1;  // TODO: 나중에 UI에서 받을 거면 여기만 바꾸면 됨

                FormMain main;

                if (cfg.SlaveMode)
                {
                    // 2) Slave 모드
                    _slave = new ModbusSlave();
                    _slave.InitDemoData();
                    _slave.Open(
                        cfg.PortName,
                        cfg.BaudRate,
                        cfg.Parity,
                        cfg.DataBits,
                        cfg.StopBits,
                        slaveId
                    );

                    main = new FormMain(null, _slave, true, slaveId);
                }
                else
                {
                    // 3) Master 모드
                    _sp = new SerialPort(
                        cfg.PortName,
                        cfg.BaudRate,
                        cfg.Parity,
                        cfg.DataBits,
                        cfg.StopBits
                    )
                    {
                        ReadTimeout = 500,
                        WriteTimeout = 500
                    };
                    _sp.Open();

                    main = new FormMain(_sp, null, false, slaveId);
                }

                // 4) 메인 폼 종료 시 정리 + 설정폼 복귀
                main.FormClosed += (_, __) =>
                {
                    CloseCurrent();
                    this.Show();
                };

                this.Hide();
                main.Show();
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
                    if (_sp.IsOpen) _sp.Close();     // 이미 열려 있는 경우에만 Close 호출해서 중복 호출로 인한 예외를 피함
                    _sp.Dispose();                   // OS 자원까지 깔끔하게 해제해서 다른 프로그램에서도 바로 포트를 사용할 수 있게
                    _sp = null;                      // 참조를 null로 만들어 현재는 Master 포트가 없다는 상태를 명확히 표현
                }

                if (_slave != null)
                {
                    _slave.Close();                  // Slave 내부에서 사용하던 포트/스레드 등을 정리
                    _slave = null;                   // 더 이상 Slave 모드가 아니라는 것을 참조 상태로 표시
                }
            }
            catch
            {
                // 실제 종료/정리 과정에서 나는 예외는 사용자에게 보여줄 필요가 크지 않아서 조용히 무시
                // 필요해지면 여기서 로그만 남기도록 확장할 수 있음
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            CloseCurrent();                          // 프로그램 종료 직전에 한 번 더 통신 자원 정리를 보장해서 포트 잠김 사고를 예방
            base.OnFormClosing(e);                   // 기본 폼 종료 처리도 그대로 수행
        }
    }
}
