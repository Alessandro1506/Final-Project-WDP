using System;
using System.IO;

namespace MessagingServer
{
    public class Logger
    {
        private readonly String logPath;

        public Logger(String path)
        {
            this.logPath = path;
        }

        public String GetLogPath()
        {
            String path;

            path = this.logPath;
            return path;
        }

        public static void WriteLog(String path, String message)
        {
            StreamWriter writer;
            DateTime now;
            String timeStamp;
            String line;

            writer = null;

            try
            {
                now = DateTime.Now;
                timeStamp = now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                line = timeStamp + " - " + message;

                writer = File.AppendText(path);
                writer.WriteLine(line);
            }
            catch
            {
                // Do not let logging errors crash the service.
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }

            return;
        }
    }
}
