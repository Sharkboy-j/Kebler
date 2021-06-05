using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Kebler.Const;
using Kebler.Update.Core;

namespace Kebler.Update
{
    public partial class App : Application
    {
        public static StringBuilder BUILDER = new StringBuilder();
        public static bool Force;
        public static bool Beta;


        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var args = e.Args.ToList().ConvertAll(x => x.ToLower());

            Log("----ARGS----");
            foreach (var a in args)
            {
                Log(a);
            }
            Log("----ARGS----");


            if (args.Count > 0)
            {
                if (args.Contains(ConstStrings.Args.Beta) || args.Contains(ConstStrings.Args.BetaShort))
                {
                    Beta = true;
                    Log("Beta true");
                }
                if (args.Contains(ConstStrings.Args.Force) || args.Contains(ConstStrings.Args.ForceShort))
                {
                    Force = true;
                    Log("Force true");
                }

            }
        }


        public static void Log(string msg)
        {
            BUILDER.Append(msg + Environment.NewLine);
        }


        static void startKebler()
        {
            Process.Start(new ProcessStartInfo()
            {
                WorkingDirectory = ConstStrings.KeblerAppFolderPath,
                FileName = ConstStrings.KeblerExepath,
                Arguments = ConstStrings.KeblerAppFolderPath,
                UseShellExecute = true,
            });
        }

        public static void CreateShortcut()
        {
            var lnkFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{nameof(Kebler)}.lnk");
            Shortcut.Create(lnkFileName, ConstStrings.KeblerExepath,
                ConstStrings.KeblerAppFolderPath, ConstStrings.KeblerAppFolderPath, "", null, ConstStrings.KeblerExepath);
        }



        public static void DONE(bool isTrue)
        {
            File.AppendAllText("install.log", BUILDER.ToString());
            if (isTrue == false)
            {
                //
            }
            else if (isTrue)
            {
                //CreateShortcut();
                startKebler();
            }
            JUSTDIEMOTHERFUCKER();

        }

        public static void JUSTDIEMOTHERFUCKER()
        {
            Current.Shutdown();
            Environment.Exit(0);
        }


    }
}