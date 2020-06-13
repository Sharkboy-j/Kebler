using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Navigation;

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
            DataContext = this;
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
