using System.Diagnostics;
using System.IO;

namespace Kebler.Models
{
    [DebuggerDisplay("{FolderName}")]
    public class FolderCategory
    {
        public FolderCategory(string fullPath)
        {
            _nativePath = NormalizePath(fullPath);
            Dir = new DirectoryInfo(_nativePath);
        }

        private readonly string _nativePath;

        public string FolderName => Dir.Name;
        public string FullPath => _nativePath;
        public readonly DirectoryInfo Dir;

        public string Title { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is FolderCategory cat)
            {
                return cat._nativePath.Equals(this._nativePath);
            }
            return false;
        }


        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _nativePath.GetHashCode();
                hashCode = (hashCode * 397) ^ hashCode;
                return hashCode;
            }
        }

        public static string NormalizePath(string path)
        {
            if (!Path.EndsInDirectorySeparator(path))
            {
                path = $"{path}{Path.AltDirectorySeparatorChar}";
            }
            
            return path;
        }
        public override string ToString()
        {
            return $"'{_nativePath}'";
        }
    }

    public class StatusCategory
    {
        public string Title { get; set; }
        public Enums.Categories Cat { get; set; }
    }
}
