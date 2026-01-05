using System.Windows.Forms;

namespace ModbusTester
{
    partial class FormGridZoom
    {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel layoutRoot;
        private Panel panelTop;
        private Label lblFont;
        private ComboBox cmbFont;
        private DataGridView gridZoom;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            layoutRoot = new TableLayoutPanel();
            panelTop = new Panel();
            lblFont = new Label();
            cmbFont = new ComboBox();
            gridZoom = new DataGridView();

            SuspendLayout();

            // --------------------
            // layoutRoot
            // --------------------
            layoutRoot.ColumnCount = 1;
            layoutRoot.RowCount = 2;
            layoutRoot.Dock = DockStyle.Fill;
            layoutRoot.Margin = new Padding(0);
            layoutRoot.Padding = new Padding(0);
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); // 상단바 높이 고정
            layoutRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // --------------------
            // panelTop
            // --------------------
            panelTop.Dock = DockStyle.Fill;
            panelTop.Margin = new Padding(0);
            panelTop.Padding = new Padding(8, 6, 8, 6);

            lblFont.AutoSize = true;
            lblFont.Text = "Font";
            lblFont.Left = 8;
            lblFont.Top = 10;

            cmbFont.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFont.Width = 80;
            cmbFont.Left = lblFont.Right + 8;
            cmbFont.Top = 6;

            panelTop.Controls.Add(lblFont);
            panelTop.Controls.Add(cmbFont);

            // --------------------
            // gridZoom
            // --------------------
            gridZoom.Dock = DockStyle.Fill;
            gridZoom.Margin = new Padding(0);
            gridZoom.RowHeadersVisible = false;
            gridZoom.AllowUserToAddRows = false;
            gridZoom.AllowUserToDeleteRows = false;
            gridZoom.MultiSelect = false;
            gridZoom.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // --------------------
            // assemble
            // --------------------
            layoutRoot.Controls.Add(panelTop, 0, 0);
            layoutRoot.Controls.Add(gridZoom, 0, 1);

            Controls.Add(layoutRoot);

            // Form
            AutoScaleMode = AutoScaleMode.Dpi;
            Text = "Grid Zoom";
            ClientSize = new System.Drawing.Size(560, 900);
            MinimumSize = new System.Drawing.Size(480, 600);

            ResumeLayout(false);
        }
    }
}
