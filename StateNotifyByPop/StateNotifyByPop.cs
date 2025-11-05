using Duckov.Modding;
using Duckov.UI;
using Duckov.Utilities;
using ItemStatsSystem;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UI;

namespace StateNotifyByPop
{

    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        void Awake()
        {
            LoadConfig();
            Debug.Log("[StateNotifyByPop]: Loaded!!!");
        }
        void OnDestroy()
        {
        }
        void OnEnable()
        {
            LoadConfig();
            Debug.Log("[StateNotifyByPop]: Enabled!!!");
        }
        void OnDisable()
        {
            Debug.Log("[StateNotifyByPop]: Disable!!!");
        }

        private void Update()
        {
            float num = Time.time - ModBehaviour.last_time;
            if (num < Config.gap_time)
            {
                return;
            }

            ModBehaviour.last_time = Time.time;
            CharacterMainControl main = CharacterMainControl.Main;
            if (main == null || main.Health == null || main.Health.IsDead)
            {
                return;
            }

            float maxWater = Mathf.Max(1f, main.MaxWater);
            float maxEnergy = Mathf.Max(1f, main.MaxEnergy);
            float currWater = Mathf.Clamp(main.CurrentWater, 0f, maxWater);
            float currEnergy = Mathf.Clamp(main.CurrentEnergy, 0f, maxEnergy);

            float waterPercent = currWater / maxWater;
            float energyPercent = currEnergy / maxEnergy;

            bool water_now = waterPercent <= Config.water_limit;
            bool energy_now = energyPercent <= Config.energy_limit;

            if (water_now && energy_now)
            {
                if (!ModBehaviour.water_last_state || !ModBehaviour.energy_last_state)
                {
                    main.PopText("我现在又渴又饿", -1f);
                }
            }
            else if (water_now && !ModBehaviour.water_last_state)
            {
                main.PopText("感觉有点渴了", -1f);
            }
            else if (energy_now && !ModBehaviour.energy_last_state)
            {
                main.PopText("感觉有点饿了", -1f);
            }

            ModBehaviour.water_last_state = water_now;
            ModBehaviour.energy_last_state = energy_now;
        }


        void LoadConfig()
        {
            try
            {
                string configPath = Path.Combine(Application.streamingAssetsPath, ModBehaviour.config_file_name);
                Debug.Log("[StateNotifyByPop]: Load Config file at: " + configPath);
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath, Encoding.UTF8);
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

        void SaveConfig()
        {
            try
            {
                string configPath = Path.Combine(Application.streamingAssetsPath, ModBehaviour.config_file_name);
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(configPath, json, Encoding.UTF8);
                Debug.Log("[StateNotifyByPop]: Generate default Config file at: " + configPath);
            }
            catch (Exception ex)
            {
                Debug.LogError("[StateNotifyByPop]: Config file save failed: " + ex);
            }
        }

        public class ConfigModel
        {
            public float water_limit = 0.3f;
            public float energy_limit = 0.3f;
            public float gap_time = 1f;
        }

        public static ConfigModel Config = new ConfigModel();

        public static float last_time = 0f;
        private static bool water_last_state = false;
        private static bool energy_last_state = false;
        private static string config_file_name = "StateNotifyByPop_Config.json";
    }
}