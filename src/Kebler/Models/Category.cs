using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using static Kebler.Models.Enums;

namespace Kebler.Models
{
    [DebuggerDisplay("{FolderName}")]
    public class Category
    {
        public Category(string fullPath)
        {
            nativePath = fullPath;
            dirInfo = new DirectoryInfo(nativePath);
        }

        public Category(string title, Brush color)
        {

        }

        public Category()
        {

        }

        private readonly string nativePath;
        private readonly DirectoryInfo dirInfo;

        public bool IsFolder => dirInfo != null;
        public string FolderName => dirInfo?.Name;
        public string FullPath => nativePath;

      
        public string Title { get; set; }
        public Categories Tag { get; set; }

        public Brush Color { get; set; } = null; // = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FE0000"));
    }
}
