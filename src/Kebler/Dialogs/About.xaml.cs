using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace Kebler.Dialogs
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : INotifyPropertyChanged
    {
        public About(Window owner) : base(owner)
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
