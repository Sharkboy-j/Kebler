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

namespace Kebler.UI.Dialogs
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class DialogBox
    {
        public string Value;
        readonly bool _isPassword;

        public DialogBox(string message, string boxHint,bool isPassword, MessageBoxDilogButtons buttons = MessageBoxDilogButtons.OkCancel)
        {
            InitializeComponent();
            this._isPassword = isPassword;
            AskDialogText.Text = message;
            if (this._isPassword)
            {
                DialogPasswordBox.Visibility = Visibility.Visible;
                DialogPasswordBox.Password = boxHint;
                DialogPasswordBox.Focus();
            }
            else
            {
                DialogTextBox.Visibility = Visibility.Visible;
                DialogTextBox.Text = boxHint;
                DialogTextBox.Focus();
            }

            ShowButtons(buttons);
        }


        private void ShowButtons(MessageBoxDilogButtons buttons)
        {
            switch (buttons)
            {
                case MessageBoxDilogButtons.Ok:
                    {
                        Ok.Visibility = Visibility.Visible;
                        Ok.IsDefault = true;
                        break;
                    }
                case MessageBoxDilogButtons.OkCancel:
                    {
                        Ok.Visibility = Cancel.Visibility = Visibility.Visible;
                        Ok.IsDefault = true;
                        break;
                    }
                case MessageBoxDilogButtons.YesNo:
                    {
                        Yes.Visibility = No.Visibility = Visibility.Visible;
                        Yes.IsDefault = true;
                        break;
                    }
                default:
                    throw new Exception("If you want show msgBox, use MessageBox control instead");
            }
        }

        private void OkYes_Click(object sender, RoutedEventArgs e)
        {
            if (_isPassword)
            {
                Value = DialogPasswordBox.Password;
                DialogPasswordBox.Clear();
                DialogPasswordBox = null;
            }
            else
                Value = DialogTextBox.Text;
            DialogResult = true;
            Close();
        }

        private void NoCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            DialogPasswordBox.Clear();
            Close();
        }

        public new void Show()
        {
            throw new Exception("You have to call ShowDialog() instead of Show()");
        }

    }
}
