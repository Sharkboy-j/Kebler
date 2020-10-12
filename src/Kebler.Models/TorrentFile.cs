using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace Kebler.Models
{
    public class TorrentFile
    {
        private readonly ObservableCollection<TorrentFile> _children = new ObservableCollection<TorrentFile>();
        public ObservableCollection<TorrentFile> Children
        {
            get { return _children; }
        }

        public TorrentFile? Parent;

        public string Name { get; set; }
        public int Id { get; set; }
        public long Size { get; set; }
        public double Done { get; set; }
        public double DonePercent { get; set; }

        public bool? Checked { get; set; } = true;
        public uint Index;

        static int _i;
        private bool @checked;

        public TorrentFile(string name, long size, long done, bool check,uint index)
        {
            Id = ++_i;
            this.Name = name;
            this.Size = size;
            Done = done;
            Checked = check;
            Index = index;
            //size  :   1
            //done  :   ?
            if(size>0)
            {
                double p = (done * 100) / size;
                DonePercent = Math.Round(p, 1);
            }
        }

        public TorrentFile(string name)
        {
            Id = ++_i;
            this.Name = name;
        }

        public TorrentFile()
        {
            Id = ++_i;
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
