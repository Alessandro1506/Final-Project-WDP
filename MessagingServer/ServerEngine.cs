// File:        ServerEngine.cs
// Authors:     Heli, Halvin, Kevin
// Date:        2025-11-27
// Description: Core TCP server responsible for handling 
//              client connections, processing requests,
//              and delegating message operations.

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MessagingServer
{
    public class ServerEngine
    {
        private readonly TcpListener listener;
        private Boolean isRunning;
        private Thread listenerThread;
        private readonly Logger logger;
        private readonly MessageProcessor messageProcessor;

        public ServerEngine(String ipAddress, Int32 port, String logPath)
        {
            IPAddress ip;
            Int32 listenerPort;

            ip = IPAddress.Parse(ipAddress);
            listenerPort = port;

            this.listener = new TcpListener(ip, listenerPort);
            this.logger = new Logger(logPath);
            this.messageProcessor = new MessageProcessor(this.logger);
            this.isRunning = false;
            this.listenerThread = null;
            return;
        }

        public void Start()
        {
            Thread thread;

            this.listener.Start();
            this.isRunning = true;

            thread = new Thread(new ThreadStart(this.ListenForClients));
            this.listenerThread = thread;
            this.listenerThread.Start();

            Logger.WriteLog(this.logger.GetLogPath(), "ServerEngine started.");
            return;
        }

        public void Stop()
        {
            Boolean threadAlive;

            this.isRunning = false;

            try
            {
                this.listener.Stop();
            }
            catch (SocketException)
            {
                // Ignore stop exception.
            }

            threadAlive = false;

            if (this.listenerThread != null)
            {
                threadAlive = this.listenerThread.IsAlive;
            }

            if (threadAlive)
            {
                this.listenerThread.Join(1000);
            }

            Logger.WriteLog(this.logger.GetLogPath(), "ServerEngine stopped.");
            return;
        }

        private void ListenForClients()
        {
            Boolean keepRunning;

            keepRunning = this.isRunning;

            while (keepRunning)
            {
                TcpClient client;

                try
                {
                    client = this.listener.AcceptTcpClient();
                    this.HandleClient(client);
                }
                catch (SocketException ex)
                {
                    Logger.WriteLog(this.logger.GetLogPath(), "SocketException in ListenForClients: " + ex.Message);
                }

                keepRunning = this.isRunning;
            }

            return;
        }

        private void HandleClient(TcpClient tcpClient)
        {
            NetworkStream clientStream;
            Byte[] buffer;
            Int32 bytesRead;
            String requestText;
            String responseText;
            Byte[] responseBytes;

            clientStream = null;
            buffer = new Byte[4096];
            bytesRead = 0;
            requestText = String.Empty;
            responseText = String.Empty;
            responseBytes = null;

            try
            {
                clientStream = tcpClient.GetStream();
                bytesRead = clientStream.Read(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    requestText = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Logger.WriteLog(this.logger.GetLogPath(), "Received: " + requestText);

                    responseText = this.messageProcessor.ProcessMessage(requestText);
                    responseBytes = Encoding.UTF8.GetBytes(responseText);
                    clientStream.Write(responseBytes, 0, responseBytes.Length);

                    Logger.WriteLog(this.logger.GetLogPath(), "Sent: " + responseText);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog(this.logger.GetLogPath(), "Error handling client communication: " + ex.Message);
            }
            finally
            {
                if (clientStream != null)
                {
                    clientStream.Close();
                }

                tcpClient.Close();
            }

            return;
        }
    }
}
