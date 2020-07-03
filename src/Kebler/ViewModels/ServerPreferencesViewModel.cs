using Caliburn.Micro;
using Kebler.Models.Torrent.Args;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace Kebler.ViewModels
{
    public class ServerPreferencesViewModel : Screen
    {
        public ServerPreferencesViewModel(SessionSettings set)
        {
            _settings = set;

            var props = _settings.GetType().GetProperties().Where(
                   prop => Attribute.IsDefined(prop, typeof(JsonPropertyAttribute)));


            foreach (PropertyInfo propertyInfo in props)
            {
                var dd = _settings.Get(propertyInfo.Name);
                var val = dd == null ? string.Empty : dd.ToString();
                prefs.Add(new ViewModels.prefs() { Key = propertyInfo.Name, Value = val });
            }
        }

        public SessionSettings _settings;
        BindableCollection<prefs> prefs = new BindableCollection<prefs>();

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
