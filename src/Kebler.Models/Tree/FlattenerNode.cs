using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Kebler.Models.Tree
{
    public abstract class FlattenerNode
    {
        private int _totalCount = -1;
        private byte _height = 1;
        protected FlattenerNode.Flattener _treeFlattener;
        private FlattenerNode _Parent;
        private FlattenerNode _left;
        private FlattenerNode _right;

        public bool IsVisible { get; protected set; } = true;

        private int Balance
        {
            get
            {
                return Height(this._right) - Height(this._left);
            }
        }

        public MultiselectionTreeViewItem GetListRoot()
        {
            FlattenerNode flattenerNode = this;
            while (flattenerNode._Parent != null)
                flattenerNode = flattenerNode._Parent;
            return (MultiselectionTreeViewItem)flattenerNode;
        }

        private int TotalCount()
        {
            if (this._totalCount > 0)
                return this._totalCount;
            int num = this.IsVisible ? 1 : 0;
            if (this._left != null)
                num += this._left.TotalCount();
            if (this._right != null)
                num += this._right.TotalCount();
            this._totalCount = num;
            return this._totalCount;
        }

        protected void InvalidateParents()
        {
            for (FlattenerNode flattenerNode = this; flattenerNode != null && flattenerNode._totalCount >= 0; flattenerNode = flattenerNode._Parent)
                flattenerNode._totalCount = -1;
        }

        public virtual void OnIsVisibleChanged()
        {
        }

        private static int Height(FlattenerNode node)
        {
            return node == null ? 0 : node._height;
        }

        protected void CheckRootInvariants()
        {
            this.GetListRoot().CheckInvariants();
        }

        private void CheckInvariants()
        {
            if (this._left != null)
                this._left.CheckInvariants();
            if (this._right == null)
                return;
            this._right.CheckInvariants();
        }

        private static void DumpTree(FlattenerNode node)
        {
            node.GetListRoot().DumpTree();
        }

        private void DumpTree()
        {
            if (this._left != null)
                this._left.DumpTree();
            if (this._right == null)
                return;
            this._right.DumpTree();
        }

        internal static FlattenerNode GetNodeByVisibleIndex(FlattenerNode root, int index)
        {
            root.TotalCount();
            FlattenerNode flattenerNode = root;
            while (true)
            {
                for (; flattenerNode._left == null || index >= flattenerNode._left._totalCount; flattenerNode = flattenerNode._right)
                {
                    if (flattenerNode._left != null)
                        index -= flattenerNode._left._totalCount;
                    if (flattenerNode.IsVisible)
                    {
                        if (index == 0)
                            return flattenerNode;
                        --index;
                    }
                }
                flattenerNode = flattenerNode._left;
            }
        }

        internal static int GetVisibleIndexForNode(FlattenerNode node)
        {
            int num = node._left != null ? node._left.TotalCount() : 0;
            for (; node._Parent != null; node = node._Parent)
            {
                if (node == node._Parent._right)
                {
                    if (node._Parent._left != null)
                        num += node._Parent._left.TotalCount();
                    if (node._Parent.IsVisible)
                        ++num;
                }
            }
            return num;
        }

        private static FlattenerNode Rebalance(FlattenerNode node)
        {
            while (Math.Abs(node.Balance) > 1)
            {
                if (node.Balance > 1)
                {
                    if (node._right.Balance < 0)
                        node._right = node._right.RRRotate();
                    node = node.LLRotate();
                    node._left = Rebalance(node._left);
                }
                else if (node.Balance < -1)
                {
                    if (node._left.Balance > 0)
                        node._left = node._left.LLRotate();
                    node = node.RRRotate();
                    node._right = Rebalance(node._right);
                }
            }
            node._height = (byte)(1 + Math.Max(Height(node._left), Height(node._right)));
            node._totalCount = -1;
            return node;
        }

        private FlattenerNode LLRotate()
        {
            FlattenerNode left = this._right._left;
            FlattenerNode right = this._right;
            if (left != null)
                left._Parent = this;
            this._right = left;
            right._left = this;
            right._Parent = this._Parent;
            this._Parent = right;
            right._left = Rebalance(this);
            return right;
        }

        private FlattenerNode RRRotate()
        {
            FlattenerNode right = this._left._right;
            FlattenerNode left = this._left;
            if (right != null)
                right._Parent = this;
            this._left = right;
            left._right = this;
            left._Parent = this._Parent;
            this._Parent = left;
            left._right = Rebalance(this);
            return left;
        }

        private static void RebalanceUntilRoot(FlattenerNode pos)
        {
            for (; pos._Parent != null; pos = pos._Parent)
                pos = pos != pos._Parent._left ? (pos._Parent._right = Rebalance(pos)) : (pos._Parent._left = Rebalance(pos));
            FlattenerNode flattenerNode = Rebalance(pos);
            if (flattenerNode == pos || pos._treeFlattener == null)
                return;
            flattenerNode._treeFlattener = pos._treeFlattener;
            pos._treeFlattener = null;
            flattenerNode._treeFlattener._root = flattenerNode;
        }

        protected static void InsertNodeAfter(FlattenerNode pos, FlattenerNode newNode)
        {
            newNode = newNode.GetListRoot();
            if (pos._right == null)
            {
                pos._right = newNode;
                newNode._Parent = pos;
            }
            else
            {
                pos = pos._right;
                while (pos._left != null)
                    pos = pos._left;
                pos._left = newNode;
                newNode._Parent = pos;
            }
            RebalanceUntilRoot(pos);
        }

        protected void RemoveNodes(FlattenerNode start, FlattenerNode end)
        {
            List<FlattenerNode> flattenerNodeList = new List<FlattenerNode>();
            FlattenerNode node = start;
            FlattenerNode flattenerNode1;
            do
            {
                HashSet<FlattenerNode> flattenerNodeSet = new HashSet<FlattenerNode>();
                for (FlattenerNode flattenerNode2 = end; flattenerNode2 != null; flattenerNode2 = flattenerNode2._Parent)
                    flattenerNodeSet.Add(flattenerNode2);
                flattenerNodeList.Add(node);
                if (!flattenerNodeSet.Contains(node) && node._right != null)
                {
                    flattenerNodeList.Add(node._right);
                    node._right._Parent = null;
                    node._right = null;
                }
                FlattenerNode flattenerNode3 = node.Successor();
                DeleteNode(node);
                flattenerNode1 = node;
                node = flattenerNode3;
            }
            while (flattenerNode1 != end);
            FlattenerNode first = flattenerNodeList[0];
            for (int index = 1; index < flattenerNodeList.Count; ++index)
                first = ConcatTrees(first, flattenerNodeList[index]);
        }

        private static FlattenerNode ConcatTrees(FlattenerNode first, FlattenerNode second)
        {
            FlattenerNode pos = first;
            while (pos._right != null)
                pos = pos._right;
            InsertNodeAfter(pos, second);
            return pos.GetListRoot();
        }

        private FlattenerNode Successor()
        {
            if (this._right != null)
            {
                FlattenerNode flattenerNode = this._right;
                while (flattenerNode._left != null)
                    flattenerNode = flattenerNode._left;
                return flattenerNode;
            }
            FlattenerNode flattenerNode1 = this;
            FlattenerNode flattenerNode2;
            do
            {
                flattenerNode2 = flattenerNode1;
                flattenerNode1 = flattenerNode1._Parent;
            }
            while (flattenerNode1 != null && flattenerNode1._right == flattenerNode2);
            return flattenerNode1;
        }

        private static void DeleteNode(FlattenerNode node)
        {
            FlattenerNode pos;
            if (node._left == null)
            {
                pos = node._Parent;
                node.ReplaceWith(node._right);
                node._right = null;
            }
            else if (node._right == null)
            {
                pos = node._Parent;
                node.ReplaceWith(node._left);
                node._left = null;
            }
            else
            {
                FlattenerNode node1 = node._right;
                while (node1._left != null)
                    node1 = node1._left;
                pos = node1._Parent;
                node1.ReplaceWith(node1._right);
                node1._right = null;
                node1._left = node._left;
                node._left = null;
                node1._right = node._right;
                node._right = null;
                if (node1._left != null)
                    node1._left._Parent = node1;
                if (node1._right != null)
                    node1._right._Parent = node1;
                node.ReplaceWith(node1);
                if (pos == node)
                    pos = node1;
            }
            node._height = 1;
            node._totalCount = -1;
            if (pos == null)
                return;
            RebalanceUntilRoot(pos);
        }

        private void ReplaceWith(FlattenerNode node)
        {
            if (this._Parent != null)
            {
                if (this._Parent._left == this)
                    this._Parent._left = node;
                else
                    this._Parent._right = node;
                if (node != null)
                    node._Parent = this._Parent;
                this._Parent = null;
            }
            else
            {
                node._Parent = null;
                if (this._treeFlattener == null)
                    return;
                node._treeFlattener = this._treeFlattener;
                this._treeFlattener = null;
                node._treeFlattener._root = node;
            }
        }

        public class Flattener : IList, IEnumerable, ICollection, INotifyCollectionChanged
        {
            private readonly object _syncRoot = new object();
            public FlattenerNode _root;

            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public Flattener(FlattenerNode root)
            {
                root = root.GetListRoot();
                this._root = root;
                root._treeFlattener = this;
            }

            public void Unmount()
            {
                this._root._treeFlattener = null;
            }

            public object this[int index]
            {
                get
                {
                    if (index < 0 || index >= this.Count)
                        throw new ArgumentOutOfRangeException();
                    return GetNodeByVisibleIndex(this._root, index + 1);
                }
                set
                {
                    throw new NotSupportedException();
                }
            }

            public bool IsReadOnly
            {
                get
                {
                    return true;
                }
            }

            public bool IsFixedSize
            {
                get
                {
                    return false;
                }
            }

            public int Count
            {
                get
                {
                    return this._root.TotalCount() - 1;
                }
            }

            public object SyncRoot
            {
                get
                {
                    return this._syncRoot;
                }
            }

            public bool IsSynchronized
            {
                get
                {
                    return false;
                }
            }

            public int Add(object value)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(object item)
            {
                return this.IndexOf(item) >= 0;
            }

            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public IEnumerator GetEnumerator()
            {
                for (int i = 0; i < this.Count; ++i)
                    yield return this[i];
            }

            public int IndexOf(object item)
            {
                return item is MultiselectionTreeViewItem multiselectionTreeViewItem && multiselectionTreeViewItem.IsVisible && multiselectionTreeViewItem.GetListRoot() == this._root ? GetVisibleIndexForNode(multiselectionTreeViewItem) - 1 : -1;
            }

            public void Insert(int index, object value)
            {
                throw new NotImplementedException();
            }

            public void Remove(object value)
            {
                throw new NotImplementedException();
            }

            public void RemoveAt(int index)
            {
                throw new NotImplementedException();
            }

            public void NodesInserted(int index, IEnumerable<MultiselectionTreeViewItem> nodes)
            {
                --index;
                foreach (MultiselectionTreeViewItem node in nodes)
                {
                    NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
                    if (collectionChanged != null)
                        collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, index++));
                }
            }

            public void NodesRemoved(int index, IEnumerable<MultiselectionTreeViewItem> nodes)
            {
                --index;
                foreach (MultiselectionTreeViewItem node in nodes)
                {
                    NotifyCollectionChangedEventHandler collectionChanged = this.CollectionChanged;
                    if (collectionChanged != null)
                        collectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node, index));
                }
            }
        }
    }
}
