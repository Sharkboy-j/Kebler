using System.Windows;

namespace Kebler.Update
{
    /// <summary>
    /// Interaction logic for EXCEPTIONWINDOW.xaml
    /// </summary>
    public partial class EXCEPTIONWINDOW : Window
    {
        public EXCEPTIONWINDOW(string txt)
        {
            InitializeComponent();

            App.Instance.Log(txt);

            TXT.Text = App.Instance.BUILDER.ToString();
        }
    }
}
