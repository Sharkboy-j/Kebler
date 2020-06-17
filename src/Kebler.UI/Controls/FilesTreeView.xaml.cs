using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Kebler.Models;

namespace Kebler.UI.Controls
{
    /// <summary>
    /// Interaction logic for FilesTreeView.xaml
    /// </summary>
    public partial class FilesTreeView : UserControl
    {
        public FilesTreeView()
        {
            InitializeComponent();
        }

        public List<int> Unwanted = new List<int>();



        private void TreeItemUnChecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox ch && ch.Tag is TorrentPath trt && !trt.IsFolder)
            {
                Unwanted.Add(trt.Index);
                //Debug.WriteLine($"Uncheck {trt.Name} | Index {trt.Index} ");
            }
        }

        private void TreeItemChecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox ch && ch.Tag is TorrentPath trt && !trt.IsFolder)
            {
                Unwanted.Remove(trt.Index);
                //Debug.WriteLine($"Check {trt.Name} | Index {trt.Index} ");
            }
        }

        public void DisableBorders()
        {
            TreeView.BorderThickness = new Thickness(0);
            TreeView.Padding = new Thickness(0);
        }
    }
}
