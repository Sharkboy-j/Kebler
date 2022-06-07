namespace Kebler.QBittorrent
{
    /// <summary>
    /// A base class for RSS items: feeds and folders.
    /// </summary>
    /// <seealso cref="RssFeed"/>
    /// <seealso cref="RssFolder"/>
    public abstract class RssItem
    {
        /// <summary>
        /// Gets or sets RSS item name.
        /// </summary>
        public string Name { get; set; }
    }
}
