using System;
using System.IO;

namespace Kebler.Const
{
    public static class Strings
    {
        public const string GitHubRepo = "/JeremiSharkboy/Kebler";

        public const string GithubRegex =
            @"\/releases\/download\/[0-9]\.[0-9]\.[0-9]\.[0-9]\/[R][e][l][e][a][s][e].[0-9]\.[0-9]\.[0-9]\.[0-9]\.zip";


        public static string KeblerRoamingFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(Kebler));

        public static string KeblerAppFolderPath =
            Path.Combine(KeblerRoamingFolder, "App");

        public static string KeblerExepath =
            Path.Combine(KeblerAppFolderPath, $"{nameof(Kebler)}.exe");

        public static string InstallerName = $"Installer";
        public static string InstallerExeName = $"Installer.exe";
        public static string InstallerExePath = Path.Combine(KeblerRoamingFolder, InstallerExeName);
    }
}
