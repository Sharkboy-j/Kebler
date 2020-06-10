using System;
using System.Collections.Generic;
using System.Text;
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
            get
            {
                return (string)GetValue(TitleProperty);
            }
            set
            {
                SetValue(TitleProperty, value);
            }
        }

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(nameof(Data), typeof(Geometry), typeof(ToolBarButton),
         new FrameworkPropertyMetadata(default(Geometry), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public Geometry Data
        {
            get
            {
                return (Geometry)GetValue(DataProperty);
            }
            set
            {
                SetValue(DataProperty, value);
            }
        }

        public static readonly DependencyProperty PathWidthProperty = DependencyProperty.Register(nameof(PathWidth), typeof(double), typeof(ToolBarButton),
       new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double PathWidth
        {
            get
            {
                return (double)GetValue(PathWidthProperty);
            }
            set
            {
                SetValue(PathWidthProperty, value);
            }
        }

        public static readonly DependencyProperty PathHeightProperty = DependencyProperty.Register(nameof(PathHeight), typeof(double), typeof(ToolBarButton),
     new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double PathHeight
        {
            get
            {
                return (double)GetValue(PathHeightProperty);
            }
            set
            {
                SetValue(PathHeightProperty, value);
            }
        }
    }
}
