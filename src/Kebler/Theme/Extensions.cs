using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace Kebler.Theme
{

    public static class LocalExtensions
    {
        //    public static void ForWindowFromChild(this object childDependencyObject, Action<Window> action)
        //    {
        //        var element = childDependencyObject as DependencyObject;
        //        while (element != null)
        //        {
        //            element = VisualTreeHelper.GetParent(element);
        //            if (element is Window window) { action(window); break; }
        //        }
        //    }
        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            if (((FrameworkElement)templateFrameworkElement).TemplatedParent is Window window) action(window);
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            var helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }
}
