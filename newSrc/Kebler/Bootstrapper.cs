using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using Caliburn.Micro;
using Kebler.Domain;
using Kebler.Domain.Interfaces;
using Kebler.Services;
using Kebler.UI;
using Kebler.UI.Controls;
using Kebler.ViewModels;
using Kebler.ViewModels.DialogWindows;

namespace Kebler
{
    public class Bootstrapper : BootstrapperBase
    {
        private readonly SimpleContainer _container = new();

        private IConfigService _configService;// = IoC.Get<IConfigService>();
        private ILocalizationManager _localizationManager;// = IoC.Get<ILocalizationManager>();

        public Bootstrapper()
        {
            ViewLocator.AddNamespaceMapping("Kebler.ViewModels", new[] { "Kebler.Views", "Kebler.UI.Dialogs" });
            ViewLocator.AddNamespaceMapping("Kebler.ViewModels.DialogWindows", new[] { "Kebler.Views" });
            Initialize();
        }

        protected override void Configure()
        {

            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.Singleton<IConfigService, ConfigService>();
            _container.Singleton<ILogger, Logger>();
            _container.Singleton<ILocalizationManager, LocalizationManager>();
            _container.Singleton<ICustomLocalizationProvider, LocalizationProvider>();
            _container.Singleton<ITorrentClientsWorker, TorrentClientsWorker>();
            _container.Singleton<KeblerViewModel>();
            _container.Singleton<DialogBoxViewModel>();
            _container.Singleton<MessageBoxViewModel>();
            _container.Singleton<BoxViewModel>();
            _container.Singleton<AboutViewModel>();

            _configService = ConfigService.Instance;
            _localizationManager = LocalizationManager.Instance;

            //container.Singleton<TaskBarIconViewModel>();
            //container.Singleton<TorrentPropsViewModel>();
            //container.Singleton<RemoveListDialogViewModel>();
            //container.Singleton<AddTorrentViewModel>();

            _container.PerRequest<ConnectionManagerViewModel>();

        }


        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            ThemeManager.ChangeAppTheme(themeName: ConstStrings.ThemeName);

            FrameworkElement.StyleProperty.OverrideMetadata(typeof(CustomWindow), new FrameworkPropertyMetadata
            {
                DefaultValue = Application.FindResource(typeof(CustomWindow))
            });

            FrameworkElement.StyleProperty.OverrideMetadata(typeof(DialogWindow), new FrameworkPropertyMetadata
            {
                DefaultValue = Application.FindResource(typeof(DialogWindow))
            });

            if (string.IsNullOrEmpty(ConfigService.DefaultSettingsInstanse.Language.Name))
                _localizationManager.CurrentCulture = _localizationManager.CultureList[0];
            else
                _localizationManager.CurrentCulture = _localizationManager.CultureList.First(x =>
                    x.TwoLetterISOLanguageName == ConfigService.DefaultSettingsInstanse.Language.TwoLetterISOLanguageName);
                    
            await DisplayRootViewFor<KeblerViewModel>();

#if RELEASE
           //System.Threading.Tasks.Task.Run(Updater.CheckUpdates);
#endif
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            var assemblies = base.SelectAssemblies().ToList();
            //assemblies.Add(typeof(RemoveListDialogView).GetTypeInfo().Assembly);

            return assemblies;
        }
    }
}