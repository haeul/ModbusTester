using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace ModbusTester
{
    public partial class FormQuickView : Form
    {
        // RX Grid 에서 넘어온 QuickView 대상 (주소, 라벨)
        private readonly List<(ushort addr, string label)> _targets;

        // 디지털 숫자용 폰트 (옵션)
        private PrivateFontCollection? _pfc;
        private Font? _valueFont;

        // UserControl 행 목록
        private readonly List<ChannelRowControl> _rows = new List<ChannelRowControl>();

        public FormQuickView(List<(ushort addr, string label)> targets)
        {
            _targets = targets?.ToList() ?? new List<(ushort addr, string label)>();

            InitializeComponent();

            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            LoadDigitalFont();

            BuildRows();

            Shown += (_, __) =>
            {
                AdjustFormHeightToContent();
                RefreshValues();
            };

            Resize += (_, __) =>
            {
                AdjustRowWidths();
            };

            RegisterCache.Changed += OnCacheChanged;
            FormClosed += (_, __) => RegisterCache.Changed -= OnCacheChanged;
        }

        /// <summary>
        /// 이미 떠 있는 QuickView 창에 대해, 표시할 타깃 목록을 갱신할 때 사용.
        /// (FormMain에서 QV 체크 변경 후 호출)
        /// </summary>
        public void UpdateTargets(List<(ushort addr, string label)> targets)
        {
            _targets.Clear();
            if (targets != null && targets.Count > 0)
                _targets.AddRange(targets);

            BuildRows();
            RefreshValues();
        }

        // ───────────────────────── 행(UserControl) 구성 ─────────────────────────

        private void BuildRows()
        {
            flowChannels.SuspendLayout();
            try
            {
                flowChannels.Controls.Clear();
                _rows.Clear();

                if (_targets.Count == 0)
                {
                    AdjustFormHeightToContent();
                    return;
                }

                foreach (var (addr, label) in _targets)
                {
                    var row = new ChannelRowControl
                    {
                        Address = addr,
                        LabelText = label
                    };

                    if (_valueFont != null)
                        row.SetValueFont(_valueFont);

                    // FlowLayoutPanel 안에서 폭 맞춰주기
                    int innerWidth = flowChannels.ClientSize.Width
                                     - flowChannels.Padding.Left
                                     - flowChannels.Padding.Right;
                    if (innerWidth < 100) innerWidth = 100;
                    row.Width = innerWidth;

                    _rows.Add(row);
                    flowChannels.Controls.Add(row);
                }
            }
            finally
            {
                flowChannels.ResumeLayout();
            }

            AdjustFormHeightToContent();
        }

        private void AdjustRowWidths()
        {
            int innerWidth = flowChannels.ClientSize.Width
                             - flowChannels.Padding.Left
                             - flowChannels.Padding.Right;
            if (innerWidth < 100) innerWidth = 100;

            foreach (Control c in flowChannels.Controls)
            {
                c.Width = innerWidth;
            }
        }

        /// <summary>
        /// 현재 flowChannels 안에 있는 채널 패널 개수에 맞춰
        /// 폼 높이를 자동으로 조절한다.
        /// </summary>
        private void AdjustFormHeightToContent()
        {
            if (flowChannels == null)
                return;

            int contentHeight = flowChannels.Padding.Top + flowChannels.Padding.Bottom;

            foreach (Control c in flowChannels.Controls)
            {
                if (!c.Visible) continue;
                contentHeight += c.Height + c.Margin.Top + c.Margin.Bottom;
            }

            if (contentHeight <= 0)
                contentHeight = 100;

            var workArea = Screen.FromControl(this).WorkingArea;

            int minClientHeight = 150;
            int maxClientHeight = workArea.Height - 80;

            int desiredClientHeight = contentHeight;

            bool needScroll = desiredClientHeight > maxClientHeight;
            if (needScroll)
                desiredClientHeight = maxClientHeight;

            desiredClientHeight = Math.Max(minClientHeight, desiredClientHeight);

            flowChannels.AutoScroll = needScroll;

            if (needScroll)
            {
                int extraBottom = 5;
                flowChannels.AutoScrollMinSize = new Size(
                    0,
                    contentHeight + extraBottom
                );
            }
            else
            {
                flowChannels.AutoScrollMinSize = Size.Empty;
            }

            int chrome = Height - ClientSize.Height;
            Height = desiredClientHeight + chrome;

            AdjustRowWidths();
            flowChannels.PerformLayout();
        }

        // ───────────────────────── 디지털 폰트 로드 ─────────────────────────

        private void LoadDigitalFont()
        {
            try
            {
                string fontDir = Path.Combine(Application.StartupPath, "Fonts");
                string fontPath = Path.Combine(fontDir, "dseg7-classic-latin-700-normal.ttf");

                if (!File.Exists(fontPath))
                {
                    _valueFont = new Font("Consolas", 28F, FontStyle.Bold, GraphicsUnit.Point);
                    return;
                }

                _pfc = new PrivateFontCollection();
                _pfc.AddFontFile(fontPath);

                if (_pfc.Families.Length == 0)
                {
                    _valueFont = new Font("Consolas", 28F, FontStyle.Bold, GraphicsUnit.Point);
                    return;
                }

                FontFamily ff = _pfc.Families[0];
                _valueFont = new Font(ff, 30F, FontStyle.Regular, GraphicsUnit.Point);
            }
            catch
            {
                _valueFont = new Font("Consolas", 28F, FontStyle.Bold, GraphicsUnit.Point);
            }
        }

        // ───────────────────────── RegisterCache 변경 이벤트 ─────────────────────────

        private void OnCacheChanged()
        {
            if (!IsHandleCreated || IsDisposed)
                return;

            try
            {
                BeginInvoke((Action)RefreshValues);
            }
            catch (ObjectDisposedException)
            {
                // 폼 닫히는 중이면 무시
            }
        }

        private void RefreshValues()
        {
            foreach (var row in _rows)
            {
                if (RegisterCache.TryGet(row.Address, out var raw))
                    row.UpdateFromRaw(raw);
                else
                    row.ClearValues();
            }
        }
    }
}
