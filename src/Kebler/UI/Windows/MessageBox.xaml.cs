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
    public partial class MessageBox : Window
    {
        public MessageBox(UserControl ctr)
        {
            InitializeComponent();
            Data.Content = ctr;
            this.Width = ctr.Width;
            this.Height = ctr.Height;
        }

        public MessageBox(string message, string title = "")
        {
            InitializeComponent();
            var txt = new TextBlock();
            txt.Text = message;
            txt.TextAlignment = TextAlignment.Left;

            this.Title = title;
            Data.Content = txt;
        }
    }
}
