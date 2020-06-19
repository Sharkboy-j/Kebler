using System.Windows;

namespace Kebler.UI.CSControls.Window
{
    public class WindowLocationState
    {
        public double Left { get; }

        public double Top { get; }

        public double Width { get; }

        public double Height { get; }

        public WindowState WindowState { get; }

        public WindowLocationState(
            double left,
            double top,
            double width,
            double height,
            WindowState windowState)
        {
            this.Left = left;
            this.Top = top;
            this.Width = width;
            this.Height = height;
            this.WindowState = windowState;
        }
    }
}
