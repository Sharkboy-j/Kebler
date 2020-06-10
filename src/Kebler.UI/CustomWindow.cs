using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Kebler.UI
{
    public class CustomWindow : Window
    {


        public CustomWindow()
        {

        }

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
