using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using Transmission.API.RPC.Entity;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Interop;
using Kebler.Models;
using Microsoft.Win32;
using Kebler.Services;
using System.Windows.Input;
using Kebler.UI.Controls;
using Kebler.Services.Converters;
using Kebler.UI.Windows;
using LiteDB;
using log4net;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Newtonsoft.Json;
using Transmission.API.RPC;
using Transmission.API.RPC.Arguments;
using Enums = Transmission.API.RPC.Entity.Enums;
using System.Runtime.CompilerServices;
using MessageBox = Kebler.UI.Windows.MessageBox;

namespace Kebler.UI.Windows
{
    public partial class KeblerWindow
    {

        public void OnSizeSouth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.South); }
        public void OnSizeNorth(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.North); }
        public void OnSizeEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.East); }
        public void OnSizeWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.West); }
        public void OnSizeNorthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.NorthWest); }
        public void OnSizeNorthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.NorthEast); }
        public void OnSizeSouthEast(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.SouthEast); }
        public void OnSizeSouthWest(object sender, MouseButtonEventArgs e) { OnSize(sender, SizingAction.SouthWest); }

        public void OnSize(object sender, SizingAction action)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Normal)
                    DragSize(this.GetWindowHandle(), action);
            }
        }

        public void IconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                this.Close();
            }
            else
            {
                SendMessage(this.GetWindowHandle(), WM_SYSCOMMAND, (IntPtr)SC_KEYMENU, (IntPtr)' ');
            }
        }

        public void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void MinButtonClick(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            this.MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;
            this.WindowState = (this.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
        }

        public void TitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ClickCount > 1 && this.ResizeMode != ResizeMode.NoResize)
            {
                MaxButtonClick(sender, e);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        public void TitleBarMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                if (this.WindowState == WindowState.Maximized)
                {
                    this.BeginInit();
                    double adjustment = 40.0;
                    var mouse1 = e.MouseDevice.GetPosition(this);
                    var width1 = Math.Max(this.ActualWidth - 2 * adjustment, adjustment);
                    this.WindowState = WindowState.Normal;
                    var width2 = Math.Max(this.ActualWidth - 2 * adjustment, adjustment);
                    this.Left = (mouse1.X - adjustment) * (1 - width2 / width1);
                    this.Top = -7;
                    this.EndInit();
                    this.DragMove();
                }

            }
        }



        #region P/Invoke

        const int WM_SYSCOMMAND = 0x112;
        const int SC_SIZE = 0xF000;
        const int SC_KEYMENU = 0xF100;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        void DragSize(IntPtr handle, SizingAction sizingAction)
        {
            SendMessage(handle, WM_SYSCOMMAND, (IntPtr)(SC_SIZE + sizingAction), IntPtr.Zero);
            SendMessage(handle, 514, IntPtr.Zero, IntPtr.Zero);
        }

        public enum SizingAction
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
    }
}
