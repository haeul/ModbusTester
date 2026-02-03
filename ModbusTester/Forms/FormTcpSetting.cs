using ModbusTester.Utils;
using NModbus;
using System;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Drawing;

namespace ModbusTester
{
    public partial class FormTcpSetting : Form
    {
        private readonly LayoutScaler _layoutScaler;

        public FormTcpSetting()
        {
            InitializeComponent();

            this.Text = $"ModbusTester-v{AppVersion.Get()}";
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _layoutScaler = new LayoutScaler(this);
            _layoutScaler.ApplyInitialScale(1.3f);

            this.StartPosition = FormStartPosition.CenterScreen;

            Load += FormTcpSetting_Load;
        }

        private void FormTcpSetting_Load(object sender, EventArgs e)
        {
            // 저장된 값이 있으면 우선 사용
            string savedHost = Properties.Settings.Default.TcpHost;
            int savedPort = Properties.Settings.Default.TcpPort;
            int savedUnit = Properties.Settings.Default.TcpUnitId;

            if (!string.IsNullOrWhiteSpace(savedHost)) txtHost.Text = savedHost;
            else txtHost.Text = "192.168.1.31";

            if (savedPort >= 1 && savedPort <= 65535) numPort.Value = savedPort;
            else numPort.Value = 13000;

            if (savedUnit >= 1 && savedUnit <= 247) numUnitId.Value = savedUnit;
            else numUnitId.Value = 1;
        }

        private TcpConfig BuildConfigFromUi()
        {
            var cfg = new TcpConfig
            {
                Host = (txtHost.Text ?? string.Empty).Trim(),
                Port = (int)numPort.Value,
                UnitId = (byte)numUnitId.Value
            };
            return cfg;
        }

        // ===== Validation =====

        private bool TryValidateTcpInputs(out string errorMessage)
        {
            errorMessage = string.Empty;

            string host = (txtHost.Text ?? string.Empty).Trim();
            int port = (int)numPort.Value;
            int unitId = (int)numUnitId.Value;

            if (string.IsNullOrWhiteSpace(host))
            {
                errorMessage = "Host(IP)를 입력하세요.";
                return false;
            }

            if (port < 1 || port > 65535)
            {
                errorMessage = "Port 범위가 올바르지 않습니다. (1~65535)";
                return false;
            }

            if (unitId < 1 || unitId > 247)
            {
                errorMessage = "Unit ID 범위가 올바르지 않습니다. (1~247)";
                return false;
            }

            if (!IsLikelyHost(host))
            {
                errorMessage = "Host 형식이 올바르지 않습니다. (예: 192.168.1.31 또는 localhost)";
                return false;
            }

            return true;
        }

        private static bool IsLikelyHost(string host)
        {
            if (IPAddress.TryParse(host, out _))
                return true;

            if (host.Length < 1 || host.Length > 255) return false;
            if (host.Contains(" ")) return false;

            foreach (char c in host)
            {
                bool ok = char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_';
                if (!ok) return false;
            }

            return true;
        }

        // (옵션) 오프라인 모드(장비 없이 UI 확인용) - ComSetting과 동일 컨셉
        private void lblHost_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                byte slaveId = 1;
                var main = new FormMain(null, null, false, slaveId);

                main.FormClosed += (_, __) =>
                {
                    this.Show();
                };

                this.Hide();
                main.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("오프라인 모드 실행 실패: " + ex.Message);
            }
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            TcpClient? client = null;

            try
            {
                // 1) 입력값 검증
                if (!TryValidateTcpInputs(out string msg))
                {
                    MessageBox.Show(msg);
                    return;
                }

                // 2) UI → 설정 객체
                TcpConfig cfg = BuildConfigFromUi();

                // 3) TCP 연결
                client = new TcpClient
                {
                    ReceiveTimeout = 500,
                    SendTimeout = 500
                };

                client.Connect(cfg.Host, cfg.Port);

                // 연결 성공 설정 저장
                Properties.Settings.Default.TcpHost = cfg.Host;
                Properties.Settings.Default.TcpPort = cfg.Port;
                Properties.Settings.Default.TcpUnitId = cfg.UnitId;
                Properties.Settings.Default.Save();

                // 4) NModbus Master 생성
                var factory = new ModbusFactory();
                IModbusMaster master = factory.CreateMaster(client);

                // 타임아웃/재시도 정책
                master.Transport.Retries = 0;
                master.Transport.ReadTimeout = 500;
                master.Transport.WriteTimeout = 500;

                // 5) Main 진입 (TCP 모드)
                // FormMain 생성자 시그니처(옵션 파라미터 버전)에 맞춰 호출
                var main = new FormMain(
                    sp: null,
                    slave: null,
                    slaveMode: false,
                    slaveId: cfg.UnitId,
                    tcpClient: client,
                    tcpMaster: master,
                    tcpMode: true,
                    tcpUnitId: cfg.UnitId
                );

                main.FormClosed += (_, __) =>
                {
                    // Main 닫히면 TCP 자원 정리 + Setting 화면 복귀
                    try
                    {
                        if (client != null)
                        {
                            try { client.Close(); } catch { }
                            try { client.Dispose(); } catch { }
                        }
                    }
                    catch { }

                    this.Show();
                };

                this.Hide();
                main.Show();

                // 여기서 client를 Dispose하면 안 됨 (Main이 쓰는 중)
                client = null;
            }
            catch (Exception ex)
            {
                try
                {
                    if (client != null)
                    {
                        try { client.Close(); } catch { }
                        try { client.Dispose(); } catch { }
                    }
                }
                catch { }

                MessageBox.Show("TCP 연결 실패: " + ex.ToString());
            }

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
        }
    }

    // TCP 설정 DTO(ComPortConfig처럼)
    public class TcpConfig
    {
        public string Host { get; set; } = "";
        public int Port { get; set; } = 502;
        public byte UnitId { get; set; } = 1;
    }
}
