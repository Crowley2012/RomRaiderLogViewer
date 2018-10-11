using System;
using System.Windows.Forms;

namespace RomRaiderLogViewer
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args"></param>
        [STAThread]
        private static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (args.Length > 0)
                Application.Run(new MainForm(args[0]));
            else
                Application.Run(new MainForm());
        }
    }
}