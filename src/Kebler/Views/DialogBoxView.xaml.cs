using System.Windows.Controls;
using Kebler.Models.Interfaces;

namespace Kebler.Views
{
    /// <summary>
    ///     Interaction logic for DialogBoxView.xaml
    /// </summary>
    public partial class DialogBoxView : IDialogBox
    {
        public DialogBoxView()
        {
            InitializeComponent();
            PWD = DialogPasswordBox;
            TBX = TBXC;
        }

        public PasswordBox PWD { get; }
        public TextBox TBX { get; }
    }
}