using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WorldBuilder
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Config config = new Config("settings.ini");
            frmBuilder.WarFolder = config.GetString("WarPath");

            if (!File.Exists(Path.Combine(frmBuilder.WarFolder, "data.myp")))
            {
                MessageBox.Show("Warhammer folder not found. Update settings.ini.", "Error");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new frmBuilder());
        }
    }
}
