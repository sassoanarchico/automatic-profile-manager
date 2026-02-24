using System.Windows;

namespace AutomationProfileManager.Views
{
    public partial class TextInputDialog : Window
    {
        public TextInputDialog(string title, string prompt)
        {
            InitializeComponent();
            Title = title;
            PromptLabel.Content = prompt;
        }

        public TextInputDialog(string title, string prompt, string defaultText) : this(title, prompt)
        {
            if (!string.IsNullOrEmpty(defaultText))
            {
                InputTextBox.Text = defaultText;
                InputTextBox.SelectAll();
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public string GetText()
        {
            return InputTextBox.Text;
        }

        public string GetInput()
        {
            return InputTextBox.Text;
        }
    }
}
