using System.Diagnostics;
using System.IO;

namespace Kebler.Models
{
    [DebuggerDisplay("{FolderName}")]
    public class FolderCategory
    {
        public readonly DirectoryInfo Dir;

        public FolderCategory(string fullPath)
        {
            FullPath = NormalizePath(fullPath);
            Dir = new DirectoryInfo(FullPath);
        }

        public string FolderName => Dir.Name;
        public string FullPath { get; }

        public string Title { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is FolderCategory cat) return cat.FullPath.Equals(FullPath);
            return false;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = FullPath.GetHashCode();
                hashCode = (hashCode * 397) ^ hashCode;
                return hashCode;
            }
        }

        public static string NormalizePath(string path)
        {
            if (!Path.EndsInDirectorySeparator(path)) path = $"{path}{Path.AltDirectorySeparatorChar}";

            return path;
        }

        public override string ToString()
        {
            return $"'{FullPath}'";
        }
    }

    public class StatusCategory
    {
        public string Title { get; set; }
        public Enums.Categories Cat { get; set; }
    }
}