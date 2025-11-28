using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ModbusTester.Utils
{
    /// <summary>
    /// 폼의 초기 컨트롤 배치를 기준으로, 폼 크기에 따라
    /// 컨트롤 위치/크기/폰트를 비율로 스케일링해 주는 헬퍼 클래스.
    /// </summary>
    internal sealed class LayoutScaler
    {
        private readonly Form _form;
        private readonly Size _baseClientSize;

        private readonly Dictionary<Control, LayoutInfo> _layoutInfo =
            new Dictionary<Control, LayoutInfo>();

        private readonly struct LayoutInfo
        {
            public LayoutInfo(Rectangle bounds, float fontSize)
            {
                Bounds = bounds;
                FontSize = fontSize;
            }

            public Rectangle Bounds { get; }
            public float FontSize { get; }
        }

        public LayoutScaler(Form form)
        {
            _form = form ?? throw new ArgumentNullException(nameof(form));
            _baseClientSize = form.ClientSize;

            SaveLayoutInfo(form);

            // 폼 크기가 바뀔 때마다 자동 스케일링
            _form.Resize += (_, __) => ScaleLayout();
        }

        /// <summary>
        /// 처음 한 번 원하는 스케일(배율)을 적용.
        /// </summary>
        public void ApplyInitialScale(float scale)
        {
            if (scale <= 0f)
                return;

            _form.ClientSize = new Size(
                (int)(_baseClientSize.Width * scale),
                (int)(_baseClientSize.Height * scale)
            );

            ScaleLayout();
        }

        private void SaveLayoutInfo(Control parent)
        {
            foreach (Control c in parent.Controls)
            {
                _layoutInfo[c] = new LayoutInfo(c.Bounds, c.Font.Size);

                if (c.HasChildren)
                    SaveLayoutInfo(c);
            }
        }

        private void ScaleLayout()
        {
            // 최소화 상태면 스케일링 안 함
            if (_form.WindowState == FormWindowState.Minimized)
                return;

            // 기준 크기 없으면 안 함
            if (_baseClientSize.Width == 0 || _baseClientSize.Height == 0)
                return;

            // 현재 폼 크기가 0이 되면(드래그로 극단적으로 줄였을 때) 안 함
            if (_form.ClientSize.Width == 0 || _form.ClientSize.Height == 0)
                return;

            float sx = (float)_form.ClientSize.Width / _baseClientSize.Width;
            float sy = (float)_form.ClientSize.Height / _baseClientSize.Height;
            float s = Math.Min(sx, sy); // 폰트는 비율 유지

            // 스케일값 자체가 0 이하이면 폰트 크기 0 나와서 터지니까 방어
            if (s <= 0f)
                return;

            foreach (var kv in _layoutInfo)
            {
                Control ctl = kv.Key;
                LayoutInfo info = kv.Value;

                // 컨트롤이 이미 Dispose 된 경우 방어
                if (ctl.IsDisposed)
                    continue;

                ctl.Bounds = new Rectangle(
                    (int)(info.Bounds.X * sx),
                    (int)(info.Bounds.Y * sy),
                    (int)(info.Bounds.Width * sx),
                    (int)(info.Bounds.Height * sy)
                );

                // 폰트 크기도 최소 1은 보장
                float fontSize = info.FontSize * s;
                if (fontSize < 1f)
                    fontSize = 1f;

                ctl.Font = new Font(
                    ctl.Font.FontFamily,
                    fontSize,
                    ctl.Font.Style,
                    GraphicsUnit.Point
                );
            }
        }
    }
}
