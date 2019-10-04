using Kebler.Models;
using LiteDB;
using System.Collections.Generic;

namespace Kebler
{
    public static class Data
    {
        public const string ConfigName = "app.config";

        public static readonly List<string> LangList = new List<string> { "en-US", "ru-RU" };

        public static LiteCollection<DefaultSettings> SettingsData;

    }
}
