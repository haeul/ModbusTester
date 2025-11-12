using System;
using System.IO.Ports;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using WinTimer = System.Windows.Forms.Timer;   // ← Timer 모호성 제거 (WinForms Timer로 고정)

namespace ModbusTester
{
    /// <summary>
    /// - 슬레이브 0x02 대상, FC=03으로 0x0012/0x002A 폴링
    /// - 읽은 값은 RegisterCache에 "가능한 방법으로" 반영 후 Changed 이벤트까지 "가능하면" 발생
    /// - FormQuickView는 RegisterCache.Changed를 구독하므로 자동 갱신
    /// </summary>
    public static class QuickViewPolling
    {
        private const byte SlaveId = 0x02;
        private const ushort Addr12 = 0x0012;
        private const ushort Addr2A = 0x002A;

        private static readonly WinTimer _tmr = new WinTimer();   // WinForms Timer 사용
        private static readonly object _ioLock = new object();

        private static SerialPort _sp;
        private static FormQuickView _qv;
        private static bool _running;

        static QuickViewPolling()
        {
            _tmr.Interval = 200;
            _tmr.Tick += PollTick;
        }

        /// <summary>버튼에서 호출: QuickView 띄우고 폴링 시작</summary>
        public static void Run(Form owner, SerialPort sp)
        {
            if (sp is null) throw new ArgumentNullException(nameof(sp));
            _sp = sp;

            // ★ 포트는 FormMain에서 이미 열어둔 전제
            if (!_sp.IsOpen)
                throw new InvalidOperationException("SerialPort가 닫혀 있습니다. 먼저 FormMain에서 OPEN 후 다시 시도하세요.");

            if (_qv == null || _qv.IsDisposed)
            {
                _qv = new FormQuickView
                {
                    Addr12h = Addr12,
                    Addr2Ah = Addr2A,
                    TopMost = true
                };
                _qv.FormClosed += (_, __) => Stop();
                if (owner != null) _qv.Show(owner); else _qv.Show();
            }

            Start();
        }

        public static void Stop()
        {
            if (_running)
            {
                _tmr.Stop();
                _running = false;
            }
        }

        // ───────────────────────── 내부 ─────────────────────────

        private static void Start()
        {
            if (!_running)
            {
                _tmr.Start();
                _running = true;
            }
        }

        private static void EnsurePortOpen()
        {
            if (_sp.IsOpen) return;

            var ports = SerialPort.GetPortNames();
            if (string.IsNullOrWhiteSpace(_sp.PortName) ||
                Array.IndexOf(ports, _sp.PortName) < 0)
                throw new InvalidOperationException(
                    $"지금 UI에서 선택된 포트({_sp.PortName ?? "NULL"})가 현재 PC에 없습니다.");

            if (_sp.ReadTimeout <= 0) _sp.ReadTimeout = 300;
            if (_sp.WriteTimeout <= 0) _sp.WriteTimeout = 300;
            _sp.Open();
        }


        private static void PollTick(object sender, EventArgs e)
        {
            if (_sp == null || !_sp.IsOpen) return;
            if (!Monitor.TryEnter(_ioLock)) return;
            try
            {
                if (TryReadHoldingRegister(SlaveId, Addr12, out ushort v12))
                    RegisterCache_Update(Addr12, v12);

                if (TryReadHoldingRegister(SlaveId, Addr2A, out ushort v2a))
                    RegisterCache_Update(Addr2A, v2a);
            }
            catch
            {
                // 필요 시 로그
            }
            finally
            {
                Monitor.Exit(_ioLock);
            }
        }

        /// <summary>
        /// FC=03, count=1 전용 간단 리더
        /// 요청: [id][03][addrHi][addrLo][00][01][crcLo][crcHi]
        /// 응답: [id][03][02][Hi][Lo][crcLo][crcHi]
        /// </summary>
        private static bool TryReadHoldingRegister(byte slaveId, ushort address, out ushort value)
        {
            value = 0;

            // 요청 프레임은 Span으로 구성하되, SerialPort.Write는 byte[] 오버로드를 사용
            Span<byte> req = stackalloc byte[8];
            req[0] = slaveId;
            req[1] = 0x03;
            req[2] = (byte)(address >> 8);
            req[3] = (byte)(address & 0xFF);
            req[4] = 0x00; // count hi
            req[5] = 0x01; // count lo

            ushort crc = Crc16(req.Slice(0, 6));
            req[6] = (byte)(crc & 0xFF); // crcLo
            req[7] = (byte)(crc >> 8);   // crcHi

            // SerialPort.Write는 string 또는 byte[] 오버로드만 확실하므로 byte[]로 변환
            byte[] reqArray = req.ToArray();

            byte[] resp = new byte[7];

            _sp.DiscardInBuffer();
            _sp.Write(reqArray, 0, reqArray.Length);   // Span -> byte[] 로 해결

            int read = 0, need = resp.Length;
            int deadline = Environment.TickCount + _sp.ReadTimeout;
            while (read < need)
            {
                int n = _sp.Read(resp, read, need - read);
                if (n <= 0)
                {
                    if (Environment.TickCount > deadline) throw new TimeoutException();
                    continue;
                }
                read += n;
            }

            if (resp[0] != slaveId) return false;
            if (resp[1] != 0x03) return false;
            if (resp[2] != 0x02) return false;

            ushort crcCalc = Crc16(resp.AsSpan(0, 5));
            ushort crcResp = (ushort)(resp[5] | (resp[6] << 8));
            if (crcCalc != crcResp) return false;

            value = (ushort)((resp[3] << 8) | resp[4]);
            return true;
        }

        private static ushort Crc16(ReadOnlySpan<byte> data)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (int b = 0; b < 8; b++)
                {
                    bool lsb = (crc & 0x0001) != 0;
                    crc >>= 1;
                    if (lsb) crc ^= 0xA001;
                }
            }
            return crc;
        }

        /// <summary>
        /// RegisterCache에 값을 "가능한 메서드명"으로 반영하고, Changed 이벤트를 "가능하면" 발생.
        /// - 프로젝트마다 메서드명이 다를 수 있어서 리플렉션으로 범용 처리.
        /// - 지원 시그니처 예: Set(ushort, ushort), Update(ushort, ushort), Put(ushort, ushort), Write(ushort, ushort), SetValue(ushort, ushort), AddOrUpdate(ushort, ushort)
        /// </summary>
        private static void RegisterCache_Update(ushort addr, ushort value)
        {
            var t = typeof(RegisterCache);

            // 쓰기 메서드 탐색 및 호출
            string[] methodNames =
            {
                "Set", "Update", "Put", "Write", "SetValue", "AddOrUpdate"
            };

            foreach (var name in methodNames)
            {
                var m = t.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static, binder: null,
                                    types: new[] { typeof(ushort), typeof(ushort) }, modifiers: null);
                if (m != null)
                {
                    try { m.Invoke(null, new object[] { addr, value }); }
                    catch { /* 무시 */ }
                    goto RAISE_CHANGED;
                }
            }

            // 사전 필드가 노출된 경우 (예: public static IDictionary<ushort, ushort> Values)
            var field = t.GetField("Values", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (field?.GetValue(null) is System.Collections.IDictionary dict)
            {
                try { dict[addr] = value; }
                catch { /* 무시 */ }
            }

        RAISE_CHANGED:
            // Changed 이벤트 발생 시도
            // Raise 메서드가 있으면 호출
            foreach (var raiseName in new[] { "RaiseChanged", "OnChanged", "NotifyChanged" })
            {
                var rm = t.GetMethod(raiseName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (rm != null)
                {
                    try { rm.Invoke(null, null); return; }
                    catch { /* 무시 */ }
                }
            }

            // 이벤트 델리게이트 직접 호출 시도 (Action 또는 EventHandler 가정)
            try
            {
                // 이벤트 메타 얻기
                var ev = t.GetEvent("Changed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (ev != null)
                {
                    // 일반적으로 이벤트의 백킹필드는 "<이름>k__BackingField" 형태거나, 필드 이벤트인 경우 동일명 Field가 존재할 수 있음
                    var fld = t.GetField("Changed", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                              ?? t.GetField("<Changed>k__BackingField", BindingFlags.NonPublic | BindingFlags.Static);
                    if (fld?.GetValue(null) is MulticastDelegate dlg)
                    {
                        var parms = dlg.Method.GetParameters();
                        if (parms.Length == 0)
                            dlg.DynamicInvoke();
                        else if (parms.Length == 2)
                            dlg.DynamicInvoke(null, EventArgs.Empty);
                    }
                }
            }
            catch { /* 이벤트 강제 발생 실패 시 조용히 무시 */ }
        }
    }
}
