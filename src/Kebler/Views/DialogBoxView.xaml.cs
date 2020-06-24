using System;
using System.Windows;
using System.Windows.Controls;
using Kebler.Models.Interfaces;
using static Kebler.Models.Enums;

namespace Kebler.Views
{
    /// <summary>
    /// Interaction logic for DialogBoxView.xaml
    /// </summary>
    public partial class DialogBoxView: IDialogBox
    {
        public DialogBoxView()
        {
            InitializeComponent();
            PWD = this.DialogPasswordBox;
        }


        //private void ShowButtons(MessageBoxDilogButtons buttons)
        //{
        //    switch (buttons)
        //    {
        //        case MessageBoxDilogButtons.Ok:
        //            {
        //                Ok.Visibility = Visibility.Visible;
        //                Ok.IsDefault = true;
        //                break;
        //            }
        //        case MessageBoxDilogButtons.OkCancel:
        //            {
        //                Ok.Visibility = Cancel.Visibility = Visibility.Visible;
        //                Ok.IsDefault = true;
        //                break;
        //            }
        //        case MessageBoxDilogButtons.YesNo:
        //            {
        //                Yes.Visibility = No.Visibility = Visibility.Visible;
        //                Yes.IsDefault = true;
        //                break;
        //            }
        //        default:
        //            throw new Exception("If you want show msgBox, use MessageBoxView control instead");
        //    }
        //}

        //private void OkYes_Click(object sender, RoutedEventArgs e)
        //{
        //    if (_isPassword)
        //    {
        //        Value = DialogPasswordBox.Password;
        //        DialogPasswordBox.Clear();
        //        DialogPasswordBox = null;
        //    }
        //    else
        //        Value = DialogTextBox.Text;
        //    DialogResult = true;
        //    Close();
        //}

        //private void NoCancel_Click(object sender, RoutedEventArgs e)
        //{
        //    DialogResult = false;
        //    DialogPasswordBox.Clear();
        //    Close();
        //}


        public PasswordBox PWD { get; }

        
    }
}
