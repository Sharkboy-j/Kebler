using System;
using System.IO;
using Kebler.Const;
using Kebler.Models;
using log4net;
using SharpConfig;

namespace Kebler.Services
{
    public static class ConfigService
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigService));
        private static Configuration ConfigurationObj;

        public static DefaultSettings Instanse;

        private static readonly object _sync = new object();

        public static void Save()
        {
            lock (_sync)
            {
                ConfigurationObj.Clear();
                ConfigurationObj.Add(Section.FromObject(nameof(DefaultSettings), Instanse));

                ConfigurationObj.SaveToFile(ConstStrings.CONFIGPATH);
            }
        }

        public static void LoadConfig()
        {
            lock (_sync)
            {
                if (IsExist())
                    LoadConfigurationFromFile();
                else
                    CreateNewConfig();
            }

            Log.Info($"Configuration:{Environment.NewLine}" + GetConfigString());
        }

        private static void CreateNewConfig()
        {
            ConfigurationObj = new Configuration();

            Instanse = new DefaultSettings();

            ConfigurationObj.Add(Section.FromObject(nameof(DefaultSettings), Instanse));

            ConfigurationObj.SaveToFile(ConstStrings.CONFIGPATH);

            // var p = Configuration[nameof(DefaultSettings)].ToObject<DefaultSettings>();
        }

        private static bool IsExist()
        {
            try
            {
                ConfigurationObj = Configuration.LoadFromFile(ConstStrings.CONFIGPATH);
                Log.Info("Configuration exists");
                return true;
            }
            catch (FileNotFoundException)
            {
                Log.Info("Configuration file not found");
                return false;
            }
        }

        private static void LoadConfigurationFromFile()
        {
            try
            {
                ConfigurationObj = Configuration.LoadFromFile(ConstStrings.CONFIGPATH);
                Instanse = ConfigurationObj[nameof(DefaultSettings)].ToObject<DefaultSettings>();
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                CreateNewConfig();
            }
        }


        private static string GetConfigString()
        {
            var text = string.Empty;

            foreach (var section in ConfigurationObj)
            {
                text += $"[{section.Name}]{Environment.NewLine}";

                foreach (var setting in section)
                {
                    if (setting.IsArray)
                        text += $"[Array, {setting.ArraySize} elements] ";

                    text += $"{setting}{Environment.NewLine}";
                }

                text += Environment.NewLine;
            }

            return text;
        }
    }
}