//COURSE: PROG2510 WINDOWS DESKTOP PROGRAMMING
//AUTHOR: HALVIN, KEVIN, HELI
//PROJECT: Final Project WDP
//DESCRIPTION: Main window is for the messaging client. It handles
//sending messages, showing chat history, and server IP and Port inputs.

using System.Windows;

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

        public void SendMessageToServer(string targetUser, string messageText)
        {
            // Networking will be implemented by the teammate
           
            return;
        }
    }
}
