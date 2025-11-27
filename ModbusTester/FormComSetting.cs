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

        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                CloseCurrent();    // 이전에 열려 있던 포트/슬레이브가 있으면 먼저 정리해서 중복 Open이나 핸들 잠김을 방지

                bool slaveMode = chkSlaveMode.Checked;  // 체크박스로 Master/Slave 모드를 간단히 전환할 수 있게 플래그로 사용

                string port = cmbPort.Text;             // 사용자가 선택한 실제 포트 이름 (예: COM3)
                int baud = (int)cmbBaud.SelectedItem;   // 콤보에서 선택한 Baudrate 그대로 사용
                Parity parity = (Parity)cmbParity.SelectedItem;  // enum 바인딩 덕분에 캐스팅만으로 바로 사용 가능
                int data = (int)cmbDataBits.SelectedItem;        // 데이터 비트 수
                StopBits stop = (StopBits)cmbStopBits.SelectedItem; // StopBits 설정

                byte slaveId = 1;                        // 현재는 기본 슬레이브 ID를 1로 고정, 이후 FormMain에서 변경 가능

                FormMain main;                           // 실제 Modbus 송수신 및 화면 표시를 담당하는 메인 폼 인스턴스

                if (slaveMode)
                {
                    _slave = new ModbusSlave();          // PC를 Modbus Slave로 동작시키기 위한 객체 생성
                    _slave.InitDemoData();               // 초기 테스트용 레지스터 데이터를 미리 채워두어 바로 통신 테스트 가능하게 함
                    _slave.Open(port, baud, parity, data, stop, slaveId); // 지정한 통신 설정으로 Slave 소켓/포트 오픈

                    main = new FormMain(null, _slave, true, slaveId); // Master 포트는 없고 Slave만 넘기며, 모드 플래그(true)로 Slave임을 알려줌
                }
                else
                {
                    _sp = new SerialPort(port, baud, parity, data, stop) // Master 모드에서는 순수 SerialPort만 사용
                    {
                        ReadTimeout = 500,          // 장치 이상이나 케이블 문제로 응답이 없을 때 무한 대기 방지용 타임아웃
                        WriteTimeout = 500          // 송신도 일정 시간 안에 안 나가면 예외를 던져서 문제를 빨리 인지할 수 있게
                    };
                    _sp.Open();                     // 실제 포트를 여는 시점, 여기서 예외가 가장 많이 발생할 수 있음

                    main = new FormMain(_sp, null, false, slaveId); // Slave는 사용하지 않고 Master 포트만 넘기며, 모드 플래그(false)로 Master임을 명시
                }

                main.FormClosed += (_, __) =>        // 메인 폼이 닫힐 때의 공통 처리 정의
                {
                    CloseCurrent();                  // 메인 폼에서 쓰던 통신 자원을 확실히 정리해서 포트 잠금 문제 방지
                    this.Show();                     // 프로그램 전체 종료가 아니라, 다시 COM 설정 화면으로 돌아올 수 있게 복귀
                };

                this.Hide();                         // 설정 폼은 계속 살아있되 화면에서만 숨겨, 나중에 재사용할 수 있게 유지
                main.Show();                         // 메인 폼을 모달이 아닌 일반 폼으로 띄워서 자연스러운 메인 화면처럼 보이게
            }
            catch (Exception ex)
            {
                MessageBox.Show("포트 열기 실패: " + ex.Message); // 어떤 이유로든 Open에 실패했을 때 구체적인 에러를 그대로 보여줘 원인 파악을 돕기
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
