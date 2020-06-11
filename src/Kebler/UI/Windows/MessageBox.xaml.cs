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
using System.Windows.Shapes;
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
            this.Title = title;
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

            //TextBlock_Message.Text = message;
            AskDialogText.Text = message;

            if (showLogo)
                Logo.Visibility = Visibility.Visible;

            ShowButtons(buttons);
            Data.Visibility = Visibility.Collapsed;
        }

        public MessageBox(UserControl obj, string title = "", MessageBoxDilogButtons buttons = MessageBoxDilogButtons.None, bool showLogo = false)
        {
            InitializeComponent();
            this.Title = title;
            if (showLogo)
                Logo.Visibility = Visibility.Visible;
            AskDialogText.Visibility = Visibility.Collapsed;

            ShowButtons(buttons);
            Data.Content = obj;
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

        public void Show(Window owner)
        {
            this.Owner = owner;
            base.ShowDialog();
        }

        public void ShowDialog(Window owner)
        {
            this.Owner = owner;
            base.ShowDialog();
        }
    }
}
