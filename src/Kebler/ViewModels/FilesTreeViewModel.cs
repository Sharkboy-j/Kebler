using System;
using System.Linq;
using System.Windows;
using BencodeNET.Torrents;
using Caliburn.Micro;
using Kebler.Models.Interfaces;
using Kebler.Models.Tree;
using TorrentInfo = Kebler.Models.Torrent.TorrentInfo;

namespace Kebler.ViewModels
{
    public class FilesTreeViewModel : PropertyChangedBase, IFilesTreeView
    {
        private MultiselectionTreeViewItem _files = new MultiselectionTreeViewItem();

        public MultiselectionTreeViewItem? Files
        {
            get => _files;
            set => Set(ref _files, value);
        }

        public Thickness Border { get; set; } = new Thickness(0);


        public uint[] getFilesWantedStatus(bool status)
        {
            var output = new uint[0];
            foreach (var tm in Files.Children)
            {
                var ids = get(tm, status);
                merge(ref output, ref ids, out output);
            }

            return output;
        }

        private void merge(ref uint[] array1, ref uint[] array2, out uint[] result)
        {
            var array1OriginalLength = array1.Length;
            Array.Resize(ref array1, array1OriginalLength + array2.Length);
            Array.Copy(array2, 0, array1, array1OriginalLength, array2.Length);
            result = array1;
        }

        private uint[] get(MultiselectionTreeViewItem Node, bool search)
        {
            var output = new uint[0];
            if (Node.HasChildren)
            {
                foreach (var item in Node.Children)
                    if (item.HasChildren)
                    {
                        var ids = get(item, search);
                        merge(ref output, ref ids, out output);
                    }
                    else
                    {
                        if (item.IsChecked == search)
                        {
                            Array.Resize(ref output, output.Length + 1);
                            output[output.GetUpperBound(0)] = item.IndexPattern;
                            //dd.Add(item.IndexPattern);
                        }
                    }
            }
            else
            {
                if (Node.IsChecked == search)
                {
                    Array.Resize(ref output, output.Length + 1);
                    output[output.GetUpperBound(0)] = Node.IndexPattern;
                    //dd.Add(Node.IndexPattern);
                }
            }

            return output;
        }

        [Obsolete("Use UpdateFilesTree(Torrent) instead of UpdateFilesTree(TorrentInfo)")]
        public void UpdateFilesTree(TorrentInfo torrent)
        {
            var items = createTree(ref torrent);

            var newItems = new MultiselectionTreeViewItem {IsExpanded = true};
            newItems.Children.Add(items);

            Files = newItems;
        }
        
        public void UpdateFilesTree(Torrent torrent)
        {
            var items = createTree(torrent);

            var newItems = new MultiselectionTreeViewItem {IsExpanded = true};
            newItems.Children.Add(items);

            Files = newItems;
        }

        public void Clear()
        {
            Files?.Children.Clear();
        }

        private static MultiselectionTreeViewItem createTree(Torrent torrent)
        {
            var root = new MultiselectionTreeViewItem {Title = torrent.DisplayName};
            // foreach (var itm in torrent.Files)
            //     itm.Name = itm.Name.TrimStart('/', '\\');


            var count = 0U;

            foreach (var file in torrent.Files)
            {
                createNodes(ref root, file, count, true);
                count++;
            }
            
            // for (uint i = 0; i < torrent.Files.Count(); i++)
            // {
            //     // if (torrent.FileStats != null)
            //     //     createNodes(ref root, torrent.Files[i].Name, count, torrent.FileStats[i].Wanted);
            //     // else
            //     //     createNodes(ref root, torrent.Files[i].Name, count, true);
            //     // createNodes(ref root, torrent.Files[i].Name, count, true);
            //     // count++;
            // }

            return root;
        }
        private static void createNodes(ref MultiselectionTreeViewItem root, MultiFileInfo file, uint index, bool wanted)
        {
            var last = root;

            for (var i = 0; i < file.Path.Count; i++)
            {
                var pathPart = file.Path[i];
                if (string.IsNullOrEmpty(pathPart))
                    continue;

                if (last.Children.All(x => x.Title != pathPart))
                {
                    //if not found children
                    var pth = new MultiselectionTreeViewItem {Title = pathPart, IsExpanded = true};

                    //this will put all dirs into priority pos
                    last.Children.Insert(i == file.Path.Count - 1 ? last.Children.Count : 0, pth);
                    last = pth;
                }
                else
                {
                    last = last.Children.First(p => p.Title == pathPart);
                }
            }

            last.IndexPattern = index;
            last.IsChecked = wanted;
        }
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        
        private static MultiselectionTreeViewItem createTree(ref TorrentInfo torrent)
        {
            var root = new MultiselectionTreeViewItem {Title = torrent.Name};
            foreach (var itm in torrent.Files)
                itm.Name = itm.Name.TrimStart('/', '\\');


            var count = 0U;
            for (uint i = 0; i < torrent.Files.Length; i++)
            {
                if (torrent.FileStats != null)
                    createNodes(ref root, torrent.Files[i].Name, count, torrent.FileStats[i].Wanted);
                else
                    createNodes(ref root, torrent.Files[i].Name, count, true);

                count++;
            }

            return root;
        }
       

        private static void createNodes(ref MultiselectionTreeViewItem root, string file, uint index, bool wanted)
        {
            file = file.Replace('/', '\\');
            var dirs = file.Split('\\');

            var last = root;

            for (var i = 0; i < dirs.Length; i++)
            {
                var pathPart = dirs[i];
                if (string.IsNullOrEmpty(pathPart))
                    continue;

                if (last.Children.All(x => x.Title != pathPart))
                {
                    //if not found children
                    var pth = new MultiselectionTreeViewItem {Title = pathPart, IsExpanded = true};

                    //this will put all dirs into priority pos
                    last.Children.Insert(i == dirs.Length - 1 ? last.Children.Count : 0, pth);
                    last = pth;
                }
                else
                {
                    last = last.Children.First(p => p.Title == pathPart);
                }
            }

            last.IndexPattern = index;
            last.IsChecked = wanted;
        }
    }
}