using System;
using System.Windows.Forms;

namespace HostingWindowsForms.Host
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new HostingForm());

        }

        static public HostingForm HostForm { get; set; }
    }
}
