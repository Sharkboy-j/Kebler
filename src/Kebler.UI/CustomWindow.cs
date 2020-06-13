using System.Windows;

namespace Kebler.UI
{
    public class CustomWindow : Window
    {
        public static readonly DependencyProperty ShowTopLineProperty = DependencyProperty.Register(nameof(ShowTopLine), typeof(bool), typeof(CustomWindow),
           new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool ShowTopLine
        {
            get
            {
                return (bool)GetValue(ShowTopLineProperty);
            }
            set
            {
                SetValue(ShowTopLineProperty, value);
            }
        }
    }
}
