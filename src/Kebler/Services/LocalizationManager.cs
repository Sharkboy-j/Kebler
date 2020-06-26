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
        static List<CultureInfo> _cultureList;
        static readonly ILog Log = LogManager.GetLogger(typeof(LocalizationManager));

        public static List<CultureInfo> CultureList
        {
            get
            {
                if (_cultureList == null)
                {
                    _cultureList = new List<CultureInfo>
                    {
                        new CultureInfo("en"),
                        new CultureInfo("ru")
                    };
                }
                return _cultureList;
            }
        }

        public static CultureInfo CurrentCulture
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

                    ea.PublishOnUIThreadAsync(new Messages.LocalizationCultureChangesMessage { Culture = CurrentCulture });

                    SetCurrentThreadCulture(CurrentCulture);
                    //App.Instance.LangChangedNotify();
                    Log.Info($"Lang changed {_currentCulture}");
                }
                catch (Exception ex)
                {
                    Log.Error(ex);

                }

            }
        }
        static CultureInfo _currentCulture;



        public static void SetCurrentThreadCulture(CultureInfo culture = null)
        {
            try
            {
                if (culture == null) culture = CurrentCulture;

                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture.TwoLetterISOLanguageName);

                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;

                LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
                LocalizeDictionary.Instance.Culture = culture;
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }



        }



    }
}
