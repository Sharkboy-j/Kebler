using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using Kebler.Const;
using Kebler.Models;
using Kebler.Services;

// ReSharper disable once CheckNamespace

namespace Kebler.ViewModels
{
    /// <summary>
    /// Title header menu actions.
    /// </summary>
    public partial class KeblerViewModel
    {
        /// <summary>
        /// Initialise languages title bar menu.
        /// </summary>
        /// <param name="cultureList">Cultures.</param>
        /// <param name="languageChanged">Event, that will be raised.</param>
        /// <returns><see cref="IEnumerable{T}"/> where T is <see cref="System.Windows.Controls.MenuItem"/></returns>
        public IEnumerable<MenuItem> InitMenu(
            IEnumerable<CultureInfo> cultureList,
            RoutedEventHandler languageChanged,
            CultureInfo checkingLanguage)
        {
            if (cultureList is null)
            {
                throw new ArgumentNullException(nameof(cultureList));
            }

            if (languageChanged is null)
            {
                throw new ArgumentNullException(nameof(languageChanged));
            }


            var languages = new List<MenuItem>();

            foreach (var item in cultureList)
            {
                var name = item.NativeName.ToCharArray();
                if (name.Length > 0)
                {
                    name[0] = Char.ToUpper(name[0]);
                }


                var menuItem = new MenuItem { Header = new string(name), Tag = item };
                menuItem.Click += languageChanged;
                menuItem.IsCheckable = true;
                menuItem.IsChecked = Equals(checkingLanguage.TwoLetterISOLanguageName, item.TwoLetterISOLanguageName);

                languages.Add(menuItem);
            }

            return languages;
        }


        /// <summary>
        /// Reinitialise servers.
        /// </summary>
        public IEnumerable<MenuItem> ReInitServers(
            IEnumerable<Server> dbServers,
            RoutedEventHandler serverClicked)
        {
            if (dbServers is null)
            {
                throw new ArgumentNullException(nameof(dbServers));
            }

            if (serverClicked is null)
            {
                throw new ArgumentNullException(nameof(serverClicked));
            }

            var servers = new List<MenuItem>();

            foreach (var server in dbServers)
            {
                var menuItem = new MenuItem { Header = server.Title, Tag = server };
                menuItem.Click += serverClicked;
                menuItem.IsCheckable = true;

                servers.Add(menuItem);
            }

            return servers;
        }


        /// <summary>
        /// Button Kebler Settings
        /// </summary>
        public void Settings()
        {
            Log.Ui();
            Log.Info($"Try start => cmd {ConstStrings.CONFIGPATH}");
            var p = new Process();
            p.StartInfo = new ProcessStartInfo(@ConstStrings.CONFIGPATH)
            {
                UseShellExecute = true
            };
            p.Start();
        }


        /// <summary>
        /// Button Server preferences.
        /// </summary>
        public void Preferences()
        {
            Log.Ui();

            if (_settings != null)
                manager.ShowDialogAsync(new ServerPreferencesViewModel(_settings));
            else
            {
                Log.Error($"{nameof(KeblerViewModel)}.{nameof(Preferences)}() => {nameof(_settings)} is null");
            }
        }


        /// <summary>
        /// MenuButton server clicked event-action.
        /// </summary>
        private void ServerClicked(object sender, RoutedEventArgs e)
        {
            Log.Ui();

            if (!(sender is MenuItem menuItem) || !(menuItem.Tag is Server server))
                return;

            foreach (var item in Servers) item.IsChecked = false;

            _eventAggregator.PublishOnUIThreadAsync(new Messages.ReconnectRequested(server));
        }

        /// <summary>
        /// MenuButton Intefrace language clicked event-action.
        /// </summary>
        private void LanguageChanged(object sender, RoutedEventArgs e)
        {
            Log.Ui();

            if (!(sender is MenuItem menuItem) || !(menuItem.Tag is CultureInfo cultureInfo))
                return;

            foreach (var item in Languages)
                item.IsChecked = false;

            LocalizationManager.CurrentCulture = cultureInfo;
        }
    }
}