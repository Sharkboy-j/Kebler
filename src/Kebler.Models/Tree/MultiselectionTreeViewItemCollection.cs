using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Kebler.Models.Tree
{
    public class MultiselectionTreeViewItemCollection : IList<MultiselectionTreeViewItem>, INotifyCollectionChanged
    {
        private List<MultiselectionTreeViewItem> _items = new List<MultiselectionTreeViewItem>();
        private readonly MultiselectionTreeViewItem _parent;

        public MultiselectionTreeViewItemCollection(MultiselectionTreeViewItem parent)
        {
            this._parent = parent;
        }

        public MultiselectionTreeViewItem this[int index]
        {
            get
            {
                return this._items[index];
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int Count
        {
            get
            {
                return this._items.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public void Add(MultiselectionTreeViewItem item)
        {
            this._items.Add(item);
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, this._items.Count - 1));
        }

        public void Clear()
        {
            List<MultiselectionTreeViewItem> items = this._items;
            this._items = new List<MultiselectionTreeViewItem>();
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, items, 0));
        }

        public bool Contains(MultiselectionTreeViewItem item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(MultiselectionTreeViewItem[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<MultiselectionTreeViewItem> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        public int IndexOf(MultiselectionTreeViewItem item)
        {
            return item != null && item.ParentItem == this._parent ? this._items.IndexOf(item) : -1;
        }

        public void Insert(int index, MultiselectionTreeViewItem item)
        {
            this._items.Insert(index, item);
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        public bool Remove(MultiselectionTreeViewItem item)
        {
            int index = this.IndexOf(item);
            if (index < 0)
                return false;
            this.RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            MultiselectionTreeViewItem multiselectionTreeViewItem = this._items[index];
            this._items.RemoveAt(index);
            this.RaiseCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, multiselectionTreeViewItem, index));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void RaiseCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            this._parent.OnChildrenChanged(args);
            NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
            if (collectionChanged == null)
                return;
            collectionChanged(this, args);
        }
    }
}
