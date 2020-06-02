using Kebler.Models;
using LiteDB;
using SharpConfig;
using System;
using System.Collections.Generic;
using System.Text;
using log4net;
using System.IO;

namespace Kebler.Services
{
    public class ConfigService
    {
        private const string CONFIG_FILE_NAME = "settings.config";
        private static readonly ILog Log = LogManager.GetLogger(typeof(ConfigService));
        private static Configuration ConfigurationObj;

        public static DefaultSettings ConfigurationData;
        private static string CONFIG_NAME = Path.Combine(Data.GetDataPath().FullName, CONFIG_FILE_NAME);

        public static void Save()
        {
            ConfigurationObj.Clear();
            ConfigurationObj.Add(Section.FromObject(nameof(DefaultSettings), ConfigurationData));

            ConfigurationObj.SaveToFile(CONFIG_NAME);
        }

        public static void LoadConfig()
        {
            if (IsExist())
            {
                LoadConfigurationFromFile();
            }
            else
            {
                CreateNewConfig();
            }

            Log.Info($"Configuration:{Environment.NewLine}" + GetConfigString());
        }

        private static void CreateNewConfig()
        {
            ConfigurationObj = new Configuration();

            ConfigurationData = new DefaultSettings();

            ConfigurationObj.Add(Section.FromObject(nameof(DefaultSettings), ConfigurationData));

            ConfigurationObj.SaveToFile(CONFIG_NAME);

            // var p = Configuration[nameof(DefaultSettings)].ToObject<DefaultSettings>();
        }

        private static bool IsExist()
        {
            try
            {
                ConfigurationObj = Configuration.LoadFromFile(CONFIG_NAME);
                Log.Info("Configuration exists");
                return true;
            }
            catch (System.IO.FileNotFoundException)
            {
                Log.Info("Configuration file not found");
                return false;
            }


        }

        private static void LoadConfigurationFromFile()
        {
            try
            {
                ConfigurationObj = Configuration.LoadFromFile(CONFIG_NAME);
                ConfigurationData = ConfigurationObj[nameof(DefaultSettings)].ToObject<DefaultSettings>();
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
                    text += "  " + Environment.NewLine;

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
