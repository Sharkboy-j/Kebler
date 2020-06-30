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

namespace Kebler.Update
{
    /// <summary>
    /// Interaction logic for EXCEPTIONWINDOW.xaml
    /// </summary>
    public partial class EXCEPTIONWINDOW : Window
    {
        public EXCEPTIONWINDOW(string txt)
        {
            InitializeComponent();

            App.Instance.Log(txt);

            TXT.Text = App.Instance.BUILDER.ToString();
        }
    }
}
