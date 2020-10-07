using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Caliburn.Micro;
using Kebler.Models;
using WPFLocalizeExtension.Engine;
using ILog = log4net.ILog;
using LogManager = log4net.LogManager;

namespace Kebler.Services
{
    public static class LocalizationManager
    {
        private static List<CultureInfo>? _cultureList;
        private static readonly ILog Log = LogManager.GetLogger(typeof(LocalizationManager));

        private static CultureInfo? _currentCulture;

        public static List<CultureInfo> CultureList
        {
            get
            {
                return _cultureList ??= new List<CultureInfo>
                {
                    new CultureInfo("en"),
                    new CultureInfo("ru")
                };
            }
        }

        public static CultureInfo? CurrentCulture
        {
            get => _currentCulture;
            set
            {
                try
                {
                    _currentCulture = value;
                    ConfigService.Instanse.Language = _currentCulture;
                    ConfigService.Save();

                    var ea = IoC.Get<IEventAggregator>();

                    ea.PublishOnUIThreadAsync(new Messages.LocalizationCultureChangesMessage
                        {Culture = CurrentCulture});

                    SetCurrentThreadCulture(CurrentCulture);
                    Log.Info($"Lang changed {_currentCulture}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }


        private static void SetCurrentThreadCulture(CultureInfo? culture)
        {
            try
            {
                if (culture != null)
                {
                    Thread.CurrentThread.CurrentUICulture = culture;
                    Thread.CurrentThread.CurrentCulture =
                        CultureInfo.CreateSpecificCulture(culture.TwoLetterISOLanguageName);

                    CultureInfo.DefaultThreadCurrentCulture = culture;
                    CultureInfo.DefaultThreadCurrentUICulture = culture;

                    LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
                    LocalizeDictionary.Instance.Culture = culture;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }
    }
}