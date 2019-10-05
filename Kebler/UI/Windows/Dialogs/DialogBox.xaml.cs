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
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class DialogBox : Window
    {
        public DialogBox()
        {
            InitializeComponent();
            Question.Focus();
        }

        public DialogBox(string message,string title)
        {
            Title=title;
            InitializeComponent();
            Question.Focus();
            Question.Content= message;
        }

        public bool Response = false;

        private void NoButtonClick(object sender, RoutedEventArgs e)
        {
            //ResponseText = ResponsePassword.Password;
            Response = false;
            DialogResult = true;
        }

        private void YesButtonClick(object sender, RoutedEventArgs e)
        {
            Response=true;
            DialogResult = true;
        }
    }
}
