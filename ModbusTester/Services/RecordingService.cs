using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ModbusTester.Services
{
    /// <summary>
    /// 로그를 파일로 녹화하는 역할만 담당하는 서비스.
    /// - 폼(UI)은 이 클래스를 통해서만 파일 기록을 요청한다.
    /// - UI 컨트롤(TextBox 등)은 전혀 모른다.
    /// </summary>
    public class RecordingService : IDisposable
    {
        private readonly string _directory;
        private StreamWriter? _writer;
        private DateTime _until;
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();

        /// <summary>현재 녹화 중인지 여부</summary>
        public bool IsRecording => _writer != null;

        /// <summary>현재 기록 중인 파일 경로 (없으면 null)</summary>
        public string? CurrentFilePath { get; private set; }

        public RecordingService(string directory)
        {
            _directory = directory;
            _timer.Interval = 200;
            _timer.Tick += Timer_Tick;
        }

        /// <summary>
        /// seconds 동안 녹화 시작. (기존이 있으면 정리 후 새로 시작)
        /// </summary>
        public void Start(int seconds)
        {
            Stop();  // 혹시 기존에 열려있으면 정리

            Directory.CreateDirectory(_directory);

            string path = Path.Combine(
                _directory,
                $"modbus_rec_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

            _writer = new StreamWriter(path, append: true, Encoding.UTF8);
            CurrentFilePath = path;

            _until = DateTime.Now.AddSeconds(seconds);
            _timer.Start();
        }

        /// <summary>
        /// 녹화 중지 (파일 닫기)
        /// </summary>
        public void Stop()
        {
            _timer.Stop();

            if (_writer != null)
            {
                try
                {
                    _writer.Flush();
                    _writer.Dispose();
                }
                catch
                {
                }
                finally
                {
                    _writer = null;
                    CurrentFilePath = null;
                }
            }
        }

        /// <summary>
        /// 한 줄 로그를 파일에 남긴다. (녹화 중일 때만)
        /// </summary>
        public void Log(string line)
        {
            if (_writer == null) return;

            try
            {
                _writer.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {line}");
                _writer.Flush();
            }
            catch
            {
                // 파일 오류 등은 조용히 무시
            }
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_writer == null)
            {
                _timer.Stop();
                return;
            }

            if (DateTime.Now >= _until)
            {
                Stop();
            }
        }

        public void Dispose()
        {
            Stop();
            _timer.Tick -= Timer_Tick;
            _timer.Dispose();
        }
    }
}
