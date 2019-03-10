using System.Windows;
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
        private ConnectionManager CMWindow;


        public MainWindow()
        {
            log4net.Config.XmlConfigurator.Configure();

            InitializeComponent();

            Loaded += MainWindow_Loaded;

        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //CMWindow = new ConnectionManager();
            //CMWindow.ShowDialog();
        }


    }
}
