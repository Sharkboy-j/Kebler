using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kebler.UI.Windows.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogTextBox.xaml
    /// </summary>
    public partial class DialogTextBox : Window
    {
        public DialogTextBox()
        {
            InitializeComponent();
        }

        public DialogTextBox(string text, string title)
        {

            InitializeComponent();
            Title = title;
            Question.Text = text;
            Okay.IsDefault = true;
            Question.Focus();
        }

        public string Response = "";

        private void NoButtonClick(object sender, RoutedEventArgs e)
        {
            //ResponseText = ResponsePassword.Password;
            DialogResult = false;
        }

        private void Okay_OnClick(object sender, RoutedEventArgs e)
        {
            Response = Question.Text;
            DialogResult = true;
        }
    }
}
