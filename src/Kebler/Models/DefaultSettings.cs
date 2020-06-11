using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using Transmission.API.RPC.Entity;

namespace Kebler.Models
{
    public class DefaultSettings
    {
        public int SortType { get; set; } = 0;
        public string SortVal { get; set; } = nameof(TorrentInfo.UploadedEver);
        public CultureInfo Language { get; set; }
        public bool IsAddTorrentWindowShow { get; set; }
        public int UpdateTime { get; set; } = 5000;
        public int TorrentPeerLimit { get; set; } = 25;
        public double CategoriesWidth { get; set; } = 150;
        public double MainWindowWidth { get; set; } = 600;
        public double MainWindowHeight { get; set; } = 800;
        public WindowState MainWindowState { get; set; } = WindowState.Normal;
        public string ColumnSizes { get; set; } = string.Empty;

    }
}
