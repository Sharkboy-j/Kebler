using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace Kebler.UI
{
    public static class ThemeManager
    {
        //private static ILogger _logger = LogManager.GetCurrentClassLogger();

        private static ResourceDictionary _currentThemeResourceDictionary;

        public static void ChangeAppTheme(string themeName)
        {
            if (TryGetTheme(themeName: themeName, themeResources: out var themeResources))
            {
                UpdateAppResources(themeResources: themeResources);
            }
        }

        private static bool TryGetTheme(string themeName, out ResourceDictionary themeResources)
        {
            var result = true;
            themeResources = null;
            try
            {
                var uri = new Uri(uriString: $"pack://application:,,,/{nameof(Kebler)}.{nameof(UI)};component/Themes/{themeName}.xaml");
                themeResources = new ResourceDictionary { Source = uri };
            }
            catch (Exception)
            {
                //_logger.Error(  ex, message: $"Can not apply theme [{themeName}]. Check file exist");
                result = false;
            }

            return result;
        }

        private static void UpdateAppResources(ResourceDictionary themeResources)
        {
            var resources = Application.Current.Resources;

            Application.Current.Resources.MergedDictionaries.Insert(index: 0, item: themeResources);
            Application.Current.Resources.MergedDictionaries.Remove(item: _currentThemeResourceDictionary);

            //DeleteDoubleGenericResources(resources: resources);

            _currentThemeResourceDictionary = themeResources;
        }

        private static void DeleteDoubleGenericResources(ResourceDictionary resources)
        {
            var resourceDictionary = new Dictionary<Uri, ResourceDictionary>();

            var allResources = FindAllResources(resourcesDictionary: resources);
            foreach (var resource in allResources.Where(predicate: r => r.Source.ToString().Contains(value: "Generic")))
            {
                try
                {
                    resourceDictionary.Add(key: resource.Source, value: resource);
                }
                catch (ArgumentException)
                {
                    //_logger.Warn(message: $"Resource {resource.Source} already exists in the resources dictionary. It will be deleted");
                    Application.Current.Resources.Remove(key: resource.Source.ToString());
                }
            }

            resourceDictionary.Clear();
        }


        private static IEnumerable<ResourceDictionary> FindAllResources(ResourceDictionary resourcesDictionary)
        {
            var resourcesList = new List<ResourceDictionary>();

            foreach (var res in resourcesDictionary.MergedDictionaries)
            {
                if (res.Source != null)
                {
                    resourcesList.Add(item: res);
                }

                resourcesList.AddRange(collection: FindAllResources(resourcesDictionary: res));
            }

            return resourcesList;
        }


        private static bool AreResourceDictionarySourcesEqual(ResourceDictionary first, ResourceDictionary second)
        {
            if (first == null || second == null)
            {
                return false;
            }

            if (first.Source == null || second.Source == null)
            {
                try
                {
                    foreach (var key in first.Keys)
                    {
                        var isTheSame = second.Contains(key: key) && Equals(objA: first[key: key], objB: second[key: key]);
                        if (!isTheSame)
                        {
                            return false;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Trace.TraceError(message: $"Could not compare resource dictionaries: {exception} {Environment.NewLine} {exception.StackTrace}");
                    return false;
                }

                return true;
            }

            return Uri.Compare(uri1: first.Source, uri2: second.Source, partsToCompare: UriComponents.Host | UriComponents.Path, compareFormat: UriFormat.SafeUnescaped, comparisonType: StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
