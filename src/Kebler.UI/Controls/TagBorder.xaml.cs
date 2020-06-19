using System.Windows;
using System.Windows.Controls;

namespace Kebler.UI.Controls
{
    /// <summary>
    /// Interaction logic for TagBorder.xaml
    /// </summary>
    public partial class TagBorder
    {
        public TagBorder()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty PercentProperty = DependencyProperty.Register(nameof(Percent), typeof(double), typeof(TagBorder),
           new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double Percent
        {
            get => (int)GetValue(PercentProperty);
            set => SetValue(PercentProperty, value);
        }


        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(nameof(Status), typeof(double), typeof(TagBorder),
           new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double Status
        {
            get => (int)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }
    }
}
