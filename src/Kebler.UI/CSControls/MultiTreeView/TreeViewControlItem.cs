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
        private Point _startPoint;
        private bool _wasSelected;
        private DragAdorner _adorner;

        public MultiselectionTreeViewItem Node
        {
            get
            {
                return this.DataContext as MultiselectionTreeViewItem;
            }
        }

        public MultiselectionTreeView ParentTreeView { get; internal set; }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property != DataContextProperty)
                return;
            this.UpdateDataContext(e.OldValue as MultiselectionTreeViewItem, e.NewValue as MultiselectionTreeViewItem);
        }

        private void UpdateDataContext(
          MultiselectionTreeViewItem oldNode,
          MultiselectionTreeViewItem newNode)
        {
            if (newNode != null)
            {
                newNode.PropertyChanged += new PropertyChangedEventHandler(this.Node_PropertyChanged);
                if (this.Template != null)
                    this.UpdateTemplate();
            }
            if (oldNode == null)
                return;
            oldNode.PropertyChanged -= new PropertyChangedEventHandler(this.Node_PropertyChanged);
        }

        private void Node_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(e.PropertyName == "IsExpanded") || !this.Node.IsExpanded)
                return;
            //this.ParentTreeView.HandleExpanding(this.Node);
        }

        private void UpdateTemplate()
        {
        }

        internal double CalculateIndent()
        {
            int num = 19 * this.Node.Level - 19;
            return num < 0 ? 0.0 : num;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            this._wasSelected = this.IsSelected;
            if (!this.IsSelected)
                base.OnMouseLeftButtonDown(e);
            if (!this.ParentTreeView.AllowDragDrop || Mouse.LeftButton != MouseButtonState.Pressed)
                return;
            this._startPoint = e.GetPosition(null);
            this.CaptureMouse();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!this.IsMouseCaptured)
                return;
            Point position = e.GetPosition(null);
            if (Math.Abs(position.X - this._startPoint.X) < SystemParameters.MinimumHorizontalDragDistance && Math.Abs(position.Y - this._startPoint.Y) < SystemParameters.MinimumVerticalDragDistance)
                return;
            this._adorner = new DragAdorner(this, e.GetPosition(this));
            if (this._adorner == null)
                return;
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(ParentTreeView);
            if (adornerLayer == null)
                return;
            adornerLayer.Add(_adorner);
            this.Node.StartDrag(this, this.ParentTreeView.GetTopLevelSelection().ToArray<MultiselectionTreeViewItem>());
            adornerLayer.Remove(_adorner);
        }

        protected override void OnGiveFeedback(GiveFeedbackEventArgs e)
        {
            if (!this.IsVisible || this._adorner == null)
                return;
            this._adorner.UpdatePosition(this.PointFromScreen(MouseHelper.GetMousePosition()));
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            this.ReleaseMouseCapture();
            if (!this._wasSelected)
                return;
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            this.ParentTreeView.HandleDragEnter(this, e);
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            this.ParentTreeView.HandleDragOver(this, e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            this.ParentTreeView.HandleDrop(this, e);
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            this.ParentTreeView.HandleDragLeave(this, e);
        }
    }
}
