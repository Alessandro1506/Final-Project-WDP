using System.ServiceProcess;

namespace MessagingServer
{
    internal static class Program
    {
        private static void Main()
        {
            ServiceBase[] servicesToRun;

            servicesToRun = new ServiceBase[]
            {
                new MessageService()
            };

            ServiceBase.Run(servicesToRun);
            return;
        }
    }
}
