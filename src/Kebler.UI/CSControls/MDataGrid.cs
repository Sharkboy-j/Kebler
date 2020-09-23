using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Kebler.UI.CSControls
{
    public class MDataGrid : System.Windows.Controls.DataGrid
    {
        public MDataGrid()
        {
            SelectionChanged += MDataGrid_SelectionChanged;
        }

        private void MDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.SelectedItemsList = this.SelectedItems;

        }

        #region SelectedItemsList

        public IList SelectedItemsList
        {
            get { return (IList)GetValue(SelectedItemsListProperty); }
            set { SetValue(SelectedItemsListProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsListProperty =
                DependencyProperty.Register(nameof(SelectedItemsList), typeof(IList), typeof(MDataGrid), new FrameworkPropertyMetadata(
            new List<object>(), FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            null,
            null,
            true,
            System.Windows.Data.UpdateSourceTrigger.PropertyChanged));

        #endregion
    }
}
