using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Kebler.Models.Tree
{
    public class MultiselectionTreeViewItemCollection : IList<MultiselectionTreeViewItem>, INotifyCollectionChanged
    {
        private List<MultiselectionTreeViewItem> _items = new List<MultiselectionTreeViewItem>();
        private readonly MultiselectionTreeViewItem _parent;

        public MultiselectionTreeViewItemCollection(MultiselectionTreeViewItem parent)
        {
            _parent = parent;
        }

        public MultiselectionTreeViewItem this[int index]
        {
            get => _items[index];
            set
            {
                _items[index] = value;
                RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this, index));

            }
        }

        public int Count => _items.Count;

        public bool IsReadOnly => true;

        public void Add(MultiselectionTreeViewItem item)
        {
            _items.Add(item);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _items.Count - 1));
        }

        public void Clear()
        {
            var items = _items;
            _items = new List<MultiselectionTreeViewItem>();
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, 0));
        }

        public bool Contains(MultiselectionTreeViewItem item)
        {
            return _items.Any(x=>x.Equals(item));
        }

        public void CopyTo(MultiselectionTreeViewItem[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<MultiselectionTreeViewItem> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public int IndexOf(MultiselectionTreeViewItem item)
        {
            return item != null && item.ParentItem == _parent ? _items.IndexOf(item) : -1;
        }

        public void Insert(int index, MultiselectionTreeViewItem item)
        {
            _items.Insert(index, item);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public bool Remove(MultiselectionTreeViewItem item)
        {
            var index = IndexOf(item);
            if (index < 0)
                return false;

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            var multiselectionTreeViewItem = _items[index];
            _items.RemoveAt(index);
            RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, multiselectionTreeViewItem, index));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            _parent.OnChildrenChanged(args);

            CollectionChanged?.Invoke(this, args);
        }
    }
}
