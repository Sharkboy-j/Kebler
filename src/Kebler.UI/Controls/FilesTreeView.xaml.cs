﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Kebler.Models.PsevdoVM;
using Kebler.Models.Tree;

namespace Kebler.UI.Controls
{
    /// <summary>
    /// Interaction logic for FilesTreeView.xaml
    /// </summary>
    public partial class FilesTreeView
    {
        public FilesTreeView()
        {
            InitializeComponent();
        }

        public delegate void FileStatusUpdateHandler(uint[] wanted, uint[] unwanted, bool status);
        public event FileStatusUpdateHandler OnFileStatusUpdate;


        private void FolderCheck_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox chk)
            {
                if (Trree.SelectedItems.Count > 1)
                {
                    foreach (MultiselectionTreeViewItem item in Trree.SelectedItems)
                    {
                        item.IsChecked = (bool)chk.IsChecked;
                    }
                }
                else
                    Trree.UnselectAll();



                var file = chk.Tag as MultiselectionTreeViewItem;
                Trree.SelectAndFocus(file);


                var dd = this.DataContext as FilesTreeViewModel;

                if (OnFileStatusUpdate != null)
                    OnFileStatusUpdate.Invoke(dd.getFilesWantedStatus(true), dd.getFilesWantedStatus(false), (bool)file.IsChecked);

                //Debug.WriteLine($"IND = {ss.IndexPattern}");
            }

        }



        private void Trree_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Space)
            {
                var val = ((MultiselectionTreeViewItem)Trree.SelectedValue).IsChecked;

                if(val==null)
                {
                    ((MultiselectionTreeViewItem)Trree.SelectedValue).IsChecked = false;
                }
                else
                    ((MultiselectionTreeViewItem)Trree.SelectedValue).IsChecked = !val;
            }
        }
    }
}