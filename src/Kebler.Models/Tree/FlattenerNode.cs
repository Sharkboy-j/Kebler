using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Kebler.Models.Tree
{
    public abstract class FlattenerNode
    {
        private byte _height = 1;
        private FlattenerNode _left;
        private FlattenerNode _Parent;
        private FlattenerNode _right;
        private int _totalCount = -1;
        protected Flattener _treeFlattener;

        public bool IsVisible { get; protected set; } = true;

        private int Balance => Height(_right) - Height(_left);

        public MultiselectionTreeViewItem GetListRoot()
        {
            var flattenerNode = this;
            while (flattenerNode._Parent != null)
                flattenerNode = flattenerNode._Parent;
            return (MultiselectionTreeViewItem) flattenerNode;
        }

        private int TotalCount()
        {
            if (_totalCount > 0)
                return _totalCount;
            var num = IsVisible ? 1 : 0;
            if (_left != null)
                num += _left.TotalCount();
            if (_right != null)
                num += _right.TotalCount();
            _totalCount = num;
            return _totalCount;
        }

        protected void InvalidateParents()
        {
            for (var flattenerNode = this;
                flattenerNode != null && flattenerNode._totalCount >= 0;
                flattenerNode = flattenerNode._Parent)
                flattenerNode._totalCount = -1;
        }

        public virtual void OnIsVisibleChanged()
        {
        }

        private static int Height(FlattenerNode node)
        {
            return node?._height ?? 0;
        }

        protected void CheckRootInvariants()
        {
            GetListRoot().CheckInvariants();
        }

        private void CheckInvariants()
        {
            _left?.CheckInvariants();
            _right?.CheckInvariants();
        }

        private static void DumpTree(FlattenerNode node)
        {
            node.GetListRoot().DumpTree();
        }

        private void DumpTree()
        {
            _left?.DumpTree();
            _right?.DumpTree();
        }

        internal static FlattenerNode GetNodeByVisibleIndex(FlattenerNode root, int index)
        {
            root.TotalCount();
            var flattenerNode = root;
            while (true)
            {
                for (;
                    flattenerNode._left == null || index >= flattenerNode._left._totalCount;
                    flattenerNode = flattenerNode._right)
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
            var num = node._left?.TotalCount() ?? 0;
            for (; node._Parent != null; node = node._Parent)
                if (node == node._Parent._right)
                {
                    if (node._Parent._left != null)
                        num += node._Parent._left.TotalCount();
                    if (node._Parent.IsVisible)
                        ++num;
                }

            return num;
        }

        private static FlattenerNode Rebalance(FlattenerNode node)
        {
            while (Math.Abs(node.Balance) > 1)
                if (node.Balance > 1)
                {
                    if (node._right.Balance < 0)
                        node._right = node._right.Rotate();
                    node = node.LLRotate();
                    node._left = Rebalance(node._left);
                }
                else if (node.Balance < -1)
                {
                    if (node._left.Balance > 0)
                        node._left = node._left.LLRotate();
                    node = node.Rotate();
                    node._right = Rebalance(node._right);
                }

            node._height = (byte) (1 + Math.Max(Height(node._left), Height(node._right)));
            node._totalCount = -1;
            return node;
        }

        private FlattenerNode LLRotate()
        {
            var left = _right._left;
            var right = _right;
            if (left != null)
                left._Parent = this;

            _right = left;
            right._left = this;
            right._Parent = _Parent;

            _Parent = right;
            right._left = Rebalance(this);
            return right;
        }

        private FlattenerNode Rotate()
        {
            var right = _left._right;
            var left = _left;
            if (right != null)
                right._Parent = this;
            _left = right;
            left._right = this;
            left._Parent = _Parent;
            _Parent = left;
            left._right = Rebalance(this);
            return left;
        }

        private static void RebalanceUntilRoot(FlattenerNode pos)
        {
            for (; pos._Parent != null; pos = pos._Parent)
                pos = pos != pos._Parent._left
                    ? pos._Parent._right = Rebalance(pos)
                    : pos._Parent._left = Rebalance(pos);
            var flattenerNode = Rebalance(pos);
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
            var flattenerNodeList = new List<FlattenerNode>();
            var node = start;
            FlattenerNode flattenerNode1;
            do
            {
                var flattenerNodeSet = new HashSet<FlattenerNode>();
                for (var flattenerNode2 = end; flattenerNode2 != null; flattenerNode2 = flattenerNode2._Parent)
                    flattenerNodeSet.Add(flattenerNode2);
                flattenerNodeList.Add(node);
                if (!flattenerNodeSet.Contains(node) && node._right != null)
                {
                    flattenerNodeList.Add(node._right);
                    node._right._Parent = null;
                    node._right = null;
                }

                var flattenerNode3 = node.Successor();
                DeleteNode(node);
                flattenerNode1 = node;
                node = flattenerNode3;
            } while (flattenerNode1 != end);

            var first = flattenerNodeList[0];
            for (var index = 1; index < flattenerNodeList.Count; ++index)
                first = ConcatTrees(first, flattenerNodeList[index]);
        }

        private static FlattenerNode ConcatTrees(FlattenerNode first, FlattenerNode second)
        {
            var pos = first;
            while (pos._right != null)
                pos = pos._right;
            InsertNodeAfter(pos, second);
            return pos.GetListRoot();
        }

        private FlattenerNode Successor()
        {
            if (_right != null)
            {
                var flattenerNode = _right;
                while (flattenerNode._left != null)
                    flattenerNode = flattenerNode._left;
                return flattenerNode;
            }

            var flattenerNode1 = this;
            FlattenerNode flattenerNode2;
            do
            {
                flattenerNode2 = flattenerNode1;
                flattenerNode1 = flattenerNode1._Parent;
            } while (flattenerNode1 != null && flattenerNode1._right == flattenerNode2);

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
                var node1 = node._right;
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
            if (_Parent != null)
            {
                if (_Parent._left == this)
                    _Parent._left = node;
                else
                    _Parent._right = node;
                if (node != null)
                    node._Parent = _Parent;
                _Parent = null;
            }
            else
            {
                node._Parent = null;
                if (_treeFlattener == null)
                    return;
                node._treeFlattener = _treeFlattener;
                _treeFlattener = null;
                node._treeFlattener._root = node;
            }
        }

        public class Flattener : IList, INotifyCollectionChanged
        {
            public FlattenerNode _root;

            public Flattener(FlattenerNode root)
            {
                root = root.GetListRoot();
                _root = root;
                root._treeFlattener = this;
            }

            public object this[int index]
            {
                get
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException();
                    return GetNodeByVisibleIndex(_root, index + 1);
                }
                set => throw new NotSupportedException();
            }

            public bool IsReadOnly => true;

            public bool IsFixedSize => false;

            public int Count => _root.TotalCount() - 1;

            public object SyncRoot { get; } = new object();

            public bool IsSynchronized => false;

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
                return IndexOf(item) >= 0;
            }

            public void CopyTo(Array array, int index)
            {
                throw new NotImplementedException();
            }

            public IEnumerator GetEnumerator()
            {
                for (var i = 0; i < Count; ++i)
                    yield return this[i];
            }

            public int IndexOf(object item)
            {
                return item is MultiselectionTreeViewItem multiselectionTreeViewItem &&
                       multiselectionTreeViewItem.IsVisible && multiselectionTreeViewItem.GetListRoot() == _root
                    ? GetVisibleIndexForNode(multiselectionTreeViewItem) - 1
                    : -1;
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

            public event NotifyCollectionChangedEventHandler CollectionChanged;

            public void Unmount()
            {
                _root._treeFlattener = null;
            }

            public void NodesInserted(int index, IEnumerable<MultiselectionTreeViewItem> nodes)
            {
                --index;
                foreach (var node in nodes)
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, node, index++));
            }

            public void NodesRemoved(int index, IEnumerable<MultiselectionTreeViewItem> nodes)
            {
                --index;
                foreach (var node in nodes)
                    CollectionChanged?.Invoke(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, node, index));
            }
        }
    }
}