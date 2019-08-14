using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Win32;

namespace Kebler.Services
{
    public class FileAssociation
    {
        public string Extension { get; set; }
        public string ProgId { get; set; }
        public string FileTypeDescription { get; set; }
        public string ExecutableFilePath { get; set; }
    }

    public class FileAssociations
    {
        // needed so that Explorer windows get refreshed after the registry is updated
        [System.Runtime.InteropServices.DllImport("Shell32.dll")]
        private static extern int SHChangeNotify(int eventId, int flags, IntPtr item1, IntPtr item2);

        private const int SHCNE_ASSOCCHANGED = 0x8000000;
        private const int SHCNF_FLUSH = 0x1000;

        public static void Create_abc_FileAssociation()
        {
            var perm = ".torrent";
            /***********************************/
            /**** Key1: Create ".abc" entry ****/
            /***********************************/
            Microsoft.Win32.RegistryKey key1 = Microsoft.Win32.Registry.ClassesRoot;

            //key1.CreateSubKey("Classes");
            //key1 = key1.OpenSubKey("Classes", true);

            var exepath = new FileInfo(Assembly.GetEntryAssembly().Location).Directory.ToString() + "\\Kebler.exe";

            key1.CreateSubKey($"{perm}");
            key1 = key1.OpenSubKey($"{perm}", true);
            key1.SetValue("", "DemoKeyValue"); // Set default key value

            key1.Close();

            /*******************************************************/
            /**** Key2: Create "DemoKeyValue\DefaultIcon" entry ****/
            /*******************************************************/
            Microsoft.Win32.RegistryKey key2 = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software", true);

            key2.CreateSubKey("Classes");
            key2 = key2.OpenSubKey("Classes", true);

            key2.CreateSubKey("DemoKeyValue");
            key2 = key2.OpenSubKey("DemoKeyValue", true);

            key2.CreateSubKey("DefaultIcon");
            key2 = key2.OpenSubKey("DefaultIcon", true);
            key2.SetValue("", "\"" + exepath + "\""); // Set default key value

            key2.Close();

            /**************************************************************/
            /**** Key3: Create "DemoKeyValue\shell\open\command" entry ****/
            /**************************************************************/
            Microsoft.Win32.RegistryKey key3 = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software", true);

            key3.CreateSubKey("Classes");
            key3 = key3.OpenSubKey("Classes", true);

            key3.CreateSubKey("DemoKeyValue");
            key3 = key3.OpenSubKey("DemoKeyValue", true);

            key3.CreateSubKey("shell");
            key3 = key3.OpenSubKey("shell", true);

            key3.CreateSubKey("open");
            key3 = key3.OpenSubKey("open", true);

            key3.CreateSubKey("command");
            key3 = key3.OpenSubKey("command", true);
            key3.SetValue("", "\"" + exepath + "\"" + " \"%1\""); // Set default key value

            key3.Close();

            SHChangeNotify(0x08000000, 0x0000, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
