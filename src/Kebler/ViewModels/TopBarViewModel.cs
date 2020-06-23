using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Kebler.Models;
using Kebler.Services;

namespace Kebler.ViewModels
{
    public class TopBarViewModel : PropertyChangedBase,IHandle<Messages.LocalizationCultureChangesMessage>
    {
        private readonly IEventAggregator _eventAggregator;


        public TopBarViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.SubscribeOnPublishedThread(this);

            foreach (var item in LocalizationManager.CultureList)
            {
                var dd = new MenuItem { Header = item.EnglishName, Tag = item };
                dd.Click += langChanged;
                dd.IsCheckable = true;
                dd.IsChecked = Equals(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName, item.TwoLetterISOLanguageName);
                Languages.Add(dd);
            }
        }

        private void langChanged(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                var ci = mi.Tag as CultureInfo;
                ChangeUi(ci);
                LocalizationManager.CurrentCulture = ci;
            }


        }


        private void ChangeUi(CultureInfo ci)
        {
            foreach (var item in Languages)
            {
                item.IsChecked = false;
            }
        }


        private BindableCollection<MenuItem> _languages = new BindableCollection<MenuItem>();
        public BindableCollection<MenuItem> Languages
        {
            get => _languages;
            set => Set(ref _languages, value);
        }

        public Task HandleAsync(Messages.LocalizationCultureChangesMessage message, CancellationToken cancellationToken)
        {
            foreach (var item in Languages)
            {
                item.IsChecked = Equals((CultureInfo)item.Tag, message.Culture);
            }

            return Task.CompletedTask;
        }
    }
}
