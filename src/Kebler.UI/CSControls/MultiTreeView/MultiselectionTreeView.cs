using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Kebler.Models.Tree;
using Kebler.UI.CSControls.MuliTreeView;

namespace Kebler.UI.CSControls.MultiTreeView
{
    public class MultiselectionTreeView : ListView
    {
        public static readonly DependencyProperty RootItemProperty = DependencyProperty.Register(nameof(RootItem), typeof(MultiselectionTreeViewItem), typeof(MultiselectionTreeView));
        private ExpandedTreeViewElement _itemsToExpand;
        private string _filterString;
        private FlattenerNode.Flattener _flattener;
        private bool doNotScrollOnExpanding;
        private bool _updatesLocked;
        private TreeViewControlItem _previewNodeView;

        public MultiselectionTreeViewItem RootItem
        {
            get
            {
                return (MultiselectionTreeViewItem)this.GetValue(RootItemProperty);
            }
            set
            {
                this.SetValue(RootItemProperty, value);
            }
        }

        public new IEnumerable ItemsSource
        {
            get
            {
                return base.ItemsSource;
            }
            set
            {
                throw new NotSupportedException("Use RootItem property instead");
            }
        }

        public bool RememberExpandedItems { get; set; }

        public bool AllowDragDrop { get; set; }

        public string FilterString
        {
            get
            {
                return this._filterString;
            }
            set
            {
                if (!(this._filterString != value))
                    return;
                int num = !this.RememberExpandedItems || !string.IsNullOrEmpty(this._filterString) ? 0 : (!string.IsNullOrEmpty(value) ? 1 : 0);
                bool flag = this.RememberExpandedItems && !string.IsNullOrEmpty(this._filterString) && string.IsNullOrEmpty(value);
                this._filterString = value;
                if (num != 0)
                    this._itemsToExpand = this.GetExpandedItems();
                this.Refilter();
                if (flag && this._itemsToExpand != null)
                {
                    this.RootItem.CollapseAllChildren();
                    this.SetExpandedItems(this._itemsToExpand);
                    this._itemsToExpand = null;
                }
                else
                    this.RootItem.ExpandAllChildren();
            }
        }

        static MultiselectionTreeView()
        {
            VirtualizingStackPanel.VirtualizationModeProperty.OverrideMetadata(typeof(MultiselectionTreeView), new FrameworkPropertyMetadata(VirtualizationMode.Recycling));
        }

        public void Refilter()
        {
            MultiselectionTreeViewItem rootItem = this.RootItem;
            if (rootItem == null)
                return;
            foreach (MultiselectionTreeViewItem child in rootItem.Children)
                rootItem.ApplyFilterToChild(child, this._filterString);
        }

        public void Expand(MultiselectionTreeViewItem node, bool expandChildren)
        {
            node.IsExpanded = true;
            if (!expandChildren)
                return;
            foreach (MultiselectionTreeViewItem child in node.Children)
                this.Expand(child, true);
        }

        public void SelectAndFocus(MultiselectionTreeViewItem node)
        {
            this.SelectedItems.Add(node);
            if (!this.IsFocused)
                return;
            this.FocusNode(node);
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new TreeViewControlItem();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is TreeViewControlItem;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);
            (element as TreeViewControlItem).ParentTreeView = this;
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            foreach (MultiselectionTreeViewItem removedItem in e.RemovedItems)
                removedItem.IsSelected = false;
            foreach (MultiselectionTreeViewItem addedItem in e.AddedItems)
                addedItem.IsSelected = true;
            base.OnSelectionChanged(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            TreeViewControlItem originalSource = e.OriginalSource as TreeViewControlItem;
            switch (e.Key)
            {
                case Key.Left:
                    if (originalSource != null && ItemsControlFromItemContainer(originalSource) == this)
                    {
                        if (originalSource.Node.IsExpanded)
                            originalSource.Node.IsExpanded = false;
                        else if (originalSource.Node.ParentItem != null)
                            this.FocusNode(originalSource.Node.ParentItem);
                        e.Handled = true;
                        break;
                    }
                    break;
                case Key.Right:
                    if (originalSource != null && ItemsControlFromItemContainer(originalSource) == this)
                    {
                        if (!originalSource.Node.IsExpanded && originalSource.Node.ShowExpander)
                            originalSource.Node.IsExpanded = true;
                        else if (originalSource.Node.Children.Count > 0)
                            originalSource.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                        e.Handled = true;
                        break;
                    }
                    break;
            }
            if (e.Handled)
                return;
            base.OnKeyDown(e);
        }

        protected override void OnPreviewMouseRightButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseRightButtonDown(e);
            this.LastClickedItem = this.GetObjectAtPoint<TreeViewControlItem>(e.GetPosition(this)) as MultiselectionTreeViewItem;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            this.LastClickedItem = this.GetObjectAtPoint<TreeViewControlItem>(e.GetPosition(this)) as MultiselectionTreeViewItem;
            base.OnMouseDoubleClick(e);
            this.LastClickedItem = null;
        }

        public MultiselectionTreeViewItem LastClickedItem { get; private set; }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property != RootItemProperty)
                return;
            this.Reload();
        }

        private void Reload()
        {
            if (this._flattener != null)
                this._flattener.Unmount();
            if (this.RootItem == null)
                return;
            this.RootItem.IsExpanded = true;
            this._flattener = new FlattenerNode.Flattener(RootItem);
            this._flattener.CollectionChanged += new NotifyCollectionChangedEventHandler(this._flattener_CollectionChanged);
            base.ItemsSource = _flattener;
        }

        public void FocusNode(MultiselectionTreeViewItem node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            this.ScrollIntoView(node);
            if (this.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                this.OnFocusItem(node);
            else
                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(this.OnFocusItem), node);
        }

        public void HandleExpanding(MultiselectionTreeViewItem node)
        {
            //if (this.doNotScrollOnExpanding)
            //    return;
            //MultiselectionTreeViewItem multiselectionTreeViewItem1 = node;
            //while (true)
            //{
            //    MultiselectionTreeViewItemCollection children = multiselectionTreeViewItem1.Children;
            //    // ISSUE: reference to a compiler-generated field
            //    //Func<MultiselectionTreeViewItem, bool> func = MultiselectionTreeView.Expand;
            //    //if (func == null)
            //        goto label_6;
            //    //else
            //    //    goto label_10;
            //    //label_4:
            //    //Func<MultiselectionTreeViewItem, bool> predicate;
            //    //MultiselectionTreeViewItem multiselectionTreeViewItem2 = children.LastOrDefault<MultiselectionTreeViewItem>(predicate);
            //    //if (multiselectionTreeViewItem2 != null)
            //    //{
            //    //    multiselectionTreeViewItem1 = multiselectionTreeViewItem2;
            //    //    continue;
            //    //}
            //    //break;
            //    //label_10:
            //    ////predicate = func;
            //    //goto label_4;
            //    label_6:
            //    // ISSUE: reference to a compiler-generated field
            //   // MultiselectionTreeView.\u003C\u003Ec.\u003C\u003E9__41_1 = predicate = (Func<MultiselectionTreeViewItem, bool>)(c => c.IsVisible);
            //    goto label_4;
            //}
            //if (multiselectionTreeViewItem1 == node)
            //    return;
            //this.ScrollIntoView((object)multiselectionTreeViewItem1);
            //this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (Delegate)(() => this.ScrollIntoView((object)node)));
        }

        public void ScrollIntoView(MultiselectionTreeViewItem node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            this.doNotScrollOnExpanding = true;
            foreach (MultiselectionTreeViewItem ancestor in node.Ancestors())
                ancestor.IsExpanded = true;
            this.doNotScrollOnExpanding = false;
            this.ScrollIntoView((object)node);
        }

        public IDisposable LockUpdates()
        {
            return new MultiselectionTreeView.UpdateLock(this);
        }

        private object OnFocusItem(object item)
        {
            if (this.ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement frameworkElement)
                frameworkElement.Focus();
            return null;
        }

        private void _flattener_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Remove || this.Items.Count <= 0)
                return;
            List<MultiselectionTreeViewItem> multiselectionTreeViewItemList = null;
            foreach (MultiselectionTreeViewItem oldItem in e.OldItems)
            {
                if (oldItem.IsSelected)
                {
                    if (multiselectionTreeViewItemList == null)
                        multiselectionTreeViewItemList = new List<MultiselectionTreeViewItem>();
                    multiselectionTreeViewItemList.Add(oldItem);
                }
            }
            if (this._updatesLocked || multiselectionTreeViewItemList == null)
                return;
            this.UpdateFocusedNode(this.SelectedItems.Cast<MultiselectionTreeViewItem>().Except<MultiselectionTreeViewItem>(multiselectionTreeViewItemList).ToList<MultiselectionTreeViewItem>(), Math.Max(0, e.OldStartingIndex - 1));
        }

        private void UpdateFocusedNode(
          List<MultiselectionTreeViewItem> newSelection,
          int topSelectedIndex)
        {
            if (this._updatesLocked)
                return;
            this.SetSelectedItems(newSelection ?? Enumerable.Empty<MultiselectionTreeViewItem>());
            if (this.SelectedItem != null)
                return;
            this.SelectedIndex = topSelectedIndex;
        }

        public IEnumerable<MultiselectionTreeViewItem> GetTopLevelSelection()
        {
            IEnumerable<MultiselectionTreeViewItem> multiselectionTreeViewItems = this.SelectedItems.OfType<MultiselectionTreeViewItem>();
            HashSet<MultiselectionTreeViewItem> selectionHash = new HashSet<MultiselectionTreeViewItem>(multiselectionTreeViewItems);
            return multiselectionTreeViewItems.Where<MultiselectionTreeViewItem>(item => item.Ancestors().All<MultiselectionTreeViewItem>(a => !selectionHash.Contains(a)));
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            this.OnDragOver(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (this.RootItem == null)
                return;
            e.Handled = true;
            e.Effects = this.RootItem.GetDropEffect(e, this.RootItem.Children.Count);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (this.RootItem == null)
                return;
            e.Handled = true;
            e.Effects = this.RootItem.GetDropEffect(e, this.RootItem.Children.Count);
            if (e.Effects == DragDropEffects.None)
                return;
            this.RootItem.InternalDrop(e, this.RootItem.Children.Count);
        }

        internal void HandleDragEnter(TreeViewControlItem item, DragEventArgs e)
        {
            this.HandleDragOver(item, e);
        }

        internal void HandleDragOver(TreeViewControlItem item, DragEventArgs e)
        {
            this.HidePreview();
            e.Effects = DragDropEffects.None;
            MultiselectionTreeView.DropTarget dropTarget = this.GetDropTarget(item, e);
            if (dropTarget == null)
                return;
            e.Handled = true;
            e.Effects = dropTarget.Effect;
            this.ShowPreview(dropTarget.Item);
        }

        internal void HandleDrop(TreeViewControlItem item, DragEventArgs e)
        {
            try
            {
                this.HidePreview();
                MultiselectionTreeView.DropTarget dropTarget = this.GetDropTarget(item, e);
                if (dropTarget == null)
                    return;
                e.Handled = true;
                e.Effects = dropTarget.Effect;
                dropTarget.Node.InternalDrop(e, dropTarget.Index);
            }
            catch (Exception ex)
            {
               // MultiselectionTreeView.Log.Debug(ex.ToString());
                throw;
            }
        }

        internal void HandleDragLeave(TreeViewControlItem item, DragEventArgs e)
        {
            this.HidePreview();
            e.Handled = true;
        }

        private MultiselectionTreeView.DropTarget GetDropTarget(
          TreeViewControlItem item,
          DragEventArgs e)
        {
            List<MultiselectionTreeView.DropTarget> dropTargetList = this.BuildDropTargets(item, e);
            double y = e.GetPosition(item).Y;
            foreach (MultiselectionTreeView.DropTarget dropTarget in dropTargetList)
            {
                if (dropTarget.Y >= y)
                    return dropTarget;
            }
            return null;
        }

        private List<MultiselectionTreeView.DropTarget> BuildDropTargets(
          TreeViewControlItem item,
          DragEventArgs e)
        {
            List<MultiselectionTreeView.DropTarget> targets = new List<MultiselectionTreeView.DropTarget>();
            MultiselectionTreeViewItem node = item.Node;
            this.TryAddDropTarget(targets, item, e);
            double actualHeight = item.ActualHeight;
            double num1 = 0.2 * actualHeight;
            double num2 = actualHeight / 2.0;
            double num3 = actualHeight - num1;
            if (targets.Count == 2)
                targets[0].Y = num2;
            else if (targets.Count == 3)
            {
                targets[0].Y = num1;
                targets[1].Y = num3;
            }
            if (targets.Count > 0)
                targets[targets.Count - 1].Y = actualHeight;
            return targets;
        }

        private void TryAddDropTarget(
          List<MultiselectionTreeView.DropTarget> targets,
          TreeViewControlItem item,
          DragEventArgs e)
        {
            MultiselectionTreeViewItem node;
            int index;
            this.GetNodeAndIndex(item, out node, out index);
            if (node == null)
                return;
            DragDropEffects dropEffect = node.GetDropEffect(e, index);
            if (dropEffect == DragDropEffects.None)
                return;
            MultiselectionTreeView.DropTarget dropTarget = new MultiselectionTreeView.DropTarget()
            {
                Item = item,
                Node = node,
                Index = index,
                Effect = dropEffect
            };
            targets.Add(dropTarget);
        }

        private void GetNodeAndIndex(
          TreeViewControlItem item,
          out MultiselectionTreeViewItem node,
          out int index)
        {
            node = null;
            index = 0;
            node = item.Node;
            index = node.Children.Count;
        }

        private void ShowPreview(TreeViewControlItem item)
        {
            this._previewNodeView = item;
            this._previewNodeView.Background = Application.Current.TryFindResource("TreeViewItem.SelectedInactive.Background") as Brush;
        }

        private void HidePreview()
        {
            if (this._previewNodeView == null)
                return;
            this._previewNodeView.ClearValue(BackgroundProperty);
            this._previewNodeView = null;
        }

        private class DropTarget
        {
            public TreeViewControlItem Item;
            public double Y;
            public MultiselectionTreeViewItem Node;
            public int Index;
            public DragDropEffects Effect;
        }

        private class UpdateLock : IDisposable
        {
            private MultiselectionTreeView _instance;

            public UpdateLock(MultiselectionTreeView instance)
            {
                this._instance = instance;
                this._instance._updatesLocked = true;
            }

            public void Dispose()
            {
                this._instance._updatesLocked = false;
            }
        }
    }
}
