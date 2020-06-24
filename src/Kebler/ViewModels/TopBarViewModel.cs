using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Kebler.Models;
using Kebler.Services;

namespace Kebler.ViewModels
{
    public class TopBarViewModel : PropertyChangedBase,
        IHandle<Messages.LocalizationCultureChangesMessage>,
        IHandle<Messages.ConnectedServerChanged>,
        IHandle<Messages.ServerRemoved>,
        IHandle<Messages.ServerAdded>
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

            ReInitServers();
        }


        private void ReInitServers()
        {

            var _dbServersList = StorageRepository.GetServersList();

            var items = _dbServersList?.FindAll() ?? new List<Server>();


            foreach (var srv in items)
            {
                if (Contains(srv))
                    continue;
                var dd = new MenuItem { Header = srv.Title, Tag = srv };
                dd.Click += srvClicked;
                dd.IsCheckable = true;
                Servers.Add(dd);
            }

        }


        private bool Contains(Server srv)
        {
            foreach (var item in Servers)
            {
                if (item is MenuItem mi && mi.Tag is Server serv)
                {
                    if (serv.Equals(srv))
                        return true;
                }
            }
            return false;
        }

        private void srvClicked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                var srv = mi.Tag as Server;

                foreach (var item in Servers)
                {
                    item.IsChecked = false;
                }

                _eventAggregator.PublishOnUIThreadAsync(new Messages.ReconnectRequested { srv = srv });
            }
        }


        private void langChanged(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi)
            {
                var ci = mi.Tag as CultureInfo;
                foreach (var item in Languages)
                {
                    item.IsChecked = false;
                }
                LocalizationManager.CurrentCulture = ci;
            }
        }





        private BindableCollection<MenuItem> _languages = new BindableCollection<MenuItem>();
        public BindableCollection<MenuItem> Languages
        {
            get => _languages;
            set => Set(ref _languages, value);
        }

        private BindableCollection<MenuItem> _servers = new BindableCollection<MenuItem>();
        public BindableCollection<MenuItem> Servers
        {
            get => _servers;
            set => Set(ref _servers, value);
        }

 

        public Task HandleAsync(Messages.LocalizationCultureChangesMessage message, CancellationToken cancellationToken)
        {
            foreach (var item in Languages)
            {
                item.IsChecked = Equals((CultureInfo)item.Tag, message.Culture);
            }

            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.ConnectedServerChanged message, CancellationToken cancellationToken)
        {
            if (message.srv == null)
            {
                foreach (var item in Servers)
                {
                    item.IsChecked = false;
                }
                return Task.CompletedTask;
            }

            foreach (var item in Servers)
            {
                if (item.Tag is Server srv)
                {
                    if (srv.Equals(message.srv))
                    {
                        item.IsChecked = true;
                        break;
                    }
                }
            }
            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.ServerAdded message, CancellationToken cancellationToken)
        {
            ReInitServers();
            return Task.CompletedTask;
        }

        public Task HandleAsync(Messages.ServerRemoved message, CancellationToken cancellationToken)
        {
            var rm = Servers.FirstOrDefault(x => ((Server)x.Tag).Equals(message.srv));

            Servers.Remove(rm);

            return Task.CompletedTask;
        }
    }
}
