using System.Linq;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Kebler.Models.Torrent.Args;
using Kebler.Services;

namespace Kebler.Views
{
    public partial class KeblerView : Kebler.Models.Interfaces.IKebler

    {

        public KeblerView()
        {
            InitializeComponent();
            TorrentsGrid = TorrentsDataGrid;
        }


        private void DragCompleted(object sender, DragCompletedEventArgs e)
        {
            ConfigService.Instanse.CategoriesWidth = CategoriesColumn.ActualWidth;
            ConfigService.Instanse.MoreInfoHeight = MoreInfoColumn.ActualHeight;
            ConfigService.Save();
        }

        public DataGrid TorrentsGrid { get; }
    }
}
