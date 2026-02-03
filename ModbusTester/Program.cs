using System;
using System.Windows.Forms;
using ModbusTester.Forms;

namespace ModbusTester
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new FormStartMode());
        }
    }
}
