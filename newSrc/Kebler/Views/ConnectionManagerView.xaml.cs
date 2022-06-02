using System.Diagnostics.CodeAnalysis;
using System.Windows.Controls;
using Kebler.Interfaces;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Kebler.Views
{
    /// <summary>
    ///     Interaction logic for ConnectionManagerView.xaml
    /// </summary>
    public partial class ConnectionManagerView : IConnectionManager
    {
        public ConnectionManagerView()
        {
            InitializeComponent();
            PasswordBox = ServerPasswordBox;
        }

        public PasswordBox PasswordBox { get; }
    }
}