using System;
using System.Collections.Generic;
using System.Windows;
using Kebler.Models.Tree;
using Kebler.UI.CSControls.MultiTreeView;

namespace Kebler.UI.CSControls.MuliTreeView
{
    public class FileListTreeView : MultiselectionTreeView
    {
        public static readonly string DragItemsFormat = "FileListItems";
        public EventHandler<DropEventArgs> ItemsDrop;

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            e.Effects = DragDropEffects.None;
            if (!(e.Data.GetData(DragItemsFormat) is MultiselectionTreeViewItem[]))
                return;
            e.Handled = true;
            e.Effects = DragDropEffects.Move;
        }

        protected override void OnDrop(DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (!(e.Data.GetData(DragItemsFormat) is MultiselectionTreeViewItem[] data))
                return;
            e.Handled = true;
            e.Effects = DragDropEffects.Move;
            var files = data.CompactMap(x => !(x is FileListItem fileListItem) ? null : fileListItem.ChangedFile);
            var itemsDrop = ItemsDrop;
            itemsDrop?.Invoke(this, new DropEventArgs(files));
        }

        public class DropEventArgs : EventArgs
        {
            public DropEventArgs(TorrentTreeFile[] files)
            {
                Files = files;
            }

            public TorrentTreeFile[] Files { get; }
        }
    }

    public static class ex
    {
        public static TResult[] CompactMap<TSource, TResult>(
            this TSource[] source,
            Func<TSource, TResult> selector)
        {
            var resultList = new List<TResult>(source.Length);
            for (var index = 0; index < source.Length; ++index)
            {
                var result = selector(source[index]);
                if (result != null)
                    resultList.Add(result);
            }

            return resultList.ToArray();
        }
    }
}