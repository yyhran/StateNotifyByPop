using System;
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
        }


        private const string ConfigFileName = "StateNotifyByPop.cfg";
        private static string _streamingPath => Path.Combine(Application.streamingAssetsPath, ConfigFileName);

        public static ConfigModel Config { get; private set; } = new ConfigModel();


        public static void LoadConfig()
        {
            try
            {
                if (File.Exists(_streamingPath))
                {
                    string json = File.ReadAllText(_streamingPath, Encoding.UTF8);
                    var config = JsonConvert.DeserializeObject<ConfigModel>(json);
                    if (config != null)
                    {
                        Config = config;
                    }
                    else
                    {
                        SaveConfig();
                    }
                }
                else
                {
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[StateNotifyByPop]: Read config failed: " + ex);
            }
        }

        public static void SaveConfig()
        {
            try
            {
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(_streamingPath, json, Encoding.UTF8);
                Debug.Log("[StateNotifyByPop]: Generate default Config file at: " + _streamingPath);
            }
            catch (Exception ex)
            {
                Debug.LogError("[StateNotifyByPop]: Config file save failed: " + ex);
            }
        }
    }
}