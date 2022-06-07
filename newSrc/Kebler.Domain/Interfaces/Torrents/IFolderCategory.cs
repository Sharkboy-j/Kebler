namespace Kebler.Domain.Interfaces.Torrents
{
    public interface IFolderCategory
    {
        int Count { get; set; }
        string FolderName { get; }
        string FullPath { get; }
        string Title { get; set; }

        bool Equals(object obj);
        int GetHashCode();
        string ToString();
    }
}