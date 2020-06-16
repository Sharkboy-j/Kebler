using System.Globalization;
using Kebler.Models.Torrent;

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
        public byte MainWindowState { get; set; } = (byte)WindowState.Normal;
        public string ColumnSizes { get; set; } = string.Empty;

    }
    public enum WindowState
    {
        /// <summary>The window is restored.</summary>
        Normal,
        /// <summary>The window is minimized.</summary>
        Minimized,
        /// <summary>The window is maximized.</summary>
        Maximized,
    }
}
