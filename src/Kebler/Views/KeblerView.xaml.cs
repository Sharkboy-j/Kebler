using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using Kebler.ViewModels;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
            if (DataContext is KeblerViewModel vm)
            {
                vm.SaveConfig();
            }
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

        private void CustomizableWindow_Closing(object sender, CancelEventArgs e)
        {
            if (ConfigService.Instanse.HideOnClose)
            {
                e.Cancel = true;

                this.Hide();
            }
        }

        private async void CustomizableWindow_Drop(object sender, DragEventArgs e)
        {
            try
            {
                var files = e.Data.GetData(DataFormats.FileDrop) as string[];
                if (DataContext is KeblerViewModel vm)
                {
                    await vm.OpenTorrent(files);
                }
            }
            catch (Exception ex)
            {
                Log.Instance.Error(ex);
            }
        }
    }

    public class GridViewSort
    {
        #region Attached properties

        public static ICommand GetCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(CommandProperty);
        }

        public static void SetCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(CommandProperty, value);
        }

        // Using a DependencyProperty as the backing store for Command.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(GridViewSort),
                new UIPropertyMetadata(
                    null,
                    (o, e) =>
                    {
                        ItemsControl listView = o as ItemsControl;
                        if (listView != null)
                        {
                            if (!GetAutoSort(listView)) // Don't change click handler if AutoSort enabled
                            {
                                if (e.OldValue != null && e.NewValue == null)
                                {
                                    listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                                }
                                if (e.OldValue == null && e.NewValue != null)
                                {
                                    listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                                }
                            }
                        }
                    }
                )
            );

        public static bool GetAutoSort(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoSortProperty);
        }

        public static void SetAutoSort(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoSortProperty, value);
        }

        // Using a DependencyProperty as the backing store for AutoSort.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoSortProperty =
            DependencyProperty.RegisterAttached(
                "AutoSort",
                typeof(bool),
                typeof(GridViewSort),
                new UIPropertyMetadata(
                    false,
                    (o, e) =>
                    {
                        ListView listView = o as ListView;
                        if (listView != null)
                        {
                            if (GetCommand(listView) == null) // Don't change click handler if a command is set
                            {
                                bool oldValue = (bool)e.OldValue;
                                bool newValue = (bool)e.NewValue;
                                if (oldValue && !newValue)
                                {
                                    listView.RemoveHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                                }
                                if (!oldValue && newValue)
                                {
                                    listView.AddHandler(ButtonBase.ClickEvent, new RoutedEventHandler(ColumnHeader_Click));
                                }
                            }
                        }
                    }
                )
            );

        public static string GetPropertyName(DependencyObject obj)
        {
            if (obj is not null)
            {
                return obj.GetValue(PropertyNameProperty)?.ToString() ?? string.Empty;
            }

            return string.Empty;
        }

        public static void SetPropertyName(DependencyObject obj, string value)
        {
            obj.SetValue(PropertyNameProperty, value);
        }

        // Using a DependencyProperty as the backing store for PropertyName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyNameProperty =
            DependencyProperty.RegisterAttached(
                "PropertyName",
                typeof(string),
                typeof(GridViewSort),
                new UIPropertyMetadata(null)
            );

        #endregion

        #region Column header click event handler

        private static void ColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            if (headerClicked != null)
            {
                _e = e;
                _sender = sender;

                var propertyName = GetPropertyName(headerClicked.Column);
                if (!string.IsNullOrEmpty(propertyName))
                {
                    ListView listView = GetAncestor<ListView>(headerClicked);
                    if (listView != null)
                    {
                        ICommand command = GetCommand(listView);
                        if (command != null)
                        {
                            if (command.CanExecute(propertyName))
                            {
                                command.Execute(propertyName);
                            }
                        }
                        else if (GetAutoSort(listView))
                        {
                            ApplySort(listView.Items, propertyName);
                        }
                    }
                }
            }
        }

        private static object _sender;
        private static RoutedEventArgs _e;
        private static ListSortDirection direction = ListSortDirection.Ascending;
        public static void ApplyCashSort()
        {
            if (_e != null)
            {
                GridViewColumnHeader headerClicked = _e.OriginalSource as GridViewColumnHeader;
                if (headerClicked != null)
                {
                    var propertyName = GetPropertyName(headerClicked.Column);
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        ListView listView = GetAncestor<ListView>(headerClicked);
                        if (listView != null)
                        {
                            ICommand command = GetCommand(listView);
                            if (command != null)
                            {
                                if (command.CanExecute(propertyName))
                                {
                                    command.Execute(propertyName);
                                }
                            }
                            else if (GetAutoSort(listView))
                            {
                                ApplySort(listView.Items, propertyName, true);
                            }
                        }
                    }
                }
            }

        }


        #endregion

        #region Helper methods

        public static T GetAncestor<T>(DependencyObject reference) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(reference);
            while (!(parent is T))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }

            return (T)parent;
        }

        public static void ApplySort(ICollectionView view, string propertyName, bool force = false)
        {
            if (view.SortDescriptions.Count > 0)
            {
                SortDescription currentSort = view.SortDescriptions[0];
                if (currentSort.PropertyName == propertyName)
                {
                    if (force == false)
                    {
                        if (currentSort.Direction == ListSortDirection.Ascending)
                            direction = ListSortDirection.Descending;
                        else
                            direction = ListSortDirection.Ascending;
                    }

                }
                view.SortDescriptions.Clear();
            }
            if (!string.IsNullOrEmpty(propertyName))
            {
                view.SortDescriptions.Add(new SortDescription(propertyName, direction));
            }
        }

        #endregion
    }
}