using System;
using Caliburn.Micro;
using Kebler.Domain.Interfaces;
using WPFLocalizeExtension.Engine;

namespace Kebler.Services
{
    public class LocalizationProvider : ICustomLocalizationProvider
    {
        public string GetLocalizedValue(string key)
        {
            try
            {
                var value = LocalizeDictionary.Instance.GetLocalizedObject(key, null, LocalizeDictionary.Instance.Culture) as string;

                if (string.IsNullOrEmpty(value))
                {
                    IoC.Get<ILogger>().Error(new Exception($"Not found localised key {key}"));
                    return key;
                }

                return value;
            }
            catch (Exception ex)
            {
                IoC.Get<ILogger>().Error(ex);
                return key;
            }
        }
    }
}
