using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using Kebler.Models.Torrent;
using Kebler.Models.Tree;

namespace Kebler.Models.PsevdoVM
{
    public class FilesTreeViewModel : INotifyPropertyChanged
    {
        public MultiselectionTreeViewItem Files { get; set; } = new MultiselectionTreeViewItem();
        public event PropertyChangedEventHandler PropertyChanged;

        public Thickness Border { get; set; } = new Thickness(0);







        public void UpdateFilesTree(ref TorrentInfo torrent)
        {

            var items = createTree(ref torrent);

            var newItems = new MultiselectionTreeViewItem { IsExpanded = true };
            newItems.Children.Add(items);

            Files = newItems;
        }

        private static MultiselectionTreeViewItem createTree(ref TorrentInfo torrent)
        {

            var root = new MultiselectionTreeViewItem { Title = torrent.Name };
            foreach (var itm in torrent.Files)
            {
                itm.Name = itm.Name.Replace(torrent.Name, string.Empty).TrimStart('/', '\\');
            }

            var count = 0;
            foreach (var file in torrent.Files)
            {
                createNodes(ref root, Path.Combine(file.Name), count);
                count++;
            }

            return root;
        }


        private static void createNodes(ref MultiselectionTreeViewItem root, string file, int index)
        {
            file = file.Replace('/', '\\');
            var dirs = file.Split('\\');

            var last = root;
            foreach (var s in dirs)
            {
                if (string.IsNullOrEmpty(s))
                    continue;

                if (last.Children.All(x => x.Title != s))
                {
                    //if not found children
                    var pth = new MultiselectionTreeViewItem { Title = s, IsExpanded = true };
                    last.Children.Add(pth);
                    last = pth;
                }
                else
                {
                    last = last.Children.First(p => p.Title == s);
                }
            }
        }



        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
