using System.Diagnostics.CodeAnalysis;

namespace Kebler.QBittorrent
{
    /// <summary>
    /// Bittorrent protocol.
    /// </summary>
    public enum BittorrentProtocol
    {
        /// <summary>
        /// Both TCP and uTP protocols.
        /// </summary>
        Both = 0,

        /// <summary>
        /// TCP only.
        /// </summary>
        Tcp = 1,

        /// <summary>
        /// uTP only.
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        uTP = 2
    }
}
