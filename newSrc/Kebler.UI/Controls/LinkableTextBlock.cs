using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Kebler.Domain;

namespace Kebler.UI.Controls
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

        private bool _isLink;

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
            return new object();
        }


        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if(Text.IsNotNullOrNotEmpty())
            {
                if (CopyOnly)
                    Clipboard.SetText(Text);
                else if (_isLink)
                {
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {Text}")
                    {
                        CreateNoWindow = true,
                        UseShellExecute = true
                    });
                }
            }
        }


        private static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is LinkableTextBlock ltb)
            {
                var bs = (TextBlock) sender;

                if(string.IsNullOrEmpty(ltb.Text))
                    return;
            
                var result = Uri.TryCreate(ltb.Text, UriKind.RelativeOrAbsolute, out _);

                if (ltb.SureLink || result)
                {
                    ltb._isLink = true;
                    SetForeground(sender, Brushes.DodgerBlue);
                }

                if (ltb.Text.Length > 0) bs.SetValue(TextProperty, ltb.Text); 
            }
        }
    }
}