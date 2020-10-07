using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Kebler.Services;
using Kebler.UI.Dialogs;
using Kebler.ViewModels;

namespace Kebler
{
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer container;

        public Bootstrapper()
        {
            ViewLocator.AddNamespaceMapping("Kebler.ViewModels", new[] {"Kebler.Views", "Kebler.UI.Dialogs"});
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
            container.Singleton<RemoveListDialogViewModel>();
            container.Singleton<AddTorrentViewModel>();
        }


        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            if (string.IsNullOrEmpty(ConfigService.Instanse.Language.Name))
                LocalizationManager.CurrentCulture = LocalizationManager.CultureList[0];
            else
                LocalizationManager.CurrentCulture = LocalizationManager.CultureList.First(x =>
                    x.TwoLetterISOLanguageName == ConfigService.Instanse.Language.TwoLetterISOLanguageName);

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

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            var assemblies = base.SelectAssemblies().ToList();
            assemblies.Add(typeof(RemoveListDialogView).GetTypeInfo().Assembly);

            return assemblies;
        }
    }
}