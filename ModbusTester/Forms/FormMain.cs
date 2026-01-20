using ModbusTester.Controls;
using ModbusTester.Core;
using ModbusTester.Modbus;
using ModbusTester.Presets;
using ModbusTester.Services;
using ModbusTester.Utils;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static ModbusTester.Utils.HexExtensions;

namespace ModbusTester
{
    public partial class FormMain : Form
    {
        // 컬럼 인덱스
        private const int COL_REG = 0;
        private const int COL_NAME = 1;
        private const int COL_HEX = 2;
        private const int COL_DEC = 3;
        private const int COL_BIT = 4;

        private readonly LayoutScaler _layoutScaler;

        private readonly SerialPort? _sp;
        private readonly ModbusSlave? _slave;
        private readonly bool _slaveMode;

        private readonly SerialModbusClient? _client;
        private readonly ModbusMasterService? _master;
        private readonly ModbusPoller? _poller;

        private FormQuickView? _quick;

        private readonly RegisterGridController _gridController;

        private readonly RecordingService _recService;

        private FormMacroSetting? _macroForm;

        private FormGridZoom? _rxZoom;

        private PollingConfig? _pollingConfig;

        // Preset 실행 분리 객체
        private PresetRunner? _presetRunner;

        private bool _isOpen => !_slaveMode && _sp != null && _sp.IsOpen;

        public FormMain(SerialPort? sp, ModbusSlave? slave, bool slaveMode, byte slaveId)
        {
            InitializeComponent();

            this.Text = $"ModbusTester-v{AppVersion.Get()}";

            StartPosition = FormStartPosition.CenterScreen;

            // exe에 박혀 있는 아이콘을 그대로 폼 아이콘으로 사용
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _layoutScaler = new LayoutScaler(this);
            _layoutScaler.ApplyInitialScale(1.058f);

            _sp = sp;
            _slave = slave;
            _slaveMode = slaveMode;

            // Recording 디렉토리: 실행 폴더 하위의 Data
            string recDir = Path.Combine(Application.StartupPath, "Data");
            _recService = new RecordingService(recDir);

            // 그리드 컨트롤러
            _gridController = new RegisterGridController(
                gridTx,
                gridRx,
                numStartRegister,
                COL_REG,
                COL_NAME,
                COL_HEX,
                COL_DEC,
                COL_BIT
            );

            // Master 모드에서만 통신 객체 생성 (오프라인 UI 용도로 _sp == null 허용)
            if (!_slaveMode && _sp != null)
            {
                _client = new SerialModbusClient(_sp);
                _master = new ModbusMasterService(_client);
                _poller = new ModbusPoller(_client);

                // PresetRunner 생성
                // Log/UpdateReceiveHeader/RegisterCache 갱신은 기존 FormMain 메서드를 그대로 콜백으로 넘김
                _presetRunner = new PresetRunner(
                    _master,
                    _gridController,
                    Log,
                    UpdateReceiveHeader,
                    (start, values) => RegisterCache.UpdateRange(start, values)
                );
            }

            Shown += FormMain_Shown;
            numSlave.Value = slaveId;
        }

        private async void FormMain_Shown(object? sender, EventArgs e)
        {
            await _gridController.InitializeGridsAsync();

            // 그리드 초기화 후 TX Name을 RX로 1회 동기화(초기 상태 안정화)
            _gridController.SyncAllTxNamesToRx();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            // Function Code 기본값
            if (cmbFunctionCode.Items.Count == 0)
            {
                cmbFunctionCode.Items.AddRange(new object[]
                {
                    "03h Read HR",
                    "04h Read IR",
                    "06h Write SR",
                    "10h Write MR"
                });
            }

            if (cmbFunctionCode.SelectedIndex < 0)
                cmbFunctionCode.SelectedIndex = 0;

            // TX 기본 설정
            numStartRegister.Hexadecimal = true;
            numStartRegister.Minimum = 0x0000;
            numStartRegister.Maximum = 0xFFFF;
            numStartRegister.Value = 0x0000;

            numCount.Value = 1;
            RefreshDataCount();

            // 그리드 공통 설정
            _gridController.SetupGrids();

            // (중복 방지) 이벤트 재결합
            gridTx.KeyDown -= GridTx_KeyDown;
            gridTx.KeyDown += GridTx_KeyDown;

            gridTx.KeyDown += Grid_DeleteKeyDown;
            gridRx.KeyDown += Grid_DeleteKeyDown;

            gridTx.EditingControlShowing -= GridTx_EditingControlShowing;
            gridTx.EditingControlShowing += GridTx_EditingControlShowing;

            gridTx.CellValueChanged -= GridTx_CellValueChanged;
            gridTx.CellValueChanged += GridTx_CellValueChanged;

            gridTx.CurrentCellDirtyStateChanged -= GridTx_CurrentCellDirtyStateChanged;
            gridTx.CurrentCellDirtyStateChanged += GridTx_CurrentCellDirtyStateChanged;

            // 아래 두 이벤트는 디자이너에서 이미 연결되어 있을 수 있음
            // (중복 호출 방지 위해 -= 후 += 형태로 안정 결합)
            gridTx.CellBeginEdit -= Grid_CellBeginEdit;
            gridTx.CellBeginEdit += Grid_CellBeginEdit;

            gridRx.CellBeginEdit -= Grid_CellBeginEdit;
            gridRx.CellBeginEdit += Grid_CellBeginEdit;

            gridTx.CellEndEdit -= HexAutoFormat_OnEndEdit;
            gridTx.CellEndEdit += HexAutoFormat_OnEndEdit;

            gridRx.CellEndEdit -= HexAutoFormat_OnEndEdit;
            gridRx.CellEndEdit += HexAutoFormat_OnEndEdit;

            gridRx.CellDoubleClick += Grid_CellDoubleClick_OpenZoom;

            // RX 그리드에서 QV 컬럼을 맨 앞으로
            var qvCol = gridRx.Columns["colRxQuickView"];
            if (qvCol != null)
                qvCol.DisplayIndex = 0;

            // 모드에 따른 UI 반영
            UpdateUiByMode();

            // 프리셋 로드
            FunctionPresetManager.Load();
            RefreshPresetCombo();
        }

        private void UpdateUiByMode()
        {
            bool slave = _slaveMode;

            grpTx.Enabled = !slave;   // TX 영역
            grpRx.Enabled = !slave;   // RX 영역
            grpOpt.Enabled = !slave;  // Polling / Recording 옵션

            btnSend.Enabled = !slave;
            btnCalcCrc.Enabled = !slave;
            btnPollStart.Enabled = !slave;
            btnPollStop.Enabled = !slave;

            gridTx.Enabled = !slave;
            gridRx.Enabled = !slave;

            Log(slave
                ? "[MODE] Slave 모드 (해당 포트로 들어오는 요청에 응답)"
                : "[MODE] Master 모드 (요청을 전송)");
        }

        private void chkSlaveMode_CheckedChanged(object sender, EventArgs e)
        {
            pollTimer.Stop();
            _pollingConfig = null;
            UpdateRecordingState();
            UpdateUiByMode();
        }

        private void Log(string line)
        {
            LogDirection dir = LogDirection.None;

            if (line.StartsWith("TX:", StringComparison.OrdinalIgnoreCase))
                dir = LogDirection.Tx;
            else if (line.StartsWith("RX:", StringComparison.OrdinalIgnoreCase))
                dir = LogDirection.Rx;

            Color color = dir switch
            {
                LogDirection.Tx => Color.DarkBlue,
                LogDirection.Rx => Color.DarkGreen,
                _ => txtLog.ForeColor
            };

            txtLog.SelectionStart = txtLog.TextLength;
            txtLog.SelectionLength = 0;
            txtLog.SelectionColor = color;

            txtLog.AppendText(line + Environment.NewLine);

            txtLog.SelectionColor = txtLog.ForeColor;
            txtLog.ScrollToCaret();
        }

        private enum LogDirection { None, Tx, Rx }

        private byte GetFunctionCode()
        {
            string raw = (cmbFunctionCode?.Text ?? "03").Trim();
            int space = raw.IndexOf(' ');
            if (space > 0) raw = raw.Substring(0, space).Trim();
            if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) raw = raw.Substring(2);
            if (raw.EndsWith("h", StringComparison.OrdinalIgnoreCase)) raw = raw.Substring(0, raw.Length - 1);
            if (raw.Length == 1) raw = "0" + raw;

            if (byte.TryParse(raw, NumberStyles.HexNumber, null, out var fcHex))
                return fcHex;
            if (byte.TryParse(raw, out var fcDec))
                return fcDec;

            return 0x03;
        }

        private void RefreshDataCount()
        {
            byte fc = GetFunctionCode();
            ushort count = (ushort)numCount.Value;

            if (fc == 0x10)
                txtDataCount.Text = (count * 2).ToString();
            else if (fc == 0x06)
                txtDataCount.Text = "2";
            else
                txtDataCount.Text = "0";
        }

        private void numCount_ValueChanged(object sender, EventArgs e) => RefreshDataCount();
        private void cmbFunctionCode_SelectedIndexChanged(object sender, EventArgs e) => RefreshDataCount();
        private void cmbFunctionCode_TextChanged(object sender, EventArgs e) => RefreshDataCount();

        // ───────────────────── RX 헤더 업데이트 ─────────────────────

        private void UpdateReceiveHeader(byte[] resp, byte slave, byte fc, ushort start, ushort count)
        {
            txtRxSlave.Text = slave.ToString();
            txtRxFc.Text = $"{fc:X2}h";
            txtRxStart.Text = $"{start:X4}h";
            txtRxCount.Text = count.ToString();

            if (resp.Length >= 3)
                txtRxDataCount.Text = (fc == 0x03 || fc == 0x04) ? resp[2].ToString() : "0";
            if (resp.Length >= 4)
                txtRxCrc.Text = $"{resp[^1]:X2}{resp[^2]:X2}h";
        }
    }
}
