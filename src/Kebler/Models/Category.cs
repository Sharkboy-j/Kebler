using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Kebler.Models
{
    public class Category
    {


     
        public enum Categories : byte
        {
            All, Downloading, Active, Stopped, Ended, Error,Inactive
        }

        public string Title { get; set; }
        public Categories Tag { get; set; }

        public Brush Color { get; set; } // = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FE0000"));
    }
}
