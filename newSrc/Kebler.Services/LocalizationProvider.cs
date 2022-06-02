using System;
using System.Resources;
using Caliburn.Micro;
using Kebler.Domain.Interfaces;
using Microsoft.VisualBasic;
using WPFLocalizeExtension.Engine;

namespace Kebler.Services
{
    public class LocalizationProvider : ICustomLocalizationProvider
    {
        public string GetLocalizedValue(string key)
        {
            try
            {
                var temp = new ResourceManager("Kebler.Resources.Strings", typeof(Strings).Assembly);
                var value = temp.GetString(key, LocalizeDictionary.Instance.Culture);

                return value ?? key;

            }
            catch (Exception ex)
            {
                IoC.Get<ILogger>().Error(ex);
                return key;
            }
        }
    }
}
