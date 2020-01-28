using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Media;

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

        public enum Categories : byte
        {
            All, Downloading, Active, Stopped, Ended, Error,Inactive
        }
        public string Title { get; set; }
        public Categories Tag { get; set; }

        public Brush Color { get; set; } = null; // = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FE0000"));
    }
}
