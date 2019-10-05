using System.Windows;

namespace Kebler.UI.Windows.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogPassword.xaml
    /// </summary>
    public partial class DialogPassword : Window
    {
        public DialogPassword()
        {
            InitializeComponent();
            ResponsePassword.Focus();
        }
        public string ResponseText => ResponsePassword.Password;

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
           //ResponseText = ResponsePassword.Password;
            DialogResult = true;
        }
    }
}
