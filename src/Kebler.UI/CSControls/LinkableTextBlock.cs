using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace Kebler.UI.CSControls
{
    public class LinkableTextBlock : TextBlock
    {
        [DefaultValue(false)] [Localizability(LocalizationCategory.Text)]
        public static readonly DependencyProperty SureLinkProperty =
            DependencyProperty.Register(nameof(SureLink), typeof(bool),
                typeof(LinkableTextBlock),
                new FrameworkPropertyMetadata());


        [DefaultValue(false)] [Localizability(LocalizationCategory.Text)]
        public static readonly DependencyProperty CopyOnlyProperty =
            DependencyProperty.Register(nameof(CopyOnly), typeof(bool),
                typeof(LinkableTextBlock),
                new FrameworkPropertyMetadata());

        private bool isLink;

        static LinkableTextBlock()
        {
            var defaultMetadata = TextBox.TextProperty.GetMetadata(typeof(TextBlock));

            TextProperty.OverrideMetadata(typeof(LinkableTextBlock), new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.Journal | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                TextPropertyChanged,
                defaultMetadata.CoerceValueCallback,
                true,
                UpdateSourceTrigger.PropertyChanged));
        }

        public bool SureLink
        {
            get => (bool) GetValue(SureLinkProperty);
            set => SetValue(SureLinkProperty, value);
        }

        public bool CopyOnly
        {
            get => (bool) GetValue(CopyOnlyProperty);
            set => SetValue(CopyOnlyProperty, value);
        }


        private static object CoerceText(DependencyObject d, object baseValue)
        {
            return null;
        }


        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (CopyOnly)
                Clipboard.SetText(Text);
            else if (isLink) Process.Start(new ProcessStartInfo("cmd", $"/c start {Text}") {CreateNoWindow = true});
        }


        private static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var txt = sender as LinkableTextBlock;
            var bs = sender as TextBlock;
            var result = Uri.TryCreate(txt.Text, UriKind.Relative, out var uriResult);

            if (txt.SureLink || result)
            {
                txt.isLink = true;
                SetForeground(sender, Brushes.DodgerBlue);
            }


            if (txt.Text.Length > 0) bs.SetValue(TextProperty, txt.Text);
        }
    }
}