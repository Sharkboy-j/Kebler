using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Kebler.UI.Controls
{
    public class DialogWindow : CustomWindow
    {
        //public DialogWindow(System.Windows.Window owner)
        //{
        //    OverridesDefaultStyle = true;
        //    Owner = owner;
        //    ShowHeaderLine = true;
        //}

        public DialogWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
        }

        static DialogWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DialogWindow),
                new FrameworkPropertyMetadata(typeof(DialogWindow)));
        }
    }
}
