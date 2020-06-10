using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kebler.UI.Controls
{
    /// <summary>
    /// Interaction logic for TagBorder.xaml
    /// </summary>
    public partial class TagBorder : UserControl
    {
        public TagBorder()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty PercentProperty = DependencyProperty.Register(nameof(Percent), typeof(double), typeof(TagBorder),
           new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double Percent
        {
            get
            {
                return (int)GetValue(PercentProperty);
            }
            set
            {
                SetValue(PercentProperty, value);
            }
        }


        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(nameof(Status), typeof(double), typeof(TagBorder),
           new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double Status
        {
            get
            {
                return (int)GetValue(StatusProperty);
            }
            set
            {
                SetValue(StatusProperty, value);
            }
        }
    }
}
