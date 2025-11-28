// FILE          : ClientProgram.cs
// PROJECT       : Messaging Client (TCP)
// PROGRAMMER    : Kevin Napoles Paneque
// FIRST VERSION : 2025-11-27
// DESCRIPTION   :
//   Simple TCP client. Establishes connection to the messaging server,
//   sends a single request, receives the response, then closes.

using System;
using System.Net.Sockets;
using System.Text;

namespace MessagingClient
{
    public class ClientProgram
    {
        private readonly string serverIp;
        private readonly int serverPort;

        // FUNCTION    : ClientProgram
        // DESCRIPTION : Constructor that stores the server IP and port.
        // PARAMETERS  : string serverIpAddress - IP address of the messaging server
        //               int serverPortNumber   - Port number of the messaging server
        // RETURNS     : (none)
        public ClientProgram(string serverIpAddress, int serverPortNumber)
        {
            this.serverIp = serverIpAddress;
            this.serverPort = serverPortNumber;
            return;
        }

        // FUNCTION    : SendAndReceive
        // DESCRIPTION : Connects to the server, sends a request string,
        //               waits for a response, and returns it.
        // PARAMETERS  : string request : Request text to send to server
        // RETURNS     : string         : Response text or error message
        public string SendAndReceive(string request)
        {
            string response;
            TcpClient client;
            NetworkStream networkStream;
            byte[] requestBytes;
            byte[] buffer;
            int bytesRead;
            StringBuilder builder;

            response = string.Empty;
            client = null;
            networkStream = null;
            requestBytes = null;
            buffer = new byte[4096];
            bytesRead = 0;
            builder = new StringBuilder();

            try
            {
                client = new TcpClient();
                client.Connect(this.serverIp, this.serverPort);

                networkStream = client.GetStream();

                // Send the raw request text (no newline needed)
                requestBytes = Encoding.UTF8.GetBytes(request);
                networkStream.Write(requestBytes, 0, requestBytes.Length);

                // Read until the server closes the connection
                do
                {
                    bytesRead = networkStream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        builder.Append(Encoding.UTF8.GetString(buffer, 0, bytesRead));
                    }
                }
                while (bytesRead > 0);

                response = builder.ToString();
            }
            catch (Exception ex)
            {
                response = "ERROR: " + ex.Message;
            }
            finally
            {
                if (networkStream != null)
                {
                    networkStream.Close();
                }

                if (client != null)
                {
                    client.Close();
                }
            }

            return response;
        }
    }
}
