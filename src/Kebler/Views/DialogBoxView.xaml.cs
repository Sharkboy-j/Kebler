using System;
using System.Windows;
using System.Windows.Controls;
using Kebler.Models.Interfaces;
using static Kebler.Models.Enums;

namespace Kebler.Views
{
    /// <summary>
    /// Interaction logic for DialogBoxView.xaml
    /// </summary>
    public partial class DialogBoxView: IDialogBox
    {
        public DialogBoxView()
        {
            InitializeComponent();
            PWD = this.DialogPasswordBox;
            TBX = this.TBXC;
        }

        public PasswordBox PWD { get; }
        public TextBox TBX { get; }
    }
}
