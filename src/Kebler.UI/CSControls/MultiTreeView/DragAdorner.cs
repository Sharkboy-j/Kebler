using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Kebler.UI.CSControls.MuliTreeView
{
    public class DragAdorner : Adorner
    {
        private Brush _visualBrush;
        private Point _initialPosition;

        private Point NewPosition { get; set; }

        public DragAdorner(UIElement adornedElement, Point position)
          : base(adornedElement)
        {
            this._initialPosition = position;
            var visualBrush = new VisualBrush(AdornedElement);
            visualBrush.Opacity = 0.6;
            this._visualBrush = visualBrush;
            this.IsHitTestVisible = false;
        }

        public void UpdatePosition(Point position)
        {
            this.NewPosition = position;
            this.InvalidateVisual();
        }

        protected override void OnRender(DrawingContext context)
        {
            var newPosition = this.NewPosition;
            newPosition.Offset(-this._initialPosition.X, -this._initialPosition.Y);
            context.DrawRectangle(this._visualBrush, null, new Rect(newPosition, this.RenderSize));
        }
    }
}
