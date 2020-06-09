using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using Kebler.UI.Windows.Dialogs;
using System.Windows.Media;
using Transmission.API.RPC.Entity;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using Kebler.UI.ViewModels;
using System.Windows.Interop;
using Kebler.Models;
using MessageBox = System.Windows.MessageBox;
using Microsoft.Win32;
using Kebler.Services;
using System.Windows.Input;
using Kebler.UI.Controls;

namespace Kebler.UI.Windows
{
    /// <inheritdoc cref="KeblerWindow" />
    /// <summary>
    /// Interaction logic for Kebler.xaml
    /// </summary>
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



















































        private MainWindowViewModel Vm => this.DataContext as MainWindowViewModel;


        public KeblerWindow()
        {
            InitializeComponent();

            //disable hardware rendering
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            DataContext = new MainWindowViewModel();


        }


        public void UpdateSorting()
        {
            Vm.UpdateSorting();
        }



        public void OpenConnectionManager()
        {
            Vm.ShowConnectionManager();
        }

        public void Connect()
        {
            Vm.InitConnection();
        }



        public void AddTorrent()
        {

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Image files (*.torrent)|*.torrent|All files (*.*)|*.*",
                Multiselect = true,
            };

            if (openFileDialog.ShowDialog() != true) return;

            OpenTorrent(openFileDialog.FileNames);
        }

        private void OpenTorrent(string[] names)
        {
            foreach (var item in names)
            {
                var dialog = new AddTorrentDialog(item, Vm._settings, ref Vm._transmissionClient);
                if (ConfigService.Instanse.IsAddTorrentWindowShow)
                {
                    dialog.Add(null, null);
                }
                else
                {
                    if (!(bool)dialog.ShowDialog())
                        return;
                }
            }
        }


        private void RetryConnection_ButtonCLick(object sender, RoutedEventArgs e)
        {
            // new Task(() => { VM.TryConnect(ServersList.FirstOrDefault()); }).Start();

        }

        private void TorrentsDataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //if (TorrentsDataGrid.SelectedValue is TorrentInfo tor)
            //{
            //    Vm.SelectedTorrent = tor;
            //}
        }

        private void RemoveTorrent_ItemClick(object sender, RoutedEventArgs e)
        {
            Vm.RemoveTorrent();
        }
        private void RemoveTorrentData_ItemClick(object sender, RoutedEventArgs e)
        {
            Vm.RemoveTorrent(true);
        }

        private void PauseTorrent_ItemClick(object sender, RoutedEventArgs e)
        {
            Vm.PauseTorrent();
        }

        private void StartAll_Button_CLick(object sender, RoutedEventArgs e)
        {
            Vm.StartAll();
        }

        private void AddTorrentButtonClick(object sender, RoutedEventArgs e)
        {
            AddTorrent();
        }

        private void PauseAll_ButtonClick(object sender, RoutedEventArgs e)
        {
            Vm.StopAll();
        }

        private void StartSelected_ButtonClick(object sender, RoutedEventArgs e)
        {
            Vm.StartOne();
        }

        private void PauseSelected_ButtonClick(object sender, RoutedEventArgs e)
        {
            Vm.PauseTorrent();
        }

        private void RemoveTorrent_ButtonClick(object sender, RoutedEventArgs e)
        {
            Vm.RemoveTorrent();
        }
        private void RemoveTorrentWithData_ButtonClick(object sender, RoutedEventArgs e)
        {
            Vm.RemoveTorrent(true);
        }


        private void Window_StateChanged_1(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                ShowInTaskbar = false;
            }
            else
            {
                ShowInTaskbar = true;
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;
            if (!(sender is System.Windows.Controls.ListBox listBox)) return;

            if (listBox.SelectedItem is Category catObj)
                Vm.ChangeFilterType(catObj.Tag);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Vm.SlowMode();
        }

        private void ListBox_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //if (CategoriesListBox.SelectedItem == null)
            //    return;

            //if (CategoriesListBox.SelectedItem is Category cat)
            //{
            //    FilterTextBox.Text = $"{{p}}:{cat.FullPath}";
            //}
        }

        private void Border_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //FilterTextBox.Clear();
            //CategoriesListBox.SelectedIndex = -1;
        }

        private void FilterTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            //if (!IsLoaded)
            //    return;
            //Vm.FilterText = FilterTextBox.Text;
            //Vm.UpdateSorting();

        }

        private void SetLocation_OnClick(object sender, RoutedEventArgs e)
        {
            Vm.SetLocation();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var dd = new Windows.MessageBox(new About());
            dd.ShowDialog(this);
        }

    }
}
