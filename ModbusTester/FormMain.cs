using System;                        // .NET 기본 네임스페이스(기본 타입, 이벤트 등)
using System.Drawing;               // GDI+ 관련 타입(폼 컨트롤의 색/폰트/이미지 등)
using System.IO;                    // 파일/스트림 입출력 (Recording에 사용)
using System.IO.Ports;              // 시리얼 포트 API
using System.Linq;                  // LINQ (배열/컬렉션 처리에 사용)
using System.Text;                  // 문자열 인코딩 등
using System.Threading;             // Thread.Sleep 등 스레딩 유틸
using System.Windows.Forms;         // WinForms UI 구성요소

namespace ModbusTester              // 프로젝트 네임스페이스
{
    public partial class FormMain : Form   // WinForms의 메인 폼 클래스(디자이너 분할 클래스)
    {
        // ───────────────────────── Fields ─────────────────────────
        private readonly SerialPort _sp = new SerialPort();   // 시리얼 포트 인스턴스(마스터 모드에서 사용)
        private bool _isOpen => _sp.IsOpen;                   // 포트가 열려 있는지 간편 확인용 프로퍼티

        // Slave 모드용 에뮬레이터 인스턴스
        private ModbusSlave _slave;                           // 슬레이브 역할을 하는 에뮬레이터(슬레이브 모드에서만 활성)

        // Recording 관련 필드
        private StreamWriter _recWriter = null;               // 로그 파일로 기록할 때 쓰는 StreamWriter
        private DateTime _recUntil;                           // 녹화를 종료할 목표 시각
        private readonly System.Windows.Forms.Timer _recTimer = new System.Windows.Forms.Timer(); // 주기적으로 종료시각 도달 확인

        // Quick View
        private FormQuickView? _quick;                        // 빠른 보기 팝업 폼(Nullable 허용)

        // ───────────────────────── Ctor ─────────────────────────
        public FormMain()
        {
            InitializeComponent();                            // 디자이너가 생성한 컨트롤 초기화(폼 구성요소 생성/배치)
        }

        // 모드(마스터/슬레이브)에 따른 UI 잠금
        private void UpdateUiByMode()
        {
            bool slave = chkSlaveMode.Checked;                // 체크박스 상태로 슬레이브 모드 여부 판단

            // Slave 모드일 때 마스터 전송 관련 UI 비활성화
            btnSend.Enabled = !slave;                         // 슬레이브일 때 전송 버튼 비활성
            btnCalcCrc.Enabled = !slave;                      // CRC 계산 버튼 비활성
            btnPollStart.Enabled = !slave;                    // 폴링 시작 버튼 비활성
            btnPollStop.Enabled = !slave;                     // 폴링 중지 버튼 비활성

            gridRx.Enabled = !slave;                          // 슬레이브 모드에서는 수신 그리드 편집 방지

            Log(slave ? "[MODE] Slave 모드 (해당 포트로 들어오는 요청에 응답)"
                      : "[MODE] Master 모드 (요청을 전송)"); // 현재 모드 로그
        }

        // ───────────────────────── Load ─────────────────────────
        private void FormMain_Load(object sender, EventArgs e)
        {
            // COM 포트 목록
            if (cmbPort.Items.Count == 0)
                cmbPort.Items.AddRange(SerialPort.GetPortNames()); // 연결 가능한 포트 이름 나열
            if (cmbPort.Items.Count > 0) cmbPort.SelectedIndex = 0; // 첫 항목 기본 선택

            // Baud / Parity / DataBits / StopBits
            cmbBaud.Items.Clear();
            cmbBaud.Items.AddRange(new object[] { 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400, 460800, 921600 }); // 일반 속도 목록
            cmbBaud.SelectedItem = 115200;                     // 기본 보레이트 115200

            cmbParity.DataSource = Enum.GetValues(typeof(Parity)); // Parity 열거형 바인딩(None/Even/Odd ...)
            cmbDataBits.Items.Clear();
            cmbDataBits.Items.AddRange(new object[] { 5, 6, 7, 8 }); // 데이터 비트 후보
            cmbDataBits.SelectedItem = 8;                   // 기본 8비트
            cmbStopBits.DataSource = Enum.GetValues(typeof(StopBits)); // StopBits 열거형 바인딩
            cmbStopBits.SelectedItem = StopBits.One;       // 기본 1 스톱비트

            // Function Code 기본값
            if (cmbFunctionCode.Items.Count == 0)
                cmbFunctionCode.Items.AddRange(new object[] { "03h Read HR", "04h Read IR", "06h Write SR", "10h Write MR" }); // FC 선택지
            if (cmbFunctionCode.SelectedIndex < 0)
                cmbFunctionCode.SelectedIndex = 0;         // 기본 03h 선택

            // TX 기본
            numSlave.Value = 1;                            // 기본 슬레이브 주소 1
            numStartRegister.Hexadecimal = true;           // 시작 레지스터를 16진수 표현으로 사용
            numStartRegister.Minimum = 0x0000;             // 최소 주소 0000h
            numStartRegister.Maximum = 0x11FF;             // 최대 주소 11FFh
            numStartRegister.Value = 0x0000;               // 기본값 0000h
            numCount.Value = 1;                            // 기본 레지스터 개수 1
            RefreshDataCount();                            // FC/Count에 맞춰 DataCount 텍스트 갱신

            // 그리드 공통 스타일
            SetupGrid(gridTx);                             // TX 그리드 스타일 설정
            SetupGrid(gridRx);                             // RX 그리드 스타일 설정

            // TX 행 0000h~11FFh
            InitializeTxRows();                            // 전송 그리드에 주소행 생성

            // RX 행 0000h~11FFh
            InitializeRxRows();                            // 수신 그리드에 주소행 생성

            // 모드에 따른 UI 반영
            UpdateUiByMode();                              // 현재 모드(Slave 체크박스)에 맞춰 UI 잠금/해제

            // Recording 타이머 설정
            _recTimer.Interval = 200;                      // 0.2초마다 종료시점 체크
            _recTimer.Tick += (s, ev) =>                   // 타이머 틱 이벤트: 종료시각 도달 시 녹화 정지
            {
                if (_recWriter == null) { _recTimer.Stop(); return; } // 녹화 중 아니면 타이머 중지
                if (DateTime.Now >= _recUntil) StopRecording();       // 시간이 되면 녹음 종료
            };
        }

        // 체크박스 핸들러
        private void chkSlaveMode_CheckedChanged(object sender, EventArgs e)
        {
            pollTimer.Stop();                               // 모드 변경 시 폴링 중지(충돌 방지)
            UpdateUiByMode();                               // UI 상태 갱신
        }

        // ───────────────────────── UI Helpers ─────────────────────────
        private void SetupGrid(DataGridView g)
        {
            g.AllowUserToAddRows = false;                  // 사용자 임의 행 추가 금지
            g.RowHeadersVisible = false;                   // 행 머리글 숨김
            g.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // 행 단위 선택
            g.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // 열 폭 자동 채움

            // 3열 고정(Register, HEX, DEC)
            if (g.Columns.Count == 3)
            {
                g.Columns[0].FillWeight = 110; // Register  열 가중치
                g.Columns[1].FillWeight = 90;  // HEX       열 가중치
                g.Columns[2].FillWeight = 90;  // DEC       열 가중치
            }
        }

        private void InitializeTxRows()
        {
            gridTx.Rows.Clear();                            // 기존 행 제거
            for (int i = 0x0000; i <= 0x11FF; i++)          // 주소 0000h ~ 11FFh
                gridTx.Rows.Add($"{i:X4}h", "", "");      // Register/HEX/DEC(빈값)으로 행 추가
        }

        private void InitializeRxRows()
        {
            gridRx.Rows.Clear();                            // 기존 행 제거
            for (int i = 0x0000; i <= 0x11fF; i++)          // 주소 0000h ~ 11FFh (주의: fF 대소문자 혼용이지만 동일)
                gridRx.Rows.Add($"{i:X4}h", "", "");      // Register/HEX/DEC(빈값)으로 행 추가
        }

        // ───────────────────────── Events (COM) ─────────────────────────
        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                pollTimer.Stop();                           // 열기 전 폴링 중지
                // 양쪽 다 닫고 시작 (충돌 방지)
                if (_isOpen) _sp.Close();                   // 마스터 포트 열려 있으면 닫기
                _slave?.Close();                            // 슬레이브 에뮬레이터가 열려 있으면 닫기

                if (chkSlaveMode.Checked)
                {
                    // ───── Slave 모드: 같은 COM 설정으로 슬레이브 열기 ─────
                    _slave = new ModbusSlave();            // 슬레이브 인스턴스 생성
                    _slave.InitDemoData();                  // 데모 레지스터 데이터 채우기
                    _slave.Open(
                        portName: cmbPort.Text,            // 포트명
                        baud: int.Parse(cmbBaud.Text),     // 보레이트
                        parity: (Parity)(cmbParity.SelectedItem ?? Parity.None), // 패리티
                        dataBits: (int)(cmbDataBits.SelectedItem ?? 8),          // 데이터 비트
                        stopBits: (StopBits)(cmbStopBits.SelectedItem ?? StopBits.One), // 스톱비트
                        slaveId: (byte)numSlave.Value      // 슬레이브 주소
                    );
                    Log($"[SLAVE] OPEN {cmbPort.Text} (ID={numSlave.Value})"); // 로그
                }
                else
                {
                    // ───── Master 모드: 기존 로직 그대로 ─────
                    _sp.PortName = cmbPort.Text;           // 포트명 설정
                    _sp.BaudRate = int.Parse(cmbBaud.Text); // 보레이트 설정
                    _sp.Parity = (Parity)(cmbParity.SelectedItem ?? Parity.None); // 패리티 설정
                    _sp.DataBits = (int)(cmbDataBits.SelectedItem ?? 8);         // 데이터 비트 설정
                    _sp.StopBits = (StopBits)(cmbStopBits.SelectedItem ?? StopBits.One); // 스톱비트 설정
                    _sp.ReadTimeout = 500;                 // 읽기 타임아웃(ms)
                    _sp.WriteTimeout = 500;                // 쓰기 타임아웃(ms)

                    _sp.Open();                            // 포트 열기
                    Log("[MASTER] PORT OPEN " + cmbPort.Text); // 로그
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("포트 열기 실패: " + ex.Message); // 예외 메시지 표시
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                pollTimer.Stop();                           // 닫기 전에 폴링 중지

                // 레코딩 중이면 먼저 안전 종료
                StopRecording();                            // 파일 핸들 안전 종료

                if (chkSlaveMode.Checked)
                {
                    _slave?.Close();                        // 슬레이브 에뮬레이터 닫기
                    _slave = null;                          // 참조 해제
                    Log("[SLAVE] CLOSE");
                }
                else
                {
                    if (_isOpen) _sp.Close();               // 마스터 포트 닫기
                    Log("[MASTER] PORT CLOSE");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("포트 닫기 실패: " + ex.Message); // 예외 메시지 표시
            }
        }

        // ───────────────────────── Events (TX/RX Controls) ─────────────────────────
        private void btnCalcCrc_Click(object sender, EventArgs e)
        {
            try
            {
                byte slave = (byte)numSlave.Value;         // 슬레이브 주소
                ushort start = (ushort)numStartRegister.Value; // 시작 주소
                ushort count = (ushort)numCount.Value;      // 레지스터 개수
                byte fc = GetFunctionCode();                // Function Code 추출

                byte[] frameBody;                           // CRC 제외한 요청 본문

                if (fc == 0x03 || fc == 0x04)
                {
                    // Read Frame (CRC 제외하고 본문만 구성)
                    frameBody = new byte[]
                    {
                        slave, fc,
                        (byte)(start >> 8), (byte)(start & 0xFF),
                        (byte)(count >> 8), (byte)(count & 0xFF)
                    };
                    txtDataCount.Text = "0";               // 읽기 요청엔 데이터 영역 없음
                }
                else if (fc == 0x06)
                {
                    ushort val = ReadTxValueOrZero(0);     // 첫 행 값 또는 0
                    frameBody = new byte[]
                    {
                        slave, fc,
                        (byte)(start >> 8), (byte)(start & 0xFF),
                        (byte)(val >> 8), (byte)(val & 0xFF)
                    };
                    txtDataCount.Text = "2";               // 1레지스터=2바이트
                }
                else if (fc == 0x10)
                {
                    ushort[] vals = ReadTxValues(count);   // count개 값 읽기(TX표에서)
                    byte byteCount = (byte)(vals.Length * 2); // 총 데이터 바이트 수
                    var temp = new byte[7 + byteCount];    // CRC 제외 본문 길이
                    temp[0] = slave;
                    temp[1] = fc;
                    temp[2] = (byte)(start >> 8);
                    temp[3] = (byte)(start & 0xFF);
                    temp[4] = (byte)(count >> 8);
                    temp[5] = (byte)(count & 0xFF);
                    temp[6] = byteCount;

                    for (int i = 0; i < vals.Length; i++)  // 값들을 Hi/Lo 순서로 채움
                    {
                        temp[7 + i * 2] = (byte)(vals[i] >> 8);
                        temp[8 + i * 2] = (byte)(vals[i] & 0xFF);
                    }
                    frameBody = temp;
                    txtDataCount.Text = byteCount.ToString();
                }
                else
                {
                    throw new NotSupportedException("지원하지 않는 Function Code");
                }

                // CRC 계산
                ushort crc = Crc16(frameBody, frameBody.Length); // 표준 Modbus CRC16 계산
                byte crcLo = (byte)(crc & 0xFF);                 // Lo 바이트
                byte crcHi = (byte)((crc >> 8) & 0xFF);          // Hi 바이트

                txtCrc.Text = $"{crcLo:X2} {crcHi:X2}";         // CRC 표시(Lo Hi)
                Log($"[CRC CALC] {ToHex(frameBody)} {crcLo:X2} {crcHi:X2}"); // 로그 출력
            }
            catch (Exception ex)
            {
                MessageBox.Show("CRC 계산 실패: " + ex.Message);  // 예외 메시지
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (chkSlaveMode.Checked)
            {
                MessageBox.Show("지금은 Slave 모드입니다. Master 모드에서 전송하세요."); // 슬레이브 모드 차단
                return;
            }
            if (!_isOpen) { MessageBox.Show("먼저 포트를 OPEN 하세요."); return; } // 포트 미오픈 방지

            try
            {
                byte slave = (byte)numSlave.Value;         // 슬레이브 주소
                ushort start = (ushort)numStartRegister.Value; // 시작 주소
                ushort count = (ushort)numCount.Value;      // 개수
                byte fc = GetFunctionCode();                // 기능 코드

                byte[] req;                                 // 최종 요청 프레임(=본문+CRC)

                if (fc == 0x03 || fc == 0x04)
                {
                    req = BuildReadFrame(slave, fc, start, count); // 읽기 요청 프레임 생성
                    Log("TX: " + ToHex(req));                     // 송신 로그
                    var resp = SendAndReceive(req);                // 전송/수신
                    Log("RX: " + ToHex(resp));                    // 수신 로그

                    if (resp.Length >= 3 && resp[1] == (byte)(fc | 0x80))
                        throw new Exception($"장치 오류 (예외코드: {resp[2]})"); // 예외 응답 처리

                    UpdateReceiveHeader(resp, slave, fc, start, count); // 헤더 텍스트 채움
                    var values = ParseReadResponse(resp);          // 데이터 파싱(ushort[])
                    FillRxGrid(start, values);                     // RX 그리드에 값 채우기

                    RegisterCache.UpdateRange(start, values);      // 내부 캐시 갱신
                }
                else if (fc == 0x06)
                {
                    ushort val = ReadTxValueOrZero(0);            // 첫 행의 값 읽기
                    req = BuildWriteSingleFrame(slave, start, val);// 06 프레임 생성
                    Log("TX: " + ToHex(req));
                    var resp = SendAndReceive(req);                // 전송/수신
                    Log("RX: " + ToHex(resp));
                    UpdateReceiveHeader(resp, slave, fc, start, 1);// 헤더 갱신(Count=1)

                    RegisterCache.UpdateRange(start, new ushort[] { val }); // 캐시 반영
                }
                else if (fc == 0x10)
                {
                    ushort[] vals = ReadTxValues(count);          // 여러 값 읽기
                    req = BuildWriteMultipleFrame(slave, start, vals); // 10 프레임 생성
                    Log("TX: " + ToHex(req));
                    var resp = SendAndReceive(req);
                    Log("RX: " + ToHex(resp));
                    UpdateReceiveHeader(resp, slave, fc, start, (ushort)vals.Length); // 헤더 갱신

                    RegisterCache.UpdateRange(start, vals);        // 캐시 반영
                }
                else
                {
                    throw new NotSupportedException("지원하지 않는 Function Code"); // 방어 코드
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("전송 실패: " + ex.Message);       // 예외 안내
            }
        }

        private void btnRxClear_Click(object sender, EventArgs e)
        {
            // 행 유지, 값만 초기화
            foreach (DataGridViewRow r in gridRx.Rows)
            {
                if (r.IsNewRow) continue;                  // 새 행은 제외
                r.Cells[1].Value = ""; // HEX 초기화
                r.Cells[2].Value = ""; // DEC 초기화
            }

            txtRxSlave.Clear();
            txtRxFc.Clear();
            txtRxStart.Clear();
            txtRxCount.Clear();
            txtRxDataCount.Clear();
            txtRxCrc.Clear();
        }

        private void btnCopyToTx_Click(object sender, EventArgs e)
        {
            // RX의 값 있는 셀들만 TX의 같은 주소로 복사
            foreach (DataGridViewRow rx in gridRx.Rows)
            {
                if (rx.IsNewRow) continue;                 // 새 행 제외

                string reg = Convert.ToString(rx.Cells[0].Value) ?? ""; // 주소 문자열
                string hex = Convert.ToString(rx.Cells[1].Value) ?? ""; // HEX 값
                string dec = Convert.ToString(rx.Cells[2].Value) ?? ""; // DEC 값

                // 값이 비었으면 스킵(행은 유지)
                if (string.IsNullOrWhiteSpace(hex) && string.IsNullOrWhiteSpace(dec))
                    continue;

                // 같은 주소의 TX 행 찾아 덮어쓰기
                foreach (DataGridViewRow tx in gridTx.Rows)
                {
                    if (tx.IsNewRow) continue;
                    if (Convert.ToString(tx.Cells[0].Value) == reg)
                    {
                        tx.Cells[1].Value = hex;           // HEX 복사
                        tx.Cells[2].Value = dec;           // DEC 복사
                        break;                             // 해당 주소 찾았으면 종료
                    }
                }
            }
        }

        private void btnLogClear_Click(object sender, EventArgs e)
        {
            txtLog.Clear();                                 // 로그 텍스트 박스 비우기
        }

        private void btnSaveLog_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Filter = "Text|*.txt", FileName = $"modbus_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt" }; // 저장 대화상자
            if (sfd.ShowDialog(this) == DialogResult.OK)
                System.IO.File.WriteAllText(sfd.FileName, txtLog.Text); // 파일로 저장
        }

        // Register Count / FC 변경 시 DataCount 자동 갱신
        private void numCount_ValueChanged(object sender, EventArgs e) => RefreshDataCount(); // 카운트가 바뀌면 DataCount 반영
        private void cmbFunctionCode_SelectedIndexChanged(object sender, EventArgs e) => RefreshDataCount(); // FC 선택 바뀌면 반영
        private void cmbFunctionCode_TextChanged(object sender, EventArgs e) => RefreshDataCount(); // FC 텍스트 변경 반영

        // 폴링
        private void btnPollStart_Click(object sender, EventArgs e)
        {
            if (chkSlaveMode.Checked)
            {
                MessageBox.Show("Slave 모드에서는 폴링을 사용할 수 없습니다."); // 슬레이브 모드 차단
                return;
            }
            pollTimer.Interval = (int)numInterval.Value;    // 폴링 주기 설정(ms)
            pollTimer.Start();                              // 타이머 시작
        }
        private void btnPollStop_Click(object sender, EventArgs e)
        {
            pollTimer.Stop();                               // 폴링 중지
        }

        private void pollTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (!_isOpen) return;                       // 포트 안 열렸으면 무시

                byte slave = (byte)numSlave.Value;          // 슬레이브 주소
                ushort start = (ushort)numStartRegister.Value; // 시작 주소
                ushort count = (ushort)numCount.Value;      // 개수
                byte fc = GetFunctionCode();                // FC
                if (!(fc == 0x03 || fc == 0x04)) return;    // 읽기 FC만 폴링 대상

                var req = BuildReadFrame(slave, fc, start, count); // 읽기 프레임 구성
                var resp = SendAndReceive(req);             // 전송/수신

                Log("TX: " + ToHex(req));                  // TX 로그
                Log("RX: " + ToHex(resp));                 // RX 로그

                if (resp.Length >= 3 && resp[1] == (byte)(fc | 0x80))
                    return; // 장치 예외 시 무시(폴링 계속 진행)

                UpdateReceiveHeader(resp, slave, fc, start, count); // 헤더 갱신
                var values = ParseReadResponse(resp);       // 값 파싱
                FillRxGrid(start, values);                  // RX 그리드 반영

                RegisterCache.UpdateRange(start, values);   // 내부 캐시 갱신
            }
            catch { /* 폴링 중 예외 무시 */ }              // 폴링은 예외로 멈추지 않도록 무시
        }

        private void FillRxGrid(ushort startAddr, ushort[] values)
        {
            if (values == null || values.Length == 0) return; // 값 없으면 종료

            // 0000h~11FFh 범위에서만 갱신
            for (int i = 0; i < values.Length; i++)
            {
                ushort addr = (ushort)(startAddr + i);     // 현재 주소
                if (addr < 0x0000 || addr > 0x11FF) continue; // 범위 벗어나면 무시

                string key = $"{addr:X4}h";                // 그리드 첫 열의 주소 문자열 형식

                // 주소로 행 찾기
                foreach (DataGridViewRow r in gridRx.Rows)
                {
                    if (r.IsNewRow) continue;              // 새 행 제외
                    if (Convert.ToString(r.Cells[0].Value) == key) // 주소 일치
                    {
                        r.Cells[1].Value = $"{values[i]:X4}"; // HEX(4자리 대문자)
                        r.Cells[2].Value = values[i].ToString(); // DEC
                        break;                             // 찾았으면 다음 값으로
                    }
                }
            }
        }

        // ───────────────────────── Helpers ─────────────────────────
        private void Log(string line)
        {
            txtLog.AppendText(line + Environment.NewLine);  // 화면 로그에 한 줄 추가

            // [추가] Recording 중이면 파일에도 동시에 기록
            if (_recWriter != null)
            {
                try
                {
                    _recWriter.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {line}"); // 타임스탬프와 함께 기록
                    _recWriter.Flush();                    // 즉시 디스크 반영 시도
                }
                catch
                {
                    // 파일 문제가 생겨도 앱은 계속 동작하도록 무시
                }
            }
        }

        private static string ToHex(byte[] bytes)
            => BitConverter.ToString(bytes).Replace("-", " "); // 바이트 배열을 "AA BB CC" 포맷 문자열로 변환

        private byte GetFunctionCode()
        {
            // "10h Write MR" → "10h" → "10" → 0x10
            string raw = (cmbFunctionCode?.Text ?? "03").Trim(); // 콤보 텍스트를 가져옴
            int space = raw.IndexOf(' ');                // 공백 전까지가 코드 표기(예: "10h")
            if (space > 0) raw = raw.Substring(0, space).Trim();
            if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) raw = raw.Substring(2); // 0x 접두 제거
            if (raw.EndsWith("h", StringComparison.OrdinalIgnoreCase)) raw = raw.Substring(0, raw.Length - 1); // h 접미 제거
            if (raw.Length == 1) raw = "0" + raw;         // 한 자리면 앞에 0 패딩

            if (byte.TryParse(raw, System.Globalization.NumberStyles.HexNumber, null, out var fcHex))
                return fcHex;                              // 16진수 파싱 성공 시 반환
            if (byte.TryParse(raw, out var fcDec))
                return fcDec;                              // 십진수 표기일 수도 있어 처리
            return 0x03;                                   // 실패 시 기본 03h
        }

        private void RefreshDataCount()
        {
            byte fc = GetFunctionCode();                   // 현재 FC
            ushort count = (ushort)numCount.Value;         // 레지스터 개수
            if (fc == 0x10)
                txtDataCount.Text = (count * 2).ToString(); // 10h: 바이트 수 = 레지스터×2
            else if (fc == 0x06)
                txtDataCount.Text = "2";                   // 06h: 고정 2바이트
            else
                txtDataCount.Text = "0";                   // 읽기: 본문 데이터 없음
        }

        // TX 그리드 입력 동기화(HEX<->DEC)
        private void GridTx_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;                    // 헤더 클릭 등 무시
            var row = gridTx.Rows[e.RowIndex];
            if (row.IsNewRow) return;                       // 새 행 무시

            // 0:Register(읽기전용), 1:HEX, 2:DEC
            string hex = Convert.ToString(row.Cells[1].Value) ?? ""; // HEX 칸
            string dec = Convert.ToString(row.Cells[2].Value) ?? ""; // DEC 칸

            // 편집된 쪽 기준으로 다른쪽 갱신
            if (e.ColumnIndex == 1) // HEX 편집
            {
                if (TryParseUShortFromHex(hex, out ushort v))
                    row.Cells[2].Value = v.ToString();     // DEC로 동기화
                else
                    row.Cells[2].Value = "";              // 파싱 실패 시 비움
            }
            else if (e.ColumnIndex == 2) // DEC 편집
            {
                if (ushort.TryParse(dec, out ushort v))
                    row.Cells[1].Value = $"{v:X4}";       // HEX로 동기화(4자리)
                else
                    row.Cells[1].Value = "";              // 파싱 실패 시 비움
            }
        }

        private static bool TryParseUShortFromHex(string s, out ushort value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(s)) return false; // 빈 문자열 처리
            s = s.Trim();
            if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                s = s[2..];                                 // 0x 접두 제거
            if (s.EndsWith("h", StringComparison.OrdinalIgnoreCase))
                s = s[..^1];                                // h 접미 제거
            return ushort.TryParse(s, System.Globalization.NumberStyles.HexNumber, null, out value); // 16진수 파싱
        }

        private ushort[] ReadTxValues(ushort count)
        {
            return Enumerable.Range(0, count).Select(ReadTxValueOrZero).ToArray(); // 0..count-1 각 행의 값 읽기
        }

        private ushort ReadTxValueOrZero(int rowIndex)
        {
            if (rowIndex >= gridTx.Rows.Count) return 0;   // 범위 밖이면 0
            string hex = Convert.ToString(gridTx.Rows[rowIndex].Cells[1].Value) ?? ""; // HEX 칸
            string dec = Convert.ToString(gridTx.Rows[rowIndex].Cells[2].Value) ?? ""; // DEC 칸

            if (!string.IsNullOrWhiteSpace(hex) && TryParseUShortFromHex(hex, out ushort v1))
                return v1;                                  // HEX 우선 파싱
            if (!string.IsNullOrWhiteSpace(dec) && ushort.TryParse(dec.Trim(), out ushort v2))
                return v2;                                  // DEC 파싱
            return 0;                                       // 둘 다 없으면 0
        }

        // ───────────────────────── CRC & Frames ─────────────────────────
        private static ushort Crc16(byte[] data, int len)
        {
            ushort crc = 0xFFFF;                           // 초기값
            for (int i = 0; i < len; i++)
            {
                crc ^= data[i];                            // 바이트 XOR
                for (int b = 0; b < 8; b++)                // 각 비트 처리
                {
                    bool lsb = (crc & 0x0001) != 0;       // LSB 체크
                    crc >>= 1;                            // 우시프트
                    if (lsb) crc ^= 0xA001;               // LSB가 1이면 다항식 XOR
                }
            }
            return crc;                                    // 최종 CRC
        }

        private static byte[] WithCrc(byte[] frame)
        {
            var crc = Crc16(frame, frame.Length);          // CRC 계산
            var arr = new byte[frame.Length + 2];          // CRC 2바이트 추가 길이
            Buffer.BlockCopy(frame, 0, arr, 0, frame.Length); // 본문 복사
            arr[^2] = (byte)(crc & 0xFF);                  // Low 바이트
            arr[^1] = (byte)((crc >> 8) & 0xFF);           // High 바이트
            return arr;                                    // CRC 포함 프레임 반환
        }

        private static byte[] BuildReadFrame(byte slave, byte fc, ushort start, ushort count)
        {
            var raw = new byte[]
            {
                slave, fc,
                (byte)(start >> 8), (byte)(start & 0xFF),
                (byte)(count >> 8), (byte)(count & 0xFF)
            };
            return WithCrc(raw);                            // CRC 붙여 최종 프레임 반환
        }

        private static byte[] BuildWriteSingleFrame(byte slave, ushort addr, ushort value)
        {
            var raw = new byte[]
            {
                slave, 0x06,
                (byte)(addr >> 8), (byte)(addr & 0xFF),
                (byte)(value >> 8), (byte)(value & 0xFF)
            };
            return WithCrc(raw);                            // CRC 포함
        }

        private static byte[] BuildWriteMultipleFrame(byte slave, ushort start, ushort[] values)
        {
            ushort count = (ushort)values.Length;          // 레지스터 개수
            byte byteCount = (byte)(count * 2);            // 데이터 바이트 수

            var raw = new byte[7 + byteCount];             // CRC 제외 본문 배열 생성
            raw[0] = slave;
            raw[1] = 0x10;
            raw[2] = (byte)(start >> 8);
            raw[3] = (byte)(start & 0xFF);
            raw[4] = (byte)(count >> 8);
            raw[5] = (byte)(count & 0xFF);
            raw[6] = byteCount;

            for (int i = 0; i < values.Length; i++)        // 값 채우기(Hi/Lo)
            {
                raw[7 + i * 2] = (byte)(values[i] >> 8);
                raw[8 + i * 2] = (byte)(values[i] & 0xFF);
            }

            return WithCrc(raw);                            // CRC 포함 최종 프레임
        }

        private static ushort[] ParseReadResponse(byte[] resp)
        {
            // resp: [addr][0x03/0x04][byteCount][Hi][Lo]...[crcLo][crcHi]
            if (resp.Length < 5) return Array.Empty<ushort>(); // 최소 길이 검증
            int bc = resp[2];                               // ByteCount
            int n = bc / 2;                                 // 레지스터 개수
            var arr = new ushort[n];
            for (int i = 0; i < n; i++)
            {
                int idx = 3 + i * 2;                       // 데이터 시작 오프셋
                if (idx + 1 >= resp.Length) break;         // 안전 검사
                arr[i] = (ushort)((resp[idx] << 8) | resp[idx + 1]); // Hi/Lo 결합
            }
            return arr;                                     // 값 배열 반환
        }

        private void UpdateReceiveHeader(byte[] resp, byte slave, byte fc, ushort start, ushort count)
        {
            txtRxSlave.Text = slave.ToString();            // 슬레이브 주소 표시
            txtRxFc.Text = $"0x{fc:X2}";                   // FC 16진 표시
            txtRxStart.Text = $"0x{start:X4}";             // 시작 주소 16진 표시
            txtRxCount.Text = count.ToString();             // 개수 표시

            // DataCount/CRC
            if (resp.Length >= 3)
                txtRxDataCount.Text = (fc == 0x03 || fc == 0x04) ? resp[2].ToString() : "0"; // 읽기 응답의 ByteCount
            if (resp.Length >= 2 && resp.Length >= 4)
                txtRxCrc.Text = $"{resp[^2]:X2} {resp[^1]:X2}"; // 응답 CRC(Lo Hi)
        }

        private byte[] SendAndReceive(byte[] request)
        {
            _sp.DiscardInBuffer();                         // 수신 버퍼 비우기(이전 잔여 데이터 제거)
            _sp.Write(request, 0, request.Length);         // 요청 전송

            Thread.Sleep(30);                              // 잠시 대기(슬레이브 응답 준비 시간)

            var buf = new byte[512];                       // 수신 버퍼
            int read = 0;                                  // 읽은 바이트 수

            try { read += _sp.Read(buf, 0, buf.Length); }  // 첫 번째 읽기 시도
            catch (TimeoutException) { }                   // 타임아웃 무시

            Thread.Sleep(30);                              // 추가 대기
            try
            {
                if (_sp.BytesToRead > 0)
                    read += _sp.Read(buf, read, Math.Min(buf.Length - read, _sp.BytesToRead)); // 남은 바이트 추가 읽기
            }
            catch (TimeoutException) { }

            var resp = new byte[read];                     // 실제 읽은 길이만큼 배열 생성
            Buffer.BlockCopy(buf, 0, resp, 0, read);       // 응답 복사
            return resp;                                   // 응답 반환
        }

        private void HexAutoFormat_OnEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            var g = (DataGridView)sender;                  // 편집된 그리드
            if (e.RowIndex < 0) return;                    // 헤더 등 무시

            // 열: 0=Register, 1=HEX, 2=DEC
            if (e.ColumnIndex == 1) // HEX 열 편집 후
            {
                var hexCell = g.Rows[e.RowIndex].Cells[1];
                var decCell = g.Rows[e.RowIndex].Cells[2];

                string raw = Convert.ToString(hexCell.Value) ?? ""; // 입력 원본
                raw = raw.Trim();

                if (raw.Length == 0) { decCell.Value = ""; return; } // 비었으면 DEC도 비움

                // 허용 입력: 0x 접두, h/H 접미, 공백, 콤마 등 → 제거
                raw = raw.TrimEnd('h', 'H');
                if (raw.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) raw = raw.Substring(2);

                // 숫자/영문 A-F 외 문자 제거 (ab -> ab 유지, abcd12 -> abcd12 유지)
                raw = new string(raw.Where(ch =>
                         (ch >= '0' && ch <= '9') ||
                         (ch >= 'a' && ch <= 'f') ||
                         (ch >= 'A' && ch <= 'F')).ToArray());

                if (raw.Length == 0) { hexCell.Value = ""; decCell.Value = ""; return; }

                // 16진수로 파싱 + 4자리 0패딩 + 대문자 + 'h'
                if (ushort.TryParse(raw, System.Globalization.NumberStyles.HexNumber, null, out ushort v))
                {
                    hexCell.Value = $"{v:X4}h";   // 예: 1, 01, 001, ab → 0001h, 00ABh
                    decCell.Value = v.ToString();
                }
                else
                {
                    // 파싱 불가면 빈칸 처리(원하면 입력 유지도 가능)
                    hexCell.Value = "";
                    decCell.Value = "";
                }
            }
            else if (e.ColumnIndex == 2) // DEC 열 편집 후 → HEX 동기화
            {
                var hexCell = g.Rows[e.RowIndex].Cells[1];
                var decCell = g.Rows[e.RowIndex].Cells[2];

                string raw = Convert.ToString(decCell.Value) ?? "";
                raw = raw.Trim();

                if (raw.Length == 0) { hexCell.Value = ""; return; }

                if (ushort.TryParse(raw, out ushort v))
                {
                    hexCell.Value = $"{v:X4}h";            // 4자리 + h 로 정규화
                    decCell.Value = v.ToString();           // DEC도 정규화
                }
                else
                {
                    decCell.Value = "";                    // 파싱 실패 시 비움
                    hexCell.Value = "";
                }
            }
        }

        // ───────────────────────── Recording 구현 ─────────────────────────

        // 체크박스 이벤트
        private void chkRecording_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRecording.Checked)
            {
                int seconds = ParseRecordSeconds();        // 콤보 텍스트에서 초 숫자 추출
                if (seconds <= 0) seconds = 60;            // 잘못된 값이면 기본 60초
                StartRecording(seconds);                    // 녹화 시작
            }
            else
            {
                StopRecording();                            // 녹화 중지
            }
        }

        // "60 sec" / "300 sec" / "120" 등에서 초 추출
        private int ParseRecordSeconds()
        {
            string raw = (cmbRecordEvery?.Text ?? "").Trim(); // 콤보 텍스트
            if (string.IsNullOrEmpty(raw)) return 60;      // 비어있으면 60초

            // 숫자만 뽑기
            string digits = new string(raw.Where(char.IsDigit).ToArray()); // 숫자만 추출
            if (int.TryParse(digits, out int s)) return s; // 숫자로 변환 성공 시

            // 아이템이 정수라면
            if (int.TryParse(raw, out s)) return s;        // 전체가 숫자면 그대로

            return 60;                                     // 실패 시 60초
        }

        private void StartRecording(int seconds)
        {
            try
            {
                // 이미 녹화 중이면 재시작
                StopRecording();                            // 이전 Writer 정리
                string dir = "C:\\Users\\haeul\\source\\repos\\ModbusTester\\Data"; // 저장 폴더(현재는 절대경로)
                string path = Path.Combine(dir, $"modbus_rec_{DateTime.Now:yyyyMMdd_HHmmss}.txt"); // 파일명
                _recWriter = new StreamWriter(path, append: true, Encoding.UTF8); // UTF-8로 append 모드
                _recUntil = DateTime.Now.AddSeconds(seconds); // 종료 목표 시각 설정

                _recTimer.Start();                          // 타이머 가동(주기 체크)
                Log($"[REC] start {seconds}s → {path}");   // 시작 로그
            }
            catch (Exception ex)
            {
                _recWriter = null;                          // 실패 시 Writer 초기화
                Log("[REC] start failed: " + ex.Message);  // 로그 남김
                MessageBox.Show("Recording 시작 실패: " + ex.Message); // 사용자 알림
                if (chkRecording.Checked) chkRecording.Checked = false; // 체크 해제
            }
        }

        private void StopRecording()
        {
            try
            {
                _recTimer.Stop();                           // 타이머 중지

                if (_recWriter != null)
                {
                    _recWriter.Flush();                     // 버퍼 비우기
                    _recWriter.Dispose();                   // 파일 닫기
                    _recWriter = null;                      // 참조 해제
                    Log("[REC] stop");                    // 종료 로그
                }
            }
            catch
            {
                // 무시: 종료 과정의 예외는 앱 동작에 영향 주지 않도록
            }
        }

        // QuickView
        private void ToggleQuickWatch()
        {
            if (_quick == null || _quick.IsDisposed)       // 폼이 없거나 이미 닫힌 경우
            {
                _quick = new FormQuickView();              // 새 인스턴스 생성
                _quick.Show(this);                         // 소유자를 현재 폼으로 설정하여 표시
            }
            else
            {
                _quick.Close();                            // 열려 있으면 닫기(토글)
            }
        }

        private void ApplyComSettingFromUi()
        {
            if (_sp.IsOpen) return; // 열린 상태에서는 속성 건드리지 않음

            _sp.PortName = cmbPort.SelectedItem?.ToString()
                           ?? throw new InvalidOperationException("포트를 선택하세요.");

            _sp.BaudRate = Convert.ToInt32(cmbBaud.SelectedItem);
            _sp.DataBits = Convert.ToInt32(cmbDataBits.SelectedItem);
            _sp.Parity = (Parity)cmbParity.SelectedItem;
            _sp.StopBits = (StopBits)cmbStopBits.SelectedItem;

            _sp.ReadTimeout = 300;
            _sp.WriteTimeout = 300;
        }

        private void btnQuickView_Click(object sender, EventArgs e)
        {
            // 닫혀있다면 UI값을 반영하고 연다
            if (!_sp.IsOpen)
            {
                ApplyComSettingFromUi();
                _sp.Open();
            }

            QuickViewPolling.Run(this, _sp);
        }

    }
}
