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
        public bool doNotScrollOnExpanding;
        private bool _updatesLocked;
        private TreeViewControlItem _previewNodeView;

        public MultiselectionTreeViewItem RootItem
        {
            get => (MultiselectionTreeViewItem)GetValue(RootItemProperty);
            set => SetValue(RootItemProperty, value);
        }

        public new IEnumerable ItemsSource
        {
            get => base.ItemsSource;
            set => throw new NotSupportedException("Use RootItem property instead");
        }

        public bool RememberExpandedItems { get; set; }

        public bool AllowDragDrop { get; set; }

        public string FilterString
        {
            get => _filterString;
            set
            {
                if (_filterString == value)
                    return;
                var num = !RememberExpandedItems || !string.IsNullOrEmpty(_filterString) ? 0 : (!string.IsNullOrEmpty(value) ? 1 : 0);
                var flag = RememberExpandedItems && !string.IsNullOrEmpty(_filterString) && string.IsNullOrEmpty(value);
                _filterString = value;
                if (num != 0)
                    _itemsToExpand = this.GetExpandedItems();
                Refilter();
                if (flag && _itemsToExpand != null)
                {
                    RootItem.CollapseAllChildren();
                    this.SetExpandedItems(_itemsToExpand);
                    _itemsToExpand = null;
                }
                else
                    RootItem.ExpandAllChildren();
            }
        }

        static MultiselectionTreeView()
        {
            VirtualizingStackPanel.VirtualizationModeProperty.OverrideMetadata(typeof(MultiselectionTreeView), new FrameworkPropertyMetadata(VirtualizationMode.Recycling));
        }

        public void Refilter()
        {
            var rootItem = RootItem;
            if (rootItem == null)
                return;
            foreach (var child in rootItem.Children)
                rootItem.ApplyFilterToChild(child, _filterString);
        }

        public void Expand(MultiselectionTreeViewItem node, bool expandChildren)
        {
            node.IsExpanded = true;
            if (!expandChildren)
                return;
            foreach (var child in node.Children)
                Expand(child, true);
        }

        public void SelectAndFocus(MultiselectionTreeViewItem node)
        {
            SelectedItems.Add(node);
            if (!IsFocused)
                return;
            FocusNode(node);
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
            var originalSource = e.OriginalSource as TreeViewControlItem;
            switch (e.Key)
            {
                case Key.Left:
                    if (originalSource != null && ItemsControlFromItemContainer(originalSource) == this)
                    {
                        if (originalSource.Node.IsExpanded)
                            originalSource.Node.IsExpanded = false;
                        else if (originalSource.Node.ParentItem != null)
                            FocusNode(originalSource.Node.ParentItem);
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
            LastClickedItem = this.GetObjectAtPoint<TreeViewControlItem>(e.GetPosition(this)) as MultiselectionTreeViewItem;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            LastClickedItem = this.GetObjectAtPoint<TreeViewControlItem>(e.GetPosition(this)) as MultiselectionTreeViewItem;
            base.OnMouseDoubleClick(e);
            LastClickedItem = null;
        }

        public MultiselectionTreeViewItem LastClickedItem { get; private set; }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property != RootItemProperty)
                return;
            Reload();
        }

        private void Reload()
        {
            if (_flattener != null)
                _flattener.Unmount();
            if (RootItem == null)
                return;
            RootItem.IsExpanded = true;
            _flattener = new FlattenerNode.Flattener(RootItem);
            _flattener.CollectionChanged += new NotifyCollectionChangedEventHandler(_flattener_CollectionChanged);
            base.ItemsSource = _flattener;
        }

        public void FocusNode(MultiselectionTreeViewItem node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            ScrollIntoView(node);
            if (ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
                OnFocusItem(node);
            else
                Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new DispatcherOperationCallback(OnFocusItem), node);
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
            doNotScrollOnExpanding = true;
            foreach (var ancestor in node.Ancestors())
                ancestor.IsExpanded = true;
            doNotScrollOnExpanding = false;
            ScrollIntoView((object)node);
        }

        public IDisposable LockUpdates()
        {
            return new UpdateLock(this);
        }

        private object OnFocusItem(object item)
        {
            if (ItemContainerGenerator.ContainerFromItem(item) is FrameworkElement frameworkElement)
                frameworkElement.Focus();
            return null;
        }

        private void _flattener_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action != NotifyCollectionChangedAction.Remove || Items.Count <= 0)
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
            if (_updatesLocked || multiselectionTreeViewItemList == null)
                return;
            UpdateFocusedNode(SelectedItems.Cast<MultiselectionTreeViewItem>().Except<MultiselectionTreeViewItem>(multiselectionTreeViewItemList).ToList<MultiselectionTreeViewItem>(), Math.Max(0, e.OldStartingIndex - 1));
        }

        private void UpdateFocusedNode(
          List<MultiselectionTreeViewItem> newSelection,
          int topSelectedIndex)
        {
            if (_updatesLocked)
                return;
            SetSelectedItems(newSelection ?? Enumerable.Empty<MultiselectionTreeViewItem>());
            if (SelectedItem != null)
                return;
            SelectedIndex = topSelectedIndex;
        }

        public IEnumerable<MultiselectionTreeViewItem> GetTopLevelSelection()
        {
            var multiselectionTreeViewItems = SelectedItems.OfType<MultiselectionTreeViewItem>();
            var selectionHash = new HashSet<MultiselectionTreeViewItem>(multiselectionTreeViewItems);
            return multiselectionTreeViewItems.Where<MultiselectionTreeViewItem>(item => item.Ancestors().All<MultiselectionTreeViewItem>(a => !selectionHash.Contains(a)));
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            OnDragOver(e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (RootItem == null)
                return;
            e.Handled = true;
            e.Effects = RootItem.GetDropEffect(e, RootItem.Children.Count);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (RootItem == null)
                return;
            e.Handled = true;
            e.Effects = RootItem.GetDropEffect(e, RootItem.Children.Count);
            if (e.Effects == DragDropEffects.None)
                return;
            RootItem.InternalDrop(e, RootItem.Children.Count);
        }

        internal void HandleDragEnter(TreeViewControlItem item, DragEventArgs e)
        {
            HandleDragOver(item, e);
        }

        internal void HandleDragOver(TreeViewControlItem item, DragEventArgs e)
        {
            HidePreview();
            e.Effects = DragDropEffects.None;
            var dropTarget = GetDropTarget(item, e);
            if (dropTarget == null)
                return;
            e.Handled = true;
            e.Effects = dropTarget.Effect;
            ShowPreview(dropTarget.Item);
        }

        internal void HandleDrop(TreeViewControlItem item, DragEventArgs e)
        {
            HidePreview();
            var dropTarget = GetDropTarget(item, e);
            if (dropTarget == null)
                return;
            e.Handled = true;
            e.Effects = dropTarget.Effect;
            dropTarget.Node.InternalDrop(e, dropTarget.Index);
        }

        internal void HandleDragLeave(TreeViewControlItem item, DragEventArgs e)
        {
            HidePreview();
            e.Handled = true;
        }

        private DropTarget GetDropTarget(
          TreeViewControlItem item,
          DragEventArgs e)
        {
            var dropTargetList = BuildDropTargets(item, e);
            var y = e.GetPosition(item).Y;
            foreach (var dropTarget in dropTargetList)
            {
                if (dropTarget.Y >= y)
                    return dropTarget;
            }
            return null;
        }

        private IEnumerable<DropTarget> BuildDropTargets(TreeViewControlItem item, DragEventArgs e)
        {
            var targets = new List<DropTarget>();
            var node = item.Node;
            TryAddDropTarget(targets, item, e);
            var actualHeight = item.ActualHeight;
            var num1 = 0.2 * actualHeight;
            var num2 = actualHeight / 2.0;
            var num3 = actualHeight - num1;
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

        private void TryAddDropTarget(List<DropTarget> targets, TreeViewControlItem item, DragEventArgs e)
        {
            GetNodeAndIndex(item, out var node, out var index);
            if (node == null)
                return;
            var dropEffect = node.GetDropEffect(e, index);
            if (dropEffect == DragDropEffects.None)
                return;
            var dropTarget = new DropTarget()
            {
                Item = item,
                Node = node,
                Index = index,
                Effect = dropEffect
            };
            targets.Add(dropTarget);
        }

        private static void GetNodeAndIndex(TreeViewControlItem item, out MultiselectionTreeViewItem node, out int index)
        {
            node = item.Node;
            index = node.Children.Count;
        }

        private void ShowPreview(TreeViewControlItem item)
        {
            _previewNodeView = item;
            _previewNodeView.Background = Application.Current.TryFindResource("TreeViewItem.SelectedInactive.Background") as Brush;
        }

        private void HidePreview()
        {
            if (_previewNodeView == null)
                return;
            _previewNodeView.ClearValue(BackgroundProperty);
            _previewNodeView = null;
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
            private readonly MultiselectionTreeView _instance;

            public UpdateLock(MultiselectionTreeView instance)
            {
                _instance = instance;
                _instance._updatesLocked = true;
            }

            public void Dispose()
            {
                _instance._updatesLocked = false;
            }
        }
    }
}
