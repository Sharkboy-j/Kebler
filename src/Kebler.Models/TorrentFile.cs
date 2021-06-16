using Caliburn.Micro;
using System;
using System.Collections.ObjectModel;

namespace Kebler.Models
{
    public class TorrentFile : PropertyChangedBase
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
        public double Done
        {
            get => done; set
            {
                if (done != value)
                    Set(ref done, value);
            }
        }
        public double DonePercent
        {
            get => donePercent; set
            {
                if (donePercent != value)
                    Set(ref donePercent, value);
            }
        }

        public bool? Checked
        {
            get => checked1; set
            {
                if (checked1 != value)
                {
                    Set(ref checked1, value);
                }
            }
        }
        

        
        public uint Index;

        static int _i;
        public bool? checked1 = true;
        private double done;
        private double donePercent;

        public TorrentFile(string name, long size, long done, bool check, uint index)
        {
            Id = ++_i;
            Name = name;
            Size = size;
            Done = done;
            Checked = check;
            Index = index;
            //size  :   1
            //done  :   ?
            if (size > 0)
            {
                double p = (done * 100) / size;
                DonePercent = Math.Round(p, 1);
            }
        }

        public TorrentFile(string name)
        {
            Id = ++_i;
            Name = name;
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
