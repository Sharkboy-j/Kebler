using System;
using System.Collections.Generic;
using System.IO;

namespace Kebler
{
    public static class Data
    {
        public const string ConfigName = "app.config";
        public static readonly List<string> LangList = new List<string> {"en-US"};

        public static DirectoryInfo GetDataPath()
		{
			var pth = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(Kebler));
			var dir = new DirectoryInfo(pth);

			if (!dir.Exists)
			{
				dir.Create();
			}
			return dir;
		}
    }
}
