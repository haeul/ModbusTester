using System.Drawing;
using System.Windows.Forms;

namespace ModbusTester
{
    partial class ChannelRowControl
    {
        private System.ComponentModel.IContainer components = null;

        private Label lblBadge;
        private Label lblDecCaption;
        private Label lblDecValue;
        private Label lblHexCaption;
        private Label lblHexValue;
        private Label lblBitCaption;
        private Label lblBitValue;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            lblBadge = new Label();
            lblDecCaption = new Label();
            lblDecValue = new Label();
            lblHexCaption = new Label();
            lblHexValue = new Label();
            lblBitCaption = new Label();
            lblBitValue = new Label();

            SuspendLayout();

            // 
            // ChannelRowControl (UserControl 자체)
            // 
            BackColor = Color.FromArgb(30, 30, 30);
            BorderStyle = BorderStyle.FixedSingle;
            Size = new Size(520, 150);
            Margin = new Padding(0, 0, 0, 16);
            // 
            // lblBadge (주소/닉네임)
            // 
            lblBadge.BackColor = Color.FromArgb(60, 60, 60);
            lblBadge.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            lblBadge.ForeColor = Color.White;
            lblBadge.Location = new Point(16, 18);
            lblBadge.Name = "lblBadge";
            lblBadge.Size = new Size(100, 28);
            lblBadge.TabIndex = 0;
            lblBadge.Text = "0000h";
            lblBadge.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // DEC 캡션
            // 
            lblDecCaption.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblDecCaption.ForeColor = Color.Gainsboro;
            lblDecCaption.Location = new Point(140, 20);
            lblDecCaption.Name = "lblDecCaption";
            lblDecCaption.Size = new Size(60, 22);
            lblDecCaption.TabIndex = 1;
            lblDecCaption.Text = "DEC :";
            lblDecCaption.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // DEC 값 (빨간 큰 숫자)
            // 
            lblDecValue.Font = new Font("Consolas", 32F, FontStyle.Bold);
            lblDecValue.ForeColor = Color.FromArgb(225, 26, 26);
            lblDecValue.Location = new Point(130, 40);
            lblDecValue.Name = "lblDecValue";
            lblDecValue.Size = new Size(340, 64);
            lblDecValue.TabIndex = 2;
            lblDecValue.Text = "0";
            lblDecValue.TextAlign = ContentAlignment.MiddleRight;
            lblDecValue.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            // 
            // HEX 캡션
            // 
            lblHexCaption.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblHexCaption.ForeColor = Color.Gainsboro;
            lblHexCaption.Location = new Point(32, 108);
            lblHexCaption.Name = "lblHexCaption";
            lblHexCaption.Size = new Size(50, 22);
            lblHexCaption.TabIndex = 3;
            lblHexCaption.Text = "HEX :";
            lblHexCaption.TextAlign = ContentAlignment.MiddleLeft;
            lblHexCaption.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            // 
            // HEX 값
            // 
            lblHexValue.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblHexValue.ForeColor = Color.Gainsboro;
            lblHexValue.Location = new Point(87, 108);
            lblHexValue.Name = "lblHexValue";
            lblHexValue.Size = new Size(120, 22);
            lblHexValue.TabIndex = 4;
            lblHexValue.Text = "0000h";
            lblHexValue.TextAlign = ContentAlignment.MiddleLeft;
            lblHexValue.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            // 
            // BIT 캡션
            // 
            lblBitCaption.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            lblBitCaption.ForeColor = Color.Gainsboro;
            lblBitCaption.Location = new Point(260, 108);
            lblBitCaption.Name = "lblBitCaption";
            lblBitCaption.Size = new Size(40, 22);
            lblBitCaption.TabIndex = 5;
            lblBitCaption.Text = "BIT :";
            lblBitCaption.TextAlign = ContentAlignment.MiddleLeft;
            lblBitCaption.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            // 
            // BIT 값
            // 
            lblBitValue.Font = new Font("Consolas", 11F, FontStyle.Bold);
            lblBitValue.ForeColor = Color.Gainsboro;
            lblBitValue.Location = new Point(305, 108);
            lblBitValue.Name = "lblBitValue";
            lblBitValue.Size = new Size(200, 22);
            lblBitValue.TabIndex = 6;
            lblBitValue.Text = "0000 0000 0000 0000";
            lblBitValue.TextAlign = ContentAlignment.MiddleLeft;
            lblBitValue.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblBitValue.AutoEllipsis = true;
            // 
            // Controls.Add
            // 
            Controls.Add(lblBadge);
            Controls.Add(lblDecCaption);
            Controls.Add(lblDecValue);
            Controls.Add(lblHexCaption);
            Controls.Add(lblHexValue);
            Controls.Add(lblBitCaption);
            Controls.Add(lblBitValue);

            ResumeLayout(false);
        }

        #endregion
    }
}
