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

        public Visibility HeaderVisibility
        {
            get
            {
                return (Visibility)GetValue(HeaderVisibilityProperty);
            }
            set
            {
                SetValue(HeaderVisibilityProperty, value);
            }
        }

        public static readonly DependencyProperty HeaderVisibilityProperty;
        public static readonly DependencyProperty IsTitleVisibleProperty;
        public static readonly DependencyProperty FullScreenProperty;

        public bool IsTitleVisible
        {
            get
            {
                return (bool)GetValue(IsTitleVisibleProperty);
            }
            set
            {
                SetValue(IsTitleVisibleProperty, value);
            }
        }

        public bool FullScreen
        {
            get
            {
                return (bool)GetValue(FullScreenProperty);
            }
            set
            {
                SetValue(FullScreenProperty, value);
            }
        }
    }
}
