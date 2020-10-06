using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Kebler.UI.CSControls
{
    public class MDataGrid : DataGrid
    {
        public MDataGrid()
        {
            SelectionChanged += MDataGrid_SelectionChanged;
        }

        private void MDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItemsList = SelectedItems;
        }

        #region SelectedItemsList

        public IList SelectedItemsList
        {
            get => (IList) GetValue(SelectedItemsListProperty);
            set => SetValue(SelectedItemsListProperty, value);
        }

        public static readonly DependencyProperty SelectedItemsListProperty =
            DependencyProperty.Register(nameof(SelectedItemsList), typeof(IList), typeof(MDataGrid),
                new FrameworkPropertyMetadata(
                    new List<object>(),
                    FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null,
                    null,
                    true,
                    UpdateSourceTrigger.PropertyChanged));

        #endregion
    }
}