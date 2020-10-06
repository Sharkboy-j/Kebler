using System.Windows;

namespace Kebler.UI.Controls
{
    /// <summary>
    ///     Interaction logic for TagBorder.xaml
    /// </summary>
    public partial class TagBorder
    {
        public static readonly DependencyProperty PercentProperty = DependencyProperty.Register(nameof(Percent),
            typeof(double), typeof(TagBorder),
            new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(nameof(Status),
            typeof(double), typeof(TagBorder),
            new FrameworkPropertyMetadata(0d,
                FrameworkPropertyMetadataOptions.AffectsRender |
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public TagBorder()
        {
            InitializeComponent();
        }

        public double Percent
        {
            get => (int) GetValue(PercentProperty);
            set => SetValue(PercentProperty, value);
        }

        public double Status
        {
            get => (int) GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }
    }
}