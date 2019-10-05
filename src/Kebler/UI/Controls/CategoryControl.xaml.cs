using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Kebler.UI.Controls
{
    /// <summary>
    /// Interaction logic for CategoryControl.xaml
    /// </summary>
    public partial class CategoryControl : UserControl
    {
        public CategoryControl()
        {
            InitializeComponent();
        }

        private void CategoryControl_OnPreviewMouseMove(object sender, MouseEventArgs e)
        {
           // this.Background = (Brush)ColorConverter.ConvertFromString("#FFDFD991");
        }
    }
}
