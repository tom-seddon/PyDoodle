using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using IronPython.Hosting;

namespace PyDoodle
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Dictionary<string, object> options = new Dictionary<string, object>();

            options["Debug"] = true;

            var engine = Python.CreateEngine(options);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(engine));
        }
    }
}
