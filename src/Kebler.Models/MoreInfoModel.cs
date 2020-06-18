using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Kebler.Models.Torrent;
using Kebler.Models.Tree;

namespace Kebler.Models
{
    public class MoreInfoModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        //public List<TorrentPath> Files { get; set; }
        public  MultiselectionTreeViewItem Files { get; set; } = new MultiselectionTreeViewItem();

        public TorrentInfo Selected { get; set; }
        public bool Loading { get; set; }
        public bool IsMore { get; set; }
        public int SelectedCount { get; set; }

        public void Clear()
        {
            Files = null;
            Selected = null;
        }
    }
}
