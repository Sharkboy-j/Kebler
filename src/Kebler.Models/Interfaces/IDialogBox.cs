using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace Kebler.Models.Interfaces
{
    public interface IDialogBox
    {
        PasswordBox PWD { get; }
        TextBox TBX { get; }
    }
}
