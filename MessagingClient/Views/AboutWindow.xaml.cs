//COURSE: PROG2510 WINDOWS DESKTOP PROGRAMMING
//AUTHOR: HALVIN ALESSANDRO SILVA MAYES
//PROJECT:FINAL PROJECT WDP
//DESCRIPTION: About window that displays general information about
//the project and the student team, with a simple Close button.

using System.Windows;

namespace MessagingClient.Views
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // It close the About window
            this.Close();
            return;
        }
    }
}
