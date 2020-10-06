using System.Diagnostics;

namespace Kebler.UI.CSControls.MultiTreeView
{
    [DebuggerDisplay("[{ChangeType}] {FilePath}")]
    public class TorrentTreeFile
    {
        public TorrentTreeFile(string filePath)
        {
            FilePath = filePath;
        }


        //public string OldFilePath { get; }

        public string FilePath { get; }

        //public bool Staged { get; }

        //public bool Tracked { get; }

        //public bool New { get; }

        public bool IsDirectory { get; }

        //public string TreeIsh { get; }

        //public string FileMode { get; }

        //internal bool ChangedFileEquals(ChangedFile other)
        //{
        //    return this.FilePath == other.FilePath && this.Staged == other.Staged && this.TreeIsh == other.TreeIsh;
        //}
    }
}