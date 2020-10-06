using System;
using System.Windows;

namespace Kebler.UI.CSControls.Window
{
    public class DialogWindow : CustomizableWindow
    {
        public DialogWindow(System.Windows.Window owner)
        {
            OverridesDefaultStyle = true;
            Owner = owner;
            ShowHeaderLine = true;
            Style = Application.Current.TryFindResource("DialogWindowStyle") as Style;
        }

        public DialogWindow()
        {
            OverridesDefaultStyle = true;
            ShowHeaderLine = true;
            Style = Application.Current.TryFindResource("DialogWindowStyle") as Style;
        }

        public new void Show()
        {
            throw new Exception("Use ShowDialog instead of Show()");
        }
    }
}