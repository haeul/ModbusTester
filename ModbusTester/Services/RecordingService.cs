using System;
using System.IO;
using System.Text;

namespace ModbusTester.Services
{
    public enum RecordingMode
    {
        Rtu,
        Tcp
    }

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
        private RecordingMode _mode;
        private string _endpoint = "-";
        private byte _slave;
        private byte _functionCode;
        private ushort _startAddress;
        private ushort _registerCount;

        // "Record every N sec" 용
        private int _intervalSeconds;
        private DateTime _lastWriteTime;

        public bool IsRecording => _writer != null;
        public string? CurrentFilePath { get; private set; }

        public RecordingService(string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            _directory = directory;
        }

        /// <summary>
        /// 새 Recording 세션 시작.
        /// </summary>
        public void Start(RecordingMode mode, string? endpoint, byte slave, byte functionCode, ushort startAddress, ushort registerCount, int intervalSeconds)
        {
            Stop();

            Directory.CreateDirectory(_directory);

            _mode = mode;
            _endpoint = string.IsNullOrWhiteSpace(endpoint) ? "-" : endpoint.Trim();
            _slave = slave;
            _functionCode = functionCode;
            _startAddress = startAddress;
            _registerCount = registerCount;
            _intervalSeconds = Math.Max(1, intervalSeconds);
            _lastWriteTime = DateTime.MinValue;

            // 모드별 하위 폴더 분리
            string modeDir = Path.Combine(_directory, mode.ToString().ToUpperInvariant());
            Directory.CreateDirectory(modeDir);

            // 파일명에 endpoint를 넣되 안전하게 sanitize
            string safeEndpoint = SanitizeFileName(_endpoint);
            if (string.IsNullOrEmpty(safeEndpoint)) safeEndpoint = "endpoint";

            string path = Path.Combine(
                modeDir,
                $"modbus_rec_{mode.ToString().ToLowerInvariant()}_{safeEndpoint}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            );

            _writer = new StreamWriter(path, append: false, Encoding.UTF8)
            {
                AutoFlush = true
            };
            CurrentFilePath = path;

            var now = DateTime.Now;

            // 메타 정보
            _writer.WriteLine($"# Recording Start: {now:yyyy-MM-dd HH:mm:ss}");
            _writer.WriteLine($"# Mode={_mode}, Endpoint={_endpoint}");
            _writer.WriteLine($"# Slave/Unit={_slave}, FC=0x{_functionCode:X2}, Start=0x{_startAddress:X4}, Count={_registerCount}");

            // 헤더 라인: timestamp,0000h,0001h,...
            const int TimeWidth = 8; // "HH:mm:ss"
            var header = new StringBuilder();
            header.Append("time".PadRight(TimeWidth));
            for (int i = 0; i < _registerCount; i++)
            {
                ushort addr = (ushort)(_startAddress + i);
                header.Append(',');
                header.Append($"{addr:X4}h");
            }
            _writer.WriteLine(header.ToString());
        }

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
            const int ValueWidth = 5;  // 값 폭

            var line = new StringBuilder();
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

        private static string SanitizeFileName(string s)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sb = new StringBuilder(s.Length);
            foreach (char c in s)
            {
                if (invalid.Contains(c)) continue;

                // 자주 문제되는 문자 추가 처리
                if (c == ':' || c == '/' || c == '\\') { sb.Append('_'); continue; }

                sb.Append(c);
            }

            // 너무 길면 자르기(윈도우 파일명 길이 안전)
            string r = sb.ToString().Trim();
            if (r.Length > 40) r = r.Substring(0, 40);
            return r;
        }
    }
}
