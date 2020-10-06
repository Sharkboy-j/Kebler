using System.Windows;
using Caliburn.Micro;

namespace Kebler.ViewModels
{
    public class TaskBarIconViewModel : Screen
    {
        public void Exit()
        {
            Application.Current.Shutdown(0);
        }
    }
}