using Kebler.Models;
using log4net;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using WPFLocalizeExtension.Engine;

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
            get { return _currentCulture; }
            set
            {
                try
                {
                    _currentCulture = value;
                    ConfigService.Instanse.Language = _currentCulture;
                    ConfigService.Save();
                    SetCurrentThreadCulture(CurrentCulture);
                    App.Instance.LangChangedNotify();
                    Log.Info($"Lang changed {_currentCulture}");
                }
                catch(Exception ex)
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
            catch(Exception ex)
            {
                Log.Error(ex);
            }
           


        }

    }
}
