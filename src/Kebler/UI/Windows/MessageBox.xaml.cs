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
    partial class MessageBox
    {
        public bool Result;
    
        public MessageBox(string message, string title = "", MessageBoxDilogButtons buttons = MessageBoxDilogButtons.Ok, bool showLogo = false)
        {
            InitializeComponent();
            this.Title = title;
            TextBlock_Message.Text = message;
            if (showLogo)
                Logo.Visibility = Visibility.Visible;

            switch (buttons)
            {
                case (MessageBoxDilogButtons.Ok):
                    {
                        Ok.Visibility = Visibility.Visible;
                        break;
                    }
                case (MessageBoxDilogButtons.OkCancel):
                    {
                        Ok.Visibility = Cancel.Visibility = Visibility.Visible;
                        break;
                    }
                case (MessageBoxDilogButtons.YesNo):
                    {
                        Yes.Visibility = No.Visibility = Visibility.Visible;
                        break;
                    }
            }
        }

        public MessageBox(UserControl obj, string title = "", MessageBoxDilogButtons buttons = MessageBoxDilogButtons.None, bool showLogo = false)
        {
            InitializeComponent();
            this.Title = title;
            if (showLogo)
                Logo.Visibility = Visibility.Visible;

            switch (buttons)
            {
                case (MessageBoxDilogButtons.Ok):
                    {
                        Ok.Visibility = Visibility.Visible;
                        break;
                    }
                case (MessageBoxDilogButtons.OkCancel):
                    {
                        Ok.Visibility = Cancel.Visibility = Visibility.Visible;
                        break;
                    }
                case (MessageBoxDilogButtons.YesNo):
                    {
                        Yes.Visibility = No.Visibility = Visibility.Visible;
                        break;
                    }
            }
            Data.Content = obj;
        }


        private void OkYes_Click(object sender, RoutedEventArgs e)
        {
            Result = true;
            Close();
        }

        private void NoCancel_Click(object sender, RoutedEventArgs e)
        {
            Result = false;
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
