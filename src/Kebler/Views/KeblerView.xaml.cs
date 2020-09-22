using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Kebler.Models.Torrent.Args;
using Kebler.Services;

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
            (this.DataContext as Kebler.ViewModels.KeblerViewModel).SaveConfig();
        }

        private void CustomizableWindow_Activated(object sender, System.EventArgs e)
        {
            this.ShowInTaskbar = true;
        }

        private void CustomizableWindow_Deactivated(object sender, System.EventArgs e)
        {
            if (this.WindowState == System.Windows.WindowState.Minimized)
                this.ShowInTaskbar = false;
        }
    }
}
