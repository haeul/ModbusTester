using System.Drawing;
using System.Windows.Forms;

namespace ModbusTester
{
    partial class FormQuickView
    {
        private System.ComponentModel.IContainer components = null;

        private FlowLayoutPanel flowChannels;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            flowChannels = new FlowLayoutPanel();

            SuspendLayout();
            // 
            // flowChannels
            // 
            flowChannels.AutoScroll = true;
            flowChannels.BackColor = Color.FromArgb(18, 18, 18);
            flowChannels.Dock = DockStyle.Fill;
            flowChannels.FlowDirection = FlowDirection.TopDown;
            flowChannels.Location = new Point(0, 0);
            flowChannels.Name = "flowChannels";
            flowChannels.Padding = new Padding(24, 24, 24, 24);
            flowChannels.Size = new Size(600, 520);
            flowChannels.TabIndex = 0;
            flowChannels.WrapContents = false;
            // 
            // FormQuickView
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            BackColor = Color.FromArgb(18, 18, 18);
            ClientSize = new Size(520, 520);
            Controls.Add(flowChannels);
            Font = new Font("Segoe UI", 9F);
            ForeColor = Color.White;
            Name = "FormQuickView";
            StartPosition = FormStartPosition.Manual;
            Text = "Quick View";

            ResumeLayout(false);
        }

        #endregion
    }
}
