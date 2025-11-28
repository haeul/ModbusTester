using System;
using System.IO;
using System.Text;

namespace ModbusTester.Services
{
    /// <summary>
    /// 폴링 결과(레지스터 값)를 CSV 파일로 기록하는 서비스.
    /// - 통신 로그(TX/RX HEX)는 다루지 않는다. (FormMain의 txtLog/SaveLog에서 처리)
    /// - UI 컨트롤(TextBox 등)은 전혀 모른다.
    /// </summary>
    public class RecordingService : IDisposable
    {
        private readonly string _directory;
        private StreamWriter? _writer;
        private bool _disposed;

        // 세션 메타데이터
        private byte _slave;
        private byte _functionCode;
        private ushort _startAddress;
        private ushort _registerCount;

        // "Record every N sec" 용
        private int _intervalSeconds;
        private DateTime _lastWriteTime;

        /// <summary>현재 녹화 중인지 여부</summary>
        public bool IsRecording => _writer != null;

        /// <summary>현재 기록 중인 파일 경로 (없으면 null)</summary>
        public string? CurrentFilePath { get; private set; }

        public RecordingService(string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            _directory = directory;
        }

        /// <summary>
        /// 새 Recording 세션 시작.
        /// - 기존 파일이 열려 있으면 닫고 새 파일 생성.
        /// - 헤더(메타 + 주소 라인)를 기록.
        /// </summary>
        /// <param name="slave">슬레이브 ID</param>
        /// <param name="functionCode">FC (03/04 예상)</param>
        /// <param name="startAddress">시작 레지스터 주소</param>
        /// <param name="registerCount">레지스터 개수</param>
        /// <param name="intervalSeconds">몇 초마다 한 줄씩 기록할지 (RecordEvery)</param>
        public void Start(byte slave, byte functionCode, ushort startAddress, ushort registerCount, int intervalSeconds)
        {
            Stop();  // 혹시 기존에 열려있으면 정리

            Directory.CreateDirectory(_directory);

            _slave = slave;
            _functionCode = functionCode;
            _startAddress = startAddress;
            _registerCount = registerCount;
            _intervalSeconds = Math.Max(1, intervalSeconds);
            _lastWriteTime = DateTime.MinValue;

            string path = Path.Combine(
                _directory,
                $"modbus_rec_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

            _writer = new StreamWriter(path, append: false, Encoding.UTF8)
            {
                AutoFlush = true
            };
            CurrentFilePath = path;

            var now = DateTime.Now;

            // 메타 정보
            _writer.WriteLine($"# Recording Start: {now:yyyy-MM-dd HH:mm:ss}");
            _writer.WriteLine($"# Slave={_slave}, FC=0x{_functionCode:X2}, Start=0x{_startAddress:X4}, Count={_registerCount}");

            // 헤더 라인: timestamp,0000h,0001h,...
            const int TimeWidth = 8; // "HH:mm:ss" 길이
            var header = new StringBuilder();
            header.Append("time".PadRight(TimeWidth));   // 첫 컬럼 헤더
            for (int i = 0; i < _registerCount; i++)
            {
                ushort addr = (ushort)(_startAddress + i);
                header.Append(',');
                header.Append($"{addr:X4}h");
            }
            _writer.WriteLine(header.ToString());
        }

        /// <summary>
        /// Polling 결과(레지스터 값)를 전달하면,
        /// 마지막 기록 시점으로부터 intervalSeconds가 지났을 때만 한 줄 기록.
        /// </summary>
        /// <param name="now">현재 시간</param>
        /// <param name="values">Poll 결과 레지스터 값 배열</param>
        public void AppendSnapshotIfDue(DateTime now, ushort[] values)
        {
            if (_writer == null) return;
            if (values == null || values.Length == 0) return;

            if (_lastWriteTime != DateTime.MinValue)
            {
                var delta = now - _lastWriteTime;
                if (delta.TotalSeconds < _intervalSeconds)
                    return;
            }

            int n = Math.Min(values.Length, _registerCount);

            const int TimeWidth = 8;   // HH:mm:ss
            const int ValueWidth = 5;  // 앞에서 맞춰둔 값 폭

            var line = new StringBuilder();

            // ── 시간만 짧게 찍기 ──
            line.Append(now.ToString("HH:mm:ss").PadRight(TimeWidth));

            for (int i = 0; i < _registerCount; i++)
            {
                line.Append(',');
                if (i < n)
                {
                    string formatted = values[i].ToString().PadLeft(ValueWidth);
                    line.Append(formatted);
                }
                else
                {
                    line.Append(new string(' ', ValueWidth));
                }
            }

            _writer.WriteLine(line.ToString());
            _lastWriteTime = now;
        }

        /// <summary>
        /// 현재 Recording 세션 종료 (파일 닫기).
        /// </summary>
        public void Stop()
        {
            if (_writer != null)
            {
                try
                {
                    _writer.Flush();
                    _writer.Dispose();
                }
                catch
                {
                    // 파일 오류는 조용히 무시
                }
                finally
                {
                    _writer = null;
                    CurrentFilePath = null;
                    _lastWriteTime = DateTime.MinValue;
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            Stop();
        }
    }
}
