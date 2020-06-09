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

namespace Kebler.UI.Windows
{
    /// <summary>
    /// Interaction logic for MessageBox.xaml
    /// </summary>
    partial class MessageBox
    {
        public MessageBox(UserControl ctr)
        {
            InitializeComponent();
            //Data.Content = ctr;
            //this.Width = ctr.Width;
            //this.Height = ctr.Height;
        }

        public MessageBox(string message, string title = "")
        {
            InitializeComponent();
            //Data.Visibility = Visibility.Collapsed;
            this.Title = title;
            TextBlock_Message.Text = message;

            //this.Height = txt.ActualHeight;

            //this.SizeToContent = SizeToContent.Manual;
        }
    }
}
