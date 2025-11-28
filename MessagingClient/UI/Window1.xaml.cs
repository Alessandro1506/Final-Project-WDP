//COURSE: PROG2510 WINDOWS DESKTOP PROGRAMMING
//AUTHOR: HALVIN, KEVIN, HELI
//PROJECT: Final Project WDP
//DESCRIPTION: Main window is for the messaging client. It handles
//sending messages, showing chat history, and server IP and Port inputs.

using System;
using System.Windows;
using MessagingClient;



namespace MessagingClient.Views
{
    public partial class Window1 : Window
    {

        public Window1()
        {
            InitializeComponent();
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
            // Here it close main window
            this.Close();
            return;
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            //It reads the input values
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
                // It build message and show it in the list
                string finalMessage;
                finalMessage = "Me to " + targetUser + ": " + messageText;

                MessagesListBox.Items.Add(finalMessage);
                MessageTextBox.Clear();

                // Call networking layer (to be implemented by teammate)
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

            // Read server IP and Port from the UI textboxes.
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
            this.ReceiveMessagesFromServer();
            return;
        }

    }
}
