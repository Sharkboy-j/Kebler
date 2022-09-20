using System.Windows;
using Caliburn.Micro;
using NLog;

namespace Kebler.ViewModels
{
    public class TaskBarIconViewModel : Screen
    {
        private ILogger Log = NLog.LogManager.GetCurrentClassLogger();


        public void Exit()
        {
            Log.Info("-----------Exit-----------");

            Application.Current.Shutdown();
            Application.Current.Shutdown(0);
        }
    }
}