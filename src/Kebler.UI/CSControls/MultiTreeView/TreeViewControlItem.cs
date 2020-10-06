using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Kebler.Models.Tree;
using Kebler.UI.CSControls.MuliTreeView;

namespace Kebler.UI.CSControls.MultiTreeView
{
    public class TreeViewControlItem : ListViewItem
    {
        private DragAdorner _adorner;
        private Point _startPoint;
        private bool _wasSelected;

        public MultiselectionTreeViewItem Node => DataContext as MultiselectionTreeViewItem;

        public MultiselectionTreeView ParentTreeView { get; internal set; }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property != DataContextProperty)
                return;
            UpdateDataContext(e.OldValue as MultiselectionTreeViewItem, e.NewValue as MultiselectionTreeViewItem);
        }

        private void UpdateDataContext(INotifyPropertyChanged oldNode, INotifyPropertyChanged newNode)
        {
            if (newNode != null)
            {
                newNode.PropertyChanged += Node_PropertyChanged;
                if (Template != null)
                    UpdateTemplate();
            }

            if (oldNode == null)
                return;
            oldNode.PropertyChanged -= Node_PropertyChanged;
        }

        private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "IsExpanded" || !Node.IsExpanded)
                return;
            ParentTreeView.HandleExpanding(Node);
        }

        private void UpdateTemplate()
        {
        }

        internal double CalculateIndent()
        {
            var num = 19 * Node.Level - 19;
            return num < 0 ? 0.0 : num;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            _wasSelected = IsSelected;
            if (!IsSelected)
                base.OnMouseLeftButtonDown(e);
            if (!ParentTreeView.AllowDragDrop || Mouse.LeftButton != MouseButtonState.Pressed)
                return;
            _startPoint = e.GetPosition(null);
            CaptureMouse();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!IsMouseCaptured)
                return;
            var position = e.GetPosition(null);
            if (Math.Abs(position.X - _startPoint.X) < SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(position.Y - _startPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;
            _adorner = new DragAdorner(this, e.GetPosition(this));
            if (_adorner == null)
                return;
            var adornerLayer = AdornerLayer.GetAdornerLayer(ParentTreeView);
            if (adornerLayer == null)
                return;
            adornerLayer.Add(_adorner);
            Node.StartDrag(this, ParentTreeView.GetTopLevelSelection().ToArray());
            adornerLayer.Remove(_adorner);
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            if (!IsVisible || _adorner == null)
                return;
            _adorner.UpdatePosition(PointFromScreen(MouseHelper.GetMousePosition()));
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            if (!_wasSelected)
                return;
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            ParentTreeView.HandleDragEnter(this, e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            ParentTreeView.HandleDragOver(this, e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            ParentTreeView.HandleDrop(this, e);
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            ParentTreeView.HandleDragLeave(this, e);
        }
    }
}