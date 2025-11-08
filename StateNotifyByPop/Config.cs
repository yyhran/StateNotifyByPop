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
            public bool enable_three_stage = true;
        }


        private const string ConfigFileName = "StateNotifyByPop.cfg";
        private static string StreamingPath => Path.Combine(Application.streamingAssetsPath, ConfigFileName);

        public static ConfigModel Config { get; private set; } = new ConfigModel();


        public static void LoadConfig()
        {
            try
            {
                if (File.Exists(StreamingPath))
                {
                    string json = File.ReadAllText(StreamingPath, Encoding.UTF8);
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
                File.WriteAllText(StreamingPath, json, Encoding.UTF8);
                Debug.Log("[StateNotifyByPop]: Generate default Config file at: " + StreamingPath);
            }
            catch (Exception ex)
            {
                Debug.LogError("[StateNotifyByPop]: Config file save failed: " + ex);
            }
        }
    }
}