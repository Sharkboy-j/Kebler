using System;
using System.Collections.Generic;
using System.IO;

namespace Kebler.Const
{
    public static class ConstStrings
    {
        public const string KEBLEREXE = "Kebler.exe";

        public static string InstallerName = $"Installer";
        public static string InstallerExeName = $"Installer.exe";
        public static string KeblerDBFileName = $"{nameof(Kebler)}.db";
        public const string GitHubRepo = "/JeremiSharkboy/Kebler";
        public const string ConfigName = "app.config";


        public const string GithubRegex =
            @"\/releases\/download\/[0-9]\.[0-9]\.[0-9]\.[0-9]\/[R][e][l][e][a][s][e].[0-9]\.[0-9]\.[0-9]\.[0-9]\.zip";
        public static readonly List<string> LangList = new List<string> { "en-US" };

        public static string KeblerRoamingFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(Kebler));

        public static string KeblerAppFolderPath = Path.Combine(KeblerRoamingFolder, "App");

        public static string KeblerExepath = Path.Combine(KeblerAppFolderPath, KEBLEREXE);



        public static string InstallerExePath = Path.Combine(KeblerRoamingFolder, InstallerExeName);
        public static string GetDataDBFilePath = Path.Combine(GetDataPath().FullName, KeblerDBFileName);

        public static DirectoryInfo GetDataPath()
        {
            var dir = new DirectoryInfo(KeblerRoamingFolder);

            if (!dir.Exists)
            {
                dir.Create();
            }
            return dir;
        }
    }
}
