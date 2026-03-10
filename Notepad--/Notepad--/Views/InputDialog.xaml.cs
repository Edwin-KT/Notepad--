using System.Windows;

namespace Notepad__.Views
{
    public partial class InputDialog : Window
    {
        public string ResponseText => ResponseBox.Text;

        public InputDialog(string message, string defaultValue = "")
        {
            InitializeComponent();
            MessageText.Text = message;
            ResponseBox.Text = defaultValue;
            ResponseBox.SelectAll();
            ResponseBox.Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; 
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; 
        }
    }
}