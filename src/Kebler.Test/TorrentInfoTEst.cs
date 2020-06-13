using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Transmission.API.RPC.Entity;

namespace Kebler.Test
{
    class TorrentInfoTest
    {
        [Test]
        public void TorrentEquals_true()
        {
            var first = new TorrentInfo { ID = 32 };
            var second = new TorrentInfo { ID = 32 };

            var result = first.Equals(second);
            Assert.IsTrue(result);
            Assert.Pass();
        }

        [Test]
        public void TorrentEquals_false()
        {
            var first = new TorrentInfo { ID = 32 };
            var second = new TorrentInfo { ID = 17 };

            var result = first.Equals(second);
            Assert.IsFalse(result);
            Assert.Pass();
        }


        [Test]
        public void TorrentListExcept()
        {
            var tors = GetTorrents();
            var remove = GetTorrentsToRemove();

            var other = tors.Except(remove).ToArray();

            Assert.IsTrue(other.Count() == 1);
            Assert.IsTrue(other[0].ID == 2);
            Assert.IsTrue(other[0].HashString == "sadasdasdas");
            Assert.Pass();
        }


        private List<TorrentInfo> GetTorrents()
        {
            var torrents = new List<TorrentInfo>
            {
                new TorrentInfo { ID = 1, HashString = "wsvgbh", Name = "First" },
                new TorrentInfo { ID = 2, HashString = "sadasdasdas", Name = "Second" },
                new TorrentInfo { ID = 3, HashString = "asdasdasd", Name = "Third" }
            };
            return torrents;
        }

        private List<TorrentInfo> GetTorrentsToRemove()
        {
            var torrents = new List<TorrentInfo>
            {
                new TorrentInfo { ID = 1, HashString = "wsvgbh", Name = "First" },
                new TorrentInfo { ID = 3, HashString = "asdasdasd", Name = "Third" }
            };
            return torrents;
        }
    }
}
