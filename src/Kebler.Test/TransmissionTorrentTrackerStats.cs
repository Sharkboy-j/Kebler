using System.Linq;
using Kebler.Models.Torrent;
using NUnit.Framework;

namespace Kebler.Test
{
    internal class TransmissionTorrentTrackerStatsTest
    {
        [Test]
        public void TorrentEquals_true()
        {
            var first = new[]
            {
                new TransmissionTorrentTrackerStats {ID = 1},
                new TransmissionTorrentTrackerStats {ID = 2},
                new TransmissionTorrentTrackerStats {ID = 3}
            };

            var second = new[]
            {
                new TransmissionTorrentTrackerStats {ID = 1}
            };

            var nothing = first.Except(second).ToArray();
            Assert.IsTrue(nothing.Length == 2);
            Assert.IsTrue(nothing[0].ID == 2);
            Assert.IsTrue(nothing[1].ID == 3);
        }

        [Test]
        public void TorrentEquals_true2()
        {
            var first = new[]
            {
                new TransmissionTorrentTrackerStats {ID = 1},
                new TransmissionTorrentTrackerStats {ID = 2},
                new TransmissionTorrentTrackerStats {ID = 3}
            };

            var second = new[]
            {
                new TransmissionTorrentTrackerStats {ID = 1},
                new TransmissionTorrentTrackerStats {ID = 4}
            };

            var nothing = first.Except(second).ToArray();
            Assert.IsTrue(nothing.Length == 2);
            Assert.IsTrue(nothing[0].ID == 2);
            Assert.IsTrue(nothing[1].ID == 3);
        }

        [Test]
        public void TorrentEquals_true3()
        {
            var first = new[]
            {
                new TransmissionTorrentTrackerStats {ID = 1},
                new TransmissionTorrentTrackerStats {ID = 2},
                new TransmissionTorrentTrackerStats {ID = 3}
            };

            var second = new[]
            {
                new TransmissionTorrentTrackerStats {ID = 1},
                new TransmissionTorrentTrackerStats {ID = 4}
            };

            var nothing = second.Except(first).ToArray();
            Assert.IsTrue(nothing.Length == 1);
            Assert.IsTrue(nothing[0].ID == 4);
        }
    }
}