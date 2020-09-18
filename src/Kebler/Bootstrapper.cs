using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Kebler.Services;
using Kebler.ViewModels;

namespace Kebler
{
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer container;

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            container = new SimpleContainer();

            container.Singleton<IWindowManager, WindowManager>();
            container.Singleton<IEventAggregator, EventAggregator>();
            container.Singleton<KeblerViewModel>();
            container.Singleton<ConnectionManagerViewModel>();
            container.Singleton<BoxViewModel>();
            container.Singleton<DialogBoxViewModel>();
            container.Singleton<MessageBoxViewModel>();
            container.Singleton<TaskBarIconViewModel>();
            container.Singleton<TorrentPropsViewModel>();

        }


        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            if (string.IsNullOrEmpty(ConfigService.Instanse.Language.Name))
            {
                LocalizationManager.CurrentCulture = LocalizationManager.CultureList[0];
            }
            else
            {
                LocalizationManager.CurrentCulture = LocalizationManager.CultureList.First(x => x.TwoLetterISOLanguageName == ConfigService.Instanse.Language.TwoLetterISOLanguageName);
            }

            await DisplayRootViewFor<KeblerViewModel>();
            await Updater.CheckUpdates();
        }

        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }
    }
}
