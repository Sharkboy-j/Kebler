using System;
using System.Linq;
using System.Reflection;
using Caliburn.Micro;
using Kebler.Models.Torrent.Args;
using Newtonsoft.Json;

namespace Kebler.ViewModels
{
    public class ServerPreferencesViewModel : Screen
    {
        public SessionSettings _settings;
        private BindableCollection<prefs> prefs = new BindableCollection<prefs>();

        public ServerPreferencesViewModel(SessionSettings set)
        {
            _settings = set;

            var props = _settings.GetType().GetProperties().Where(
                prop => Attribute.IsDefined(prop, typeof(JsonPropertyAttribute)));


            foreach (PropertyInfo propertyInfo in props)
            {
                var dd = _settings.Get(propertyInfo.Name);
                var val = dd == null ? string.Empty : dd.ToString();
                prefs.Add(new prefs {Key = propertyInfo.Name, Value = val});
            }
        }

        public BindableCollection<prefs> Preferences
        {
            get => prefs;
            set => Set(ref prefs, value);
        }
    }

    public class prefs
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}