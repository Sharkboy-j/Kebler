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
using ILog = Kebler.Domain.Interfaces.ILog;

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
            Initialize();
        }

        protected override void Configure()
        {

            _container.Singleton<IWindowManager, WindowManager>();
            _container.Singleton<IEventAggregator, EventAggregator>();
            _container.Singleton<IConfigService, ConfigService>();
            _container.Singleton<ILog, Log>();
            _container.Singleton<ILocalizationManager, LocalizationManager>();
            _container.Singleton<KeblerViewModel>();

            _configService = ConfigService.Instance;
            _localizationManager = LocalizationManager.Instance;

            _configService.LoadConfig();
            //container.Singleton<KeblerViewModel>();
            //container.Singleton<ConnectionManagerViewModel>();
            //container.Singleton<BoxViewModel>();
            //container.Singleton<DialogBoxViewModel>();
            //container.Singleton<MessageBoxViewModel>();
            //container.Singleton<TaskBarIconViewModel>();
            //container.Singleton<TorrentPropsViewModel>();
            //container.Singleton<RemoveListDialogViewModel>();
            //container.Singleton<AddTorrentViewModel>();
        }


        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            ReadTheme();

            FrameworkElement.StyleProperty.OverrideMetadata(typeof(CustomWindow), new FrameworkPropertyMetadata
            {
                DefaultValue = Application.FindResource(typeof(CustomWindow))
            });

            if (string.IsNullOrEmpty(_configService.DefaultSettingsInstanse.Language.Name))
                _localizationManager.CurrentCulture = _localizationManager.CultureList[0];
            else
                _localizationManager.CurrentCulture = _localizationManager.CultureList.First(x =>
                    x.TwoLetterISOLanguageName == _configService.DefaultSettingsInstanse.Language.TwoLetterISOLanguageName);
                    
            await DisplayRootViewFor<KeblerViewModel>();

#if RELEASE
           //System.Threading.Tasks.Task.Run(Updater.CheckUpdates);
#endif
        }

        private void ReadTheme()
        {
            ThemeManager.ChangeAppTheme(themeName: ConstStrings.ThemeName);
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