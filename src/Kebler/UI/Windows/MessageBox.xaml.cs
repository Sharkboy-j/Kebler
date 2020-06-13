using System.Windows;
using System.Windows.Controls;
using static Kebler.Models.Enums;

namespace Kebler.UI.Windows
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    public partial class MessageBox
    {
        public bool Result;
        public string Value;
        bool isDialog;
        bool isPassword;

        public MessageBox(bool isDialog, string question, bool isPassword = false, string title = "", MessageBoxDilogButtons buttons = MessageBoxDilogButtons.OkCancel, bool showLogo = true)
        {
            isDialog = true;
            this.isPassword = isPassword;
            this.isDialog = isDialog;
            InitializeComponent();
            Title = title;
            MinWidth = 350;
            TextBlock_Message.Visibility = Visibility.Collapsed;
            AskDialogText.Text = question;
            if (isPassword)
            {
                DialogTextBox.Visibility = Visibility.Collapsed;
                DialogPasswordBox.Visibility = Visibility.Visible;
                DialogPasswordBox.Focus();
            }
            else
            {
                DialogTextBox.Visibility = Visibility.Visible;
                DialogPasswordBox.Visibility = Visibility.Collapsed;
                DialogTextBox.Focus();
            }
            Data.Visibility = Visibility.Collapsed;
            ShowButtons(buttons);
            if (showLogo)
                Logo.Visibility = Visibility.Visible;
        }

        public MessageBox(string message, string title = "", MessageBoxDilogButtons buttons = MessageBoxDilogButtons.Ok, bool showLogo = false, bool isDialog=false)
        {
            InitializeComponent();
            //this.Title = title;
            this.isDialog = isDialog;
            MinWidth = 350;

            TextBlock_Message.Text = message;
            TextBlock_Message.Visibility = Visibility.Visible;
            AskDialogText.Visibility = Visibility.Collapsed;
            //AskDialogText.Text = message;

            if (showLogo)
                Logo.Visibility = Visibility.Visible;

            ShowButtons(buttons);
            Data.Visibility = Visibility.Collapsed;
        }

        private void ShowButtons(MessageBoxDilogButtons buttons)
        {
            switch (buttons)
            {
                case (MessageBoxDilogButtons.Ok):
                    {
                        Ok.Visibility = Visibility.Visible;
                        Ok.IsDefault = true;
                        break;
                    }
                case (MessageBoxDilogButtons.OkCancel):
                    {
                        Ok.Visibility = Cancel.Visibility = Visibility.Visible;
                        Ok.IsDefault = true;
                        break;
                    }
                case (MessageBoxDilogButtons.YesNo):
                    {
                        Yes.Visibility = No.Visibility = Visibility.Visible;
                        Yes.IsDefault = true;
                        break;
                    }
            }
        }

        private void OkYes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            if (isDialog)
                DialogResult = true;
            if (isDialog && isPassword)
            {
                Value = DialogPasswordBox.Password;
                DialogPasswordBox.Clear();
            }
            else
                Value = DialogTextBox.Text;

            Close();
        }

        private void NoCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
            DialogPasswordBox.Clear();
            Close();
        }







        public MessageBox(UserControl obj,bool isDialog=true, string title = "", MessageBoxDilogButtons buttons = MessageBoxDilogButtons.None, bool showLogo = false)
        {
            InitializeComponent();
            Title = title;
            this.isDialog = isDialog;
            if (showLogo)
                Logo.Visibility = Visibility.Visible;
            AskDialogText.Visibility = Visibility.Collapsed;

            ShowButtons(buttons);
            Data.Content = obj;
            MaxWidth = obj.MaxWidth;
            Height = obj.Height;
            Width = obj.Width;
        }

    }
}
