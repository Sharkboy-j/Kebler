using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Kebler.UI
{
    public class ToolBarButton:Button
    {
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(ToolBarButton),
          new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof(Data), typeof(Geometry), typeof(ToolBarButton),
         new FrameworkPropertyMetadata(default(Geometry), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Geometry Data
        {
            get => (Geometry)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty PathWidthProperty = DependencyProperty.Register(nameof(PathWidth), typeof(double), typeof(ToolBarButton),
       new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double PathWidth
        {
            get => (double)GetValue(PathWidthProperty);
            set => SetValue(PathWidthProperty, value);
        }

        public static readonly DependencyProperty PathHeightProperty = DependencyProperty.Register(nameof(PathHeight), typeof(double), typeof(ToolBarButton),
     new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double PathHeight
        {
            get => (double)GetValue(PathHeightProperty);
            set => SetValue(PathHeightProperty, value);
        }



        public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(nameof(ImageSource), typeof(ImageSource), typeof(ToolBarButton),
            new FrameworkPropertyMetadata(default(ImageSource), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }
    }
}
