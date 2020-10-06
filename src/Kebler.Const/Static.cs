using System;
using System.IO;

namespace Kebler.Const
{
    public static class ConstStrings
    {
        public const string GITHUB_USER = "JeremiSharkboy";
        public const string CONFIGNAME = "app.config";

        public static string KeblerRoamingFolder =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nameof(Kebler));

        public static string KeblerAppFolderPath = Path.Combine(KeblerRoamingFolder, "App");

        public static string KeblerExepath =
            Path.Combine(KeblerAppFolderPath, $"{nameof(Kebler)}.exe");

        public static string InstallerName = "Installer";
        public static string InstallerExeName = $"{InstallerName}.exe";
        public static string InstallerExePath = Path.Combine(KeblerRoamingFolder, InstallerExeName);

        public static string TempInstallerFolder = Path.Combine(Path.GetTempPath(), nameof(Kebler));
        public static string TempInstallerExePath = Path.Combine(TempInstallerFolder, InstallerExeName);
        public static string CONFIGPATH = Path.Combine(GetDataPath().FullName, CONFIGNAME);


        public static DirectoryInfo GetDataPath()
        {
            var dir = new DirectoryInfo(KeblerRoamingFolder);

            if (!dir.Exists) dir.Create();
            return dir;
        }
    }
}