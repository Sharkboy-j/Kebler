using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using Kebler.Models.Interfaces;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Kebler.Views
{
    /// <summary>
    /// Interaction logic for ConnectionManagerView.xaml
    /// </summary>
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public partial class ConnectionManagerView : IConnectionManager
    {


        public PasswordBox pwd { get; }
    
        public ConnectionManagerView()
        {
            InitializeComponent();
            pwd = this.ServerPasswordBox;
        }


    }
}
