using System.Windows;

namespace Kebler.UI.CSControls.Window
{
    public static class UIElementExtensions
    {
        public static void Show(this UIElement element)
        {
            element.Visibility = Visibility.Visible;
        }

        public static void Collapse(this UIElement element)
        {
            element.Visibility = Visibility.Collapsed;
        }

        public static void Hide(this UIElement element)
        {
            element.Visibility = Visibility.Hidden;
        }

        public static void Disable(this UIElement element)
        {
            element.IsEnabled = false;
        }

        public static void Enable(this UIElement element)
        {
            element.IsEnabled = true;
        }
    }
}