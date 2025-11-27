using System;
using System.ServiceProcess;

namespace MessagingServer
{
    public partial class MessageService : ServiceBase
    {
        private ServerEngine serverEngine;

        public MessageService()
        {
            this.InitializeComponent();
        }

        protected override void OnStart(String[] args)
        {
            String ipAddress;
            String portString;
            String logPath;
            Int32 port;

            // Fully qualified name so there is no confusion about ConfigurationManager
            ipAddress = System.Configuration.ConfigurationManager.AppSettings["ServerIP"];
            portString = System.Configuration.ConfigurationManager.AppSettings["ServerPort"];
            logPath = System.Configuration.ConfigurationManager.AppSettings["LogFilePath"];

            port = Int32.Parse(portString);

            this.serverEngine = new ServerEngine(ipAddress, port, logPath);
            this.serverEngine.Start();
            return;
        }

        protected override void OnStop()
        {
            if (this.serverEngine != null)
            {
                this.serverEngine.Stop();
            }

            return;
        }
    }
}
