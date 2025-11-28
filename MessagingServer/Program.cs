// File:        Program.cs
// Authors:     Heli, Halvin, Kevin
// Date:        2025-11-27
// Description: Application entry point for the Windows
//              Service hosting the messaging server.

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
