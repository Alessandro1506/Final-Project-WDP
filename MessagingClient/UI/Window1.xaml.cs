//COURSE: PROG2510 WINDOWS DESKTOP PROGRAMMING
//AUTHOR: HALVIN, KEVIN, HELI
//PROJECT: Final Project WDP
//DESCRIPTION: Main window is for the messaging client. It handles
//sending messages, showing chat history, and server IP and Port inputs.

using System;
using System.Threading;
using System.Windows;
using MessagingClient;

namespace MessagingClient.Views
{
    public partial class Window1 : Window
    {
        private bool keepPolling;
        private Thread pollingThread;

        public Window1()
        {
            InitializeComponent();

            keepPolling = true;
            StartPollingThread();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Show the About window as a dialog
            AboutWindow aboutWindow;
            aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
            return;
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Here it closes the main window
            Close();
            return;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // It reads the input values
            string targetUser;
            string messageText;
            bool isTargetEmpty;
            bool isMessageEmpty;

            targetUser = TargetUserTextBox.Text;
            messageText = MessageTextBox.Text;

            // Here it validates input
            isTargetEmpty = string.IsNullOrWhiteSpace(targetUser);
            isMessageEmpty = string.IsNullOrWhiteSpace(messageText);

            if (isTargetEmpty == true || isMessageEmpty == true)
            {
                MessageBox.Show("Please enter a target user and a message.");
            }
            else
            {
                // It builds message and shows it in the list
                string finalMessage;
                finalMessage = "Me to " + targetUser + ": " + messageText;

                MessagesListBox.Items.Add(finalMessage);
                MessageTextBox.Clear();

                // Call networking layer
                SendMessageToServer(targetUser, messageText);
            }

            return;
        }

        // FUNCTION    : SendMessageToServer
        // DESCRIPTION : Reads server IP and Port from the UI, builds a SEND
        //               request for the Windows Service, sends it using the
        //               TCP client, and handles the response.
        // PARAMETERS  : string targetUser   - Recipient user name
        //               string messageText  - Text of the message to send
        // RETURNS     : void
        public void SendMessageToServer(string targetUser, string messageText)
        {
            string serverIp;
            string portText;
            int serverPort;
            bool isPortValid;
            bool hasValidConfig;
            string request;
            string reply;
            ClientProgram client;
            string ackMessage;
            string userName;

            // Read server IP, Port, and user name from the UI textboxes.
            serverIp = ServerIpTextBox.Text;
            portText = ServerPortTextBox.Text;
            userName = UserNameTextBox.Text;

            isPortValid = int.TryParse(portText, out serverPort);
            hasValidConfig = true;

            if (string.IsNullOrWhiteSpace(serverIp) == true || isPortValid == false)
            {
                MessageBox.Show("Please enter a valid server IP and Port.");
                hasValidConfig = false;
            }

            if (string.IsNullOrWhiteSpace(userName) == true)
            {
                MessageBox.Show("Please enter a user name.");
                hasValidConfig = false;
            }

            if (hasValidConfig == true)
            {
                // Create TCP client using the current IP and Port
                client = new ClientProgram(serverIp, serverPort);

                // The server expects: SEND|fromUser|toUser|text
                request = "SEND|" + userName + "|" + targetUser + "|" + messageText;

                reply = client.SendAndReceive(request);

                if (reply != null && reply.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase) == true)
                {
                    MessageBox.Show("Error sending message: " + reply);
                }
                else if (string.IsNullOrWhiteSpace(reply) == false)
                {
                    // Show server acknowledgement as a separate entry
                    ackMessage = "[Server]: " + reply;
                    MessagesListBox.Items.Add(ackMessage);
                }
            }

            return;
        }

        // FUNCTION    : ReceiveMessagesFromServer
        // DESCRIPTION : Sends a PULL request to the server for the local user
        //               and displays any returned messages in the list box.
        // PARAMETERS  : (none)
        // RETURNS     : void
        public void ReceiveMessagesFromServer()
        {
            string serverIp;
            string portText;
            int serverPort;
            bool isPortValid;
            bool hasValidConfig;
            string request;
            string reply;
            ClientProgram client;
            string[] messageBlocks;
            int index;
            string block;
            string[] parts;
            string fromUser;
            string timeStampText;
            string text;
            string displayLine;
            string userName;

            serverIp = ServerIpTextBox.Text;
            portText = ServerPortTextBox.Text;
            userName = UserNameTextBox.Text;

            isPortValid = int.TryParse(portText, out serverPort);
            hasValidConfig = true;

            if (string.IsNullOrWhiteSpace(serverIp) == true || isPortValid == false)
            {
                MessageBox.Show("Please enter a valid server IP and Port.");
                hasValidConfig = false;
            }

            if (string.IsNullOrWhiteSpace(userName) == true)
            {
                MessageBox.Show("Please enter a user name.");
                hasValidConfig = false;
            }

            if (hasValidConfig == true)
            {
                client = new ClientProgram(serverIp, serverPort);

                // The server expects: PULL|userName
                request = "PULL|" + userName;

                reply = client.SendAndReceive(request);

                if (reply != null && reply.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase) == true)
                {
                    MessageBox.Show("Error receiving messages: " + reply);
                }
                else if (string.IsNullOrWhiteSpace(reply) == false && reply != "NOMSG")
                {
                    // The server returns one or more messages in the format:
                    // MSG|FromUser|TimeStamp|Text
                    // Multiple messages are separated with '~'
                    messageBlocks = reply.Split('~');

                    for (index = 0; index < messageBlocks.Length; index++)
                    {
                        block = messageBlocks[index];

                        if (string.IsNullOrWhiteSpace(block) == false)
                        {
                            parts = block.Split('|');

                            if (parts.Length >= 4 && parts[0] == "MSG")
                            {
                                fromUser = parts[1];
                                timeStampText = parts[2];
                                text = parts[3];

                                displayLine = fromUser + " (" + timeStampText + "): " + text;
                                MessagesListBox.Items.Add(displayLine);
                            }
                            else
                            {
                                // Fallback: show raw block if it does not match the expected format
                                MessagesListBox.Items.Add("[Unknown]: " + block);
                            }
                        }
                    }
                }
                else
                {
                    // Optional: uncomment if you want to show when there are no messages
                    // MessagesListBox.Items.Add("[Info]: No new messages.");
                }
            }

            return;
        }

        private void ReceiveButton_Click(object sender, RoutedEventArgs e)
        {
            ReceiveMessagesFromServer();
            return;
        }

        // FUNCTION    : StartPollingThread
        // DESCRIPTION : Creates and starts a background thread that periodically
        //               polls the server for new messages.
        // PARAMETERS  : (none)
        // RETURNS     : void
        private void StartPollingThread()
        {
            pollingThread = new Thread(PollMessagesLoop);
            pollingThread.IsBackground = true;
            pollingThread.Start();
            return;
        }

        // FUNCTION    : PollMessagesLoop
        // DESCRIPTION : Background thread loop that periodically calls
        //               ReceiveMessagesFromServer using the UI dispatcher.
        // PARAMETERS  : (none)
        // RETURNS     : void
        private void PollMessagesLoop()
        {
            while (keepPolling == true)
            {
                try
                {
                    // Marshal the call onto the UI thread so it can safely
                    // access controls like the list box and textboxes.
                    Dispatcher.Invoke(new Action(ReceiveMessagesFromServer));
                }
                catch
                {
                    // Ignore any errors during polling to keep the loop alive.
                }

                Thread.Sleep(5000);   // wait 5 seconds between polls
            }

            return;
        }

        // FUNCTION    : OnClosed
        // DESCRIPTION : Stops the polling loop when the window is closed.
        // PARAMETERS  : EventArgs e - Event arguments
        // RETURNS     : void
        protected override void OnClosed(EventArgs e)
        {
            keepPolling = false;

            if (pollingThread != null && pollingThread.IsAlive == true)
            {
                pollingThread.Join(1000);
            }

            base.OnClosed(e);
            return;
        }
    }
}
