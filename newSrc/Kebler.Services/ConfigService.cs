using System;
using System.IO;
using Caliburn.Micro;
using Kebler.Domain;
using Kebler.Domain.Interfaces;
using Kebler.Domain.Models;
using SharpConfig;

namespace Kebler.Services
{
    public class ConfigService : IConfigService
    {
        public bool IsInited { get; private set; }
        public static DefaultSettings DefaultSettingsInstanse;
        public static IConfigService Instance => _configService ??= new ConfigService();


        private static readonly ILogger Logger = Services.Logger.Instance;
        private static Configuration _configurationObj;

        private static IConfigService _configService;

        private static readonly object Sync = new();

        public void Save()
        {
            lock (Sync)
            {
                _configurationObj.Clear();
                _configurationObj.Add(Section.FromObject(nameof(DefaultSettings), DefaultSettingsInstanse));

                _configurationObj.SaveToFile(ConstStrings.Configpath);
            }
        }

        public void LoadConfig()
        {
            lock (Sync)
            {
                if (IsExist())
                    LoadConfigurationFromFile();
                else
                    CreateNewConfig();
            }

            Logger.Info($"Configuration:{Environment.NewLine}" + GetConfigString());
            IsInited = true;
        }

        private void CreateNewConfig()
        {
            _configurationObj = new Configuration();
            DefaultSettingsInstanse = new DefaultSettings();

            _configurationObj.Add(Section.FromObject(nameof(DefaultSettings), DefaultSettingsInstanse));

            _configurationObj.SaveToFile(ConstStrings.Configpath);

            // var p = Configuration[nameof(DefaultSettings)].ToObject<DefaultSettings>();
        }

        private static bool IsExist()
        {
            try
            {
                _configurationObj = Configuration.LoadFromFile(ConstStrings.Configpath);
                Logger.Info("Configuration exists");
                return true;
            }
            catch (FileNotFoundException)
            {
                Logger.Info("Configuration file not found");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            return false;

        }

        private void LoadConfigurationFromFile()
        {
            try
            {
                _configurationObj = Configuration.LoadFromFile(ConstStrings.Configpath);
                DefaultSettingsInstanse = _configurationObj[nameof(DefaultSettings)].ToObject<DefaultSettings>();
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                CreateNewConfig();
            }
        }

        private static string GetConfigString()
        {
            var text = string.Empty;

            lock (Sync)
            {
                foreach (var section in _configurationObj)
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
            }

            return text;
        }
    }
}