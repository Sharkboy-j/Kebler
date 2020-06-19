using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

// ReSharper disable UnusedMember.Global
// ReSharper disable once MemberCanBePrivate.Global
namespace Kebler.UI.CSControls
{
    public class WaterBox : TextBox
    {
        [DefaultValue("")]
        [Localizability(LocalizationCategory.Text)]

        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string),
                typeof(WaterBox),
                new FrameworkPropertyMetadata(string.Empty));

        public string Watermark
        {
            get => (string)GetValue(WatermarkProperty);
            set => SetValue(WatermarkProperty, value);
        }

        static WaterBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WaterBox),
                new FrameworkPropertyMetadata(typeof(WaterBox)));

            TextProperty.OverrideMetadata(typeof(WaterBox),
                new FrameworkPropertyMetadata(TextPropertyChanged));
        }

        private static readonly DependencyPropertyKey RemoveWatermarkPropertyKey =
            DependencyProperty.RegisterReadOnly("RemoveWatermark", typeof(bool),
                typeof(WaterBox),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty RemoveWatermarkProperty =
            RemoveWatermarkPropertyKey.DependencyProperty;

        public bool RemoveWatermark => (bool)GetValue(RemoveWatermarkProperty);

        static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var watermarkTextBox = (WaterBox)sender;

            var textExists = watermarkTextBox.Text.Length > 0;
            if (textExists != watermarkTextBox.RemoveWatermark)
            {
                watermarkTextBox.SetValue(RemoveWatermarkPropertyKey, textExists);
            }
        }
    }
}
