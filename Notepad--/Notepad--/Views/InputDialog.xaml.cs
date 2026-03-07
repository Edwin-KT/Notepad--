using System.Windows;

namespace Notepad__.Views
{
    public partial class InputDialog : Window
    {
        // Textul introdus de utilizator
        public string ResponseText => ResponseBox.Text;

        public InputDialog(string message, string defaultValue = "")
        {
            InitializeComponent();
            MessageText.Text = message;
            ResponseBox.Text = defaultValue;
            // Selectam textul implicit ca utilizatorul sa poata scrie direct
            ResponseBox.SelectAll();
            ResponseBox.Focus();
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true; // inchidem cu succes
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // utilizatorul a anulat
        }
    }
}