using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

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
