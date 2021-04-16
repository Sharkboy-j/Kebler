using Kebler.Resources;
using Microsoft.AppCenter.Crashes;
using System;
using System.Resources;
using System.Windows;
using WPFLocalizeExtension.Engine;
using WPFLocalizeExtension.Providers;

namespace Kebler.Services
{
    public static class LocalizationProvider
    {
        public static string GetLocalizedValue(string key)
        {
            ResourceManager temp = new ResourceManager("Kebler.Resources.Strings", typeof(Strings).Assembly);
            var value = temp.GetString(key, LocalizeDictionary.Instance.Culture);
            if (value is null)
            {
                Crashes.TrackError(new Exception($"Not found localization {key}"));
                return string.Empty;
            }

            return value;
        }
    }
}