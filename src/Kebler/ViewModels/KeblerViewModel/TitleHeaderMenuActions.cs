using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
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
        #region Title header

        /// <summary>
        /// Initialise menu.
        /// </summary>
        private void InitMenu()
        {
            foreach (var item in LocalizationManager.CultureList)
            {
                var name = item.NativeName.ToCharArray();
                if (name.Length > 0)
                {
                    name[0] = Char.ToUpper(name[0]);
                }


                var menuItem = new MenuItem {Header = new string(name), Tag = item};
                menuItem.Click += LanguageChanged;
                menuItem.IsCheckable = true;
                menuItem.IsChecked = Equals(Thread.CurrentThread.CurrentCulture.TwoLetterISOLanguageName,
                    item.TwoLetterISOLanguageName);
                Languages.Add(menuItem);
            }

            ReInitServers();
        }

        /// <summary>
        /// Button Kebler Settings
        /// </summary>
        public void Settings()
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {ConstStrings.CONFIGPATH}") {CreateNoWindow = true});
        }


        /// <summary>
        /// Button Server preferences.
        /// </summary>
        public void Preferences()
        {
            if (_settings != null)
                manager.ShowDialogAsync(new ServerPreferencesViewModel(_settings));
            else
            {
                Log.Error($"{nameof(KeblerViewModel)}.{nameof(Preferences)}() => {nameof(_settings)} is null");
            }
        }


        /// <summary>
        /// Reinitialise servers.
        /// </summary>
        private void ReInitServers()
        {
            var dbServersList = StorageRepository.GetServersList();

            var items = dbServersList.FindAll() ?? new List<Server>();

            Servers.Clear();

            foreach (var srv in items)
            {
                var dd = new MenuItem {Header = srv.Title, Tag = srv};
                dd.Click += ServerClicked;
                dd.IsCheckable = true;
                Servers.Add(dd);
            }
        }


        /// <summary>
        /// MenuButton server clicked event-action.
        /// </summary>
        private void ServerClicked(object sender, RoutedEventArgs e)
        {
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
            if (!(sender is MenuItem menuItem) || !(menuItem.Tag is CultureInfo cultureInfo))
                return;
            
            foreach (var item in Languages)
                item.IsChecked = false;

            LocalizationManager.CurrentCulture = cultureInfo;
        }

        #endregion
    }
}