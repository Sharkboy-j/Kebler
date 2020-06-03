using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Transmission.API.RPC.Entity;

namespace Kebler.Models
{
    public class DefaultSettings
    {
        public int SortType { get; set; } = 0;
        public string SortVal { get; set; } = nameof(TorrentInfo.UploadedEver);
        public CultureInfo Language { get; set; }
        public bool IsAddTorrentWindowShow { get; set; }

    }
}
