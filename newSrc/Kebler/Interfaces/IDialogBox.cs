using System.Windows.Controls;

namespace Kebler.Interfaces
{
    public interface IDialogBox
    {
        PasswordBox PasswordBox { get; }

        TextBox TextBox { get; }

        ComboBox Combobox { get; }
    }
}