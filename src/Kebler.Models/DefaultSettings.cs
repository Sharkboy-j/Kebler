using System.Globalization;
using System.Windows;
using Kebler.Models.Torrent;

namespace Kebler.Models
{
    public class DefaultSettings
    {
        public int SortType { get; set; } = 0;
        public string SortVal { get; set; } = nameof(TorrentInfo.UploadedEver);
        public CultureInfo Language { get; set; } = new CultureInfo("en");
        public bool IsAddTorrentWindowShow { get; set; }
        public int UpdateTime { get; set; } = 5000;
        public int TorrentPeerLimit { get; set; } = 25;
        public double CategoriesWidth { get; set; } = 150;
        public double MoreInfoHeight { get; set; } = 202;
        public double MainWindowWidth { get; set; } = 600;
        public double MainWindowHeight { get; set; } = 800;
        public WindowState MainWindowState { get; set; } = WindowState.Normal;
        public string ColumnSizes { get; set; } = string.Empty;

    }
    
}
