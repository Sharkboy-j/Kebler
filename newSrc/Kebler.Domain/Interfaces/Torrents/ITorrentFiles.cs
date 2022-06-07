namespace Kebler.Domain.Interfaces.Torrents
{
    public interface ITorrentFiles
    {
        long BytesCompleted { get; set; }
        long Length { get; set; }
        string Name { get; set; }
        string[] NameParts { get; }
    }
}