using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using log4net;

namespace Kebler.UI.Windows
{
    /// <inheritdoc cref="MainWindow" />
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public MainWindow()
        {
            log4net.Config.XmlConfigurator.Configure();

            InitializeComponent();
        }

      
    }
}
