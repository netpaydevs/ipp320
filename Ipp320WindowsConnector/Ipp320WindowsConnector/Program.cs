using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Ipp320WindowsConnector
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
            // Ipp320
            Application.Run(new Form1());
            // New6210
            //Application.Run(new FormNew6210());
        }
    }
}
