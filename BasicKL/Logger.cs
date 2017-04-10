using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

using log4net;
using log4net.Appender;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace BasicKL
{
    public class Logger
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Logger));

        private string activeWndTitle;
        private Guid guid;

        public Logger(Guid guid)
        {
            this.guid = guid;
            
            log.Info(String.Format("\n{0:d/M/yyyy HH:mm:ss}", DateTime.Now) + " - " + guid.ToString());
            log.Info("\nStarting in "+GetLogPath());
            activeWndTitle = GetActiveWindowTitle();
            log.Info(String.Format("\n\n===\n{0}\n===\n", activeWndTitle));
        }

        public void Log(string msg)
        {
            string curWndTitle = GetActiveWindowTitle();
            if (curWndTitle != activeWndTitle)
            {
                log.Info(String.Format("\n\n===\n{0}\n===\n", curWndTitle));
                activeWndTitle = curWndTitle;
            }

            log.Info(msg);
        }

        public string GetLogPath()
        {
            var root = LogManager.GetRepository()
                .GetAppenders()
                .OfType<FileAppender>()
                .FirstOrDefault();

            //force create file first
            log.Info("");
            return root.File;
        }

        private string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder buf = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, buf, nChars) > 0)
            {
                return buf.ToString();
            }
            return null;
        }
    }
}
