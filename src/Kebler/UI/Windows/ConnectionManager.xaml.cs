using System.ComponentModel;
using System.Windows;

namespace Kebler.UI.Windows
{
    /// <summary>
    /// Interaction logic for ConnectionManager.xaml
    /// </summary>
    public partial class ConnectionManager : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsSelectedServerTabEnabled { get; set; } = false;

        public ConnectionManager()
        {
            InitializeComponent();
            this.DataContext = this;
        }



    }
}
