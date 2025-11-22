using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;


namespace StateNotifyByPop
{
    public static class ConfigManager
    {
        public class ConfigModel
        {
            public float water_limit = 0.3f;
            public float energy_limit = 0.3f;
            public float health_limit = 0.3f;
            public float stamina_limit = 0.3f;
            public bool enable_three_stage = true;

            public Dictionary<string, Dictionary<string, string>> localization = new Dictionary<string, Dictionary<string, string>>();
        }

        private const string ConfigFileName = "StateNotifyByPop.cfg";
        private static string _streamingPath => Path.Combine(Application.streamingAssetsPath, ConfigFileName);

        public static ConfigModel Config { get; private set; } = new ConfigModel();

        public static void LoadConfig()
        {
            try
            {
                bool needSave = false;

                if (File.Exists(_streamingPath))
                {
                    string json = File.ReadAllText(_streamingPath, Encoding.UTF8);
                    var config = JsonConvert.DeserializeObject<ConfigModel>(json);
                    if (config != null)
                    {
                        Config = config;

                        if (Config.localization == null || Config.localization.Count == 0)
                        {
                            Config.localization = LocalizationProvider.GetDefaultLocalizations();
                            needSave = true;
                        }
                    }
                    else
                    {
                        Config = new ConfigModel();
                        Config.localization = LocalizationProvider.GetDefaultLocalizations();
                        needSave = true;
                    }
                }
                else
                {
                    Config = new ConfigModel();
                    Config.localization = LocalizationProvider.GetDefaultLocalizations();
                    needSave = true;
                }

                if (needSave)
                {
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[StateNotifyByPop]: Read config failed: " + ex);
                if (Config.localization == null || Config.localization.Count == 0)
                {
                    Config.localization = LocalizationProvider.GetDefaultLocalizations();
                }
            }
        }

        public static void SaveConfig()
        {
            try
            {
                if (Config.localization == null || Config.localization.Count == 0)
                {
                    Config.localization = LocalizationProvider.GetDefaultLocalizations();
                }

                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(_streamingPath, json, Encoding.UTF8);
                Debug.Log("[StateNotifyByPop]: Config file saved at: " + _streamingPath);
            }
            catch (Exception ex)
            {
                Debug.LogError("[StateNotifyByPop]: Config file save failed: " + ex);
            }
        }
    }
}