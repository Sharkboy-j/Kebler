using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Kebler.UI.CSControls.MuliTreeView
{
    public class DragAdorner : Adorner
    {
        private readonly Brush _visualBrush;
        private Point _initialPosition;

        public DragAdorner(UIElement adornedElement, Point position)
            : base(adornedElement)
        {
            _initialPosition = position;
            var visualBrush = new VisualBrush(AdornedElement);
            visualBrush.Opacity = 0.6;
            _visualBrush = visualBrush;
            IsHitTestVisible = false;
        }

        private Point NewPosition { get; set; }

        public void UpdatePosition(Point position)
        {
            NewPosition = position;
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext context)
        {
            var newPosition = NewPosition;
            newPosition.Offset(-_initialPosition.X, -_initialPosition.Y);
            context.DrawRectangle(_visualBrush, null, new Rect(newPosition, RenderSize));
        }
    }
}