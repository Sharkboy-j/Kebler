using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Kebler.Models.PsevdoVM;
using Kebler.Models.Torrent;
using Kebler.Models.Tree;

namespace Kebler.Models
{
    public class MoreInfoModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public FilesTreeViewModel FilesTree{ get; private set; } = new FilesTreeViewModel();
        public bool Loading { get; set; }
        public bool IsMore { get; set; }
        public int SelectedCount { get; set; }
        public double PercentDone { get; set; }


        public void Clear()
        {
            FilesTree.Files = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

      
    }
}
