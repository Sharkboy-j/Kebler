using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
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

namespace Kebler.UI.Controls
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl, INotifyPropertyChanged
    {
        public About()
        {
            InitializeComponent();
            this.DataContext = this;
            Vers = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public string Vers { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

      

        private void Open(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {e.Uri.AbsoluteUri}") { CreateNoWindow = true });
            e.Handled = true;
        }

    }
}
