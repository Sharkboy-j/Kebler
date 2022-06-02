using System.Windows.Controls;
using Kebler.Interfaces;

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
            PasswordBox = DialogPasswordBox;
            TextBox = TBXC;
            Combobox = CBXT;
        }

        public PasswordBox PasswordBox { get; }
        public TextBox TextBox { get; }
        public ComboBox Combobox { get; }
    }
}