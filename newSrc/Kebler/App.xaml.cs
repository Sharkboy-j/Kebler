using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Kebler.Domain;
using Kebler.UI;
using Kebler.Views;

namespace Kebler
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Bootstrapper boots;

        public App()
        {
            boots = new Bootstrapper();
        }
    }
}
