using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Kebler.UI.CSControls.MuliTreeView
{
    public static class ItemsControlExtensions
    {
        public static object GetObjectAtPoint<ItemContainer>(this ItemsControl control, Point p) where ItemContainer : DependencyObject
        {
            var containerAtPoint = control.GetContainerAtPoint<ItemContainer>(p);
            return containerAtPoint == null ? null : control.ItemContainerGenerator.ItemFromContainer(containerAtPoint);
        }

        public static ItemContainer GetContainerAtPoint<ItemContainer>(
          this ItemsControl control,
          Point p)
          where ItemContainer : DependencyObject
        {
            var hitTestResult = VisualTreeHelper.HitTest(control, p);
            if (hitTestResult == null)
                return default(ItemContainer);
            var reference = hitTestResult.VisualHit;
            while (VisualTreeHelper.GetParent(reference) != null && !(reference is ItemContainer))
                reference = VisualTreeHelper.GetParent(reference);
            return reference as ItemContainer;
        }

        public static void FocusSelectedItem(this Selector control)
        {
            if (control.SelectedIndex <= 0)
                return;
            var item = control.ItemContainerGenerator.ContainerFromIndex(control.SelectedIndex) as IInputElement;
            if (item == null)
                return;
            control.Dispatcher.Invoke(() => { Keyboard.Focus(item); });
            //control.Dispatcher.BeginInvoke(DispatcherPriority.Input, delegate(){ Keyboard.Focus(item); });
        }
    }
}
