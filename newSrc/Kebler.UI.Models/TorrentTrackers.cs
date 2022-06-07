using Caliburn.Micro;
using Kebler.Domain.Interfaces.Torrents;

namespace Kebler.UI.Models
{
    public class TorrentTrackers : PropertyChangedBase, ITorrentTrackers
    {
        private string _announce;
        private uint _id;
        private string _scrape;
        private int _tier;

        public string announce
        {
            get => _announce;
            set => Set(ref _announce, value);
        }

        public uint ID
        {
            get => _id;
            set => Set(ref _id, value);
        }

        public string Scrape
        {
            get => _scrape;
            set => Set(ref _scrape, value);
        }

        public int Tier
        {
            get => _tier;
            set => Set(ref _tier, value);
        }

        public override bool Equals(object obj)
        {
            if (obj is TorrentTrackers tracker)
            {
                return tracker.announce.ToLower().Equals(announce.ToLower());
            }
            return false;
        }

        public override int GetHashCode()
        {
            return announce.ToLower().GetHashCode();
        }
    }
}
