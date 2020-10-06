using System.Windows;

namespace Kebler.UI.CSControls.Window
{
    public class WindowLocationState
    {
        public WindowLocationState(
            double left,
            double top,
            double width,
            double height,
            WindowState windowState)
        {
            Left = left;
            Top = top;
            Width = width;
            Height = height;
            WindowState = windowState;
        }

        public double Left { get; }

        public double Top { get; }

        public double Width { get; }

        public double Height { get; }

        public WindowState WindowState { get; }
    }
}