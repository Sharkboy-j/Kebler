using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace Kebler.Models
{
    [DebuggerDisplay("{Name}({Count})")]
    public class TorrentPath : INotifyPropertyChanged
    {


        bool? _isChecked = false;
        TorrentPath _parent;

        public List<TorrentPath> Children { get; }
        public int Count => Children.Count;
        public bool IsFolder => Children.Count != 0;
        public string Name { get; }
        public int Index;


        public TorrentPath(string name, bool isChecked = true)
        {
            Name = name;
            IsChecked = isChecked;
            Children = new List<TorrentPath>();
        }

        public void Initialize()
        {
            foreach (var child in Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }

        public bool? IsChecked
        {
            get => _isChecked;
            set => SetIsChecked(value, true, true);
        }

        private void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
            {
                if (Children != null && Children.Count > 0)
                    Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));
            }

            if (updateParent)
                _parent?.VerifyCheckState();

            OnPropertyChanged("IsChecked");
        }

        private void VerifyCheckState()
        {
            bool? state = null;
            for (var i = 0; i < Children.Count; ++i)
            {
                var current = Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            SetIsChecked(state, false, true);
        }




        void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;


    }
}
