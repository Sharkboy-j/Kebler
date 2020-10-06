using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using Kebler.ViewModels;

namespace Kebler.Views
{
    public partial class KeblerView

    {
        public KeblerView()
        {
            InitializeComponent();
        }


        private void DragCompleted(object sender, DragCompletedEventArgs e)
        {
            (DataContext as KeblerViewModel).SaveConfig();
        }

        private void CustomizableWindow_Activated(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
        }

        private void CustomizableWindow_Deactivated(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                ShowInTaskbar = false;
        }
    }
}