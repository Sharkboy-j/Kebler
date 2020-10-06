using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Kebler.Models.Tree;
using Kebler.UI.CSControls.MuliTreeView;
using static System.String;

namespace Kebler.UI.CSControls.MultiTreeView
{
    public class FileListItem : MultiselectionTreeViewItem
    {
        public FileListItem(TorrentTreeFile changedFile, string name, ImageSource fileTypeIcon)
        {
            ChangedFile = changedFile;
            ChangeTypeIcon = GetChangeTypeIcon(changedFile);
            Title = name;
            FileTypeIcon = fileTypeIcon;
            if (IsNullOrEmpty(name))
                return;
            FileName = Path.GetFileName(name);
            FolderPath = Path.GetDirectoryName(name);
        }

        public TorrentTreeFile ChangedFile { get; }

        public ImageSource ChangeTypeIcon { get; }

        public ImageSource FileTypeIcon { get; }

        public bool IsDirectory => ChangedFile.IsDirectory;

        public string FileName { get; }

        public string FolderPath { get; }

        protected override bool MatchFilter(string filterString)
        {
            return IsNullOrEmpty(filterString) ||
                   ChangedFile.FilePath.IndexOf(filterString, StringComparison.OrdinalIgnoreCase) != -1;
        }

        private static ImageSource GetChangeTypeIcon(TorrentTreeFile changedFile)
        {
            return null;
            //return changedFile.IsDirectory ? (ImageSource)null : changedFile.ChangeType.GetImageSource();
        }


        public override void StartDrag(DependencyObject dragSource, MultiselectionTreeViewItem[] nodes)
        {
            try
            {
                var num = (int) DragDrop.DoDragDrop(dragSource, GetDataObject(nodes), DragDropEffects.All);
            }
            catch
            {
                // ignored
            }
        }

        protected override IDataObject GetDataObject(MultiselectionTreeViewItem[] nodes)
        {
            var dataObject = new DataObject();
            dataObject.SetData(FileListTreeView.DragItemsFormat, nodes);
            return dataObject;
        }
    }
}