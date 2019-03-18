using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows;
using System.Windows.Interop;

namespace Kebler.Theme.Default
{

    internal static class LocalExtensions
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

    public partial class MainTheme
    {
        #region sizing event handlers

        private void OnSizeSouth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.South); }
        private void OnSizeNorth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.North); }
        private void OnSizeEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.East); }
        private void OnSizeWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.West); }
        private void OnSizeNorthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.NorthWest); }
        private void OnSizeNorthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.NorthEast); }
        private void OnSizeSouthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.SouthEast); }
        private void OnSizeSouthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.SouthWest); }

        private void OnSize(object sender, SizingAction action)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                sender.ForWindowFromTemplate(w =>
                {
                    if (w.WindowState == WindowState.Normal)
                        DragSize(w.GetWindowHandle(), action);
                });
            }
        }

        /*
                private void IconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
                {
                    if (e.ClickCount > 1)
                    {
                        sender.ForWindowFromTemplate(w => w.Close());
                    }
                    else
                    {
                        sender.ForWindowFromTemplate(w =>
                            SendMessage(w.GetWindowHandle(), WmSyscommand, (IntPtr)ScKeymenu, (IntPtr)' '));
                    }
                }
        */

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
        }

        //private static void WindowStateChanged(object sender, EventArgs e)
        //{
        //    var w = ((Window)sender);
        //    var handle = w.GetWindowHandle();
        //    var containerBorder = (Border)w.Template.FindName("PART_Container", w);

        //    //if (w.WindowState == WindowState.Maximized)
        //    //{
        //    //    // Make sure window doesn't overlap with the taskbar.
        //    //    var screen = System.Windows.Forms.Screen.FromHandle(handle);
        //    //    if (screen.Primary)
        //    //    {
        //    //        containerBorder.Padding = new Thickness(
        //    //            SystemParameters.WorkArea.Left + 7,
        //    //            SystemParameters.WorkArea.Top + 7,
        //    //            (SystemParameters.PrimaryScreenWidth - SystemParameters.WorkArea.Right) + 7,
        //    //            (SystemParameters.PrimaryScreenHeight - SystemParameters.WorkArea.Bottom) + 5);
        //    //    }
        //    //}
        //    //else
        //    //{
        //    //    containerBorder.Padding = new Thickness(7, 7, 7, 5);
        //    //}
        //}

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => w.Close());
        }


        private void MinButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => w.WindowState = WindowState.Minimized);
        }

        private void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => w.WindowState = (w.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized);
        }

        private void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                MaxButtonClick(sender, e);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                sender.ForWindowFromTemplate(w => w.DragMove());
            }
        }

        private void TitleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                sender.ForWindowFromTemplate(w =>
                {
                    if (w.WindowState == WindowState.Maximized)
                    {
                        w.BeginInit();
                        double adjustment = 40.0;
                        var mouse1 = e.MouseDevice.GetPosition(w);
                        var width1 = Math.Max(w.ActualWidth - 2 * adjustment, adjustment);
                        w.WindowState = WindowState.Normal;
                        var width2 = Math.Max(w.ActualWidth - 2 * adjustment, adjustment);
                        w.Left = (mouse1.X - adjustment) * (1 - width2 / width1);
                        w.Top = -7;
                        w.EndInit();
                        w.DragMove();
                    }
                });
            }
        }

        #endregion

        #region P/Invoke

        private const int WmSyscommand = 0x112;
        private const int ScSize = 0xF000;
        /*
                private const int ScKeymenu = 0xF100;
        */

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private static void DragSize(IntPtr handle, SizingAction sizingAction)
        {
            SendMessage(handle, WmSyscommand, (IntPtr)(ScSize + sizingAction), IntPtr.Zero);
            SendMessage(handle, 514, IntPtr.Zero, IntPtr.Zero);
        }

        private enum SizingAction
        {
            North = 3,
            South = 6,
            East = 2,
            West = 1,
            NorthEast = 5,
            NorthWest = 4,
            SouthEast = 8,
            SouthWest = 7
        }

        #endregion

        #region TopBar Events

        private void OpenCM(object sender, RoutedEventArgs e)
        {
            App.MainWindowControl.OpenCM();
        }
        private void ConnectFirst(object sender, RoutedEventArgs e)
        {
            App.MainWindowControl.Connect();
        }
        private void AddTorrent(object sender, RoutedEventArgs e)
        {
            App.MainWindowControl.AddTorrent();
        }
        

        #endregion
    }
}