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
            if (num < ModBehaviour.gap_time)
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

            if (Config.enable_three_stage)
            {
                int water_stage = GetStageThreeAuto(waterPercent, Config.water_limit);
                int energy_stage = GetStageThreeAuto(energyPercent, Config.energy_limit);
                // Debug.Log($"[StateNotifyByPop]: Water Stage: {water_stage}/{waterPercent}, Energy Stage: {energy_stage}/{energyPercent}");

                // 若水和能量同时处于任一非 0 阶段，显示合并提示
                if (water_stage > 0 && energy_stage > 0)
                {
                    if (water_stage != ModBehaviour.water_last_stage || energy_stage != ModBehaviour.energy_last_stage)
                    {
                        if (water_stage >= energy_stage)
                        {
                            string msg = GetStageMessage(water_stage, true);
                            string msg2 = GetStageMessage(energy_stage, false);
                            main.PopText(msg + ", 还" + msg2);
                        }
                        else
                        {
                            string msg = GetStageMessage(energy_stage, false);
                            string msg2 = GetStageMessage(water_stage, true);
                            main.PopText(msg + ", 还" + msg2);
                        }
                    }
                }
                else if (water_stage > 0)
                {
                    if (water_stage != ModBehaviour.water_last_stage)
                    {
                        string msg = GetStageMessage(water_stage, true);
                        main.PopText(msg, -1f);
                    }
                }
                else if (energy_stage > 0)
                {
                    if (energy_stage != ModBehaviour.energy_last_stage)
                    {
                        string msg = GetStageMessage(energy_stage, false);
                        main.PopText(msg, -1f);
                    }
                }

                ModBehaviour.water_last_stage = water_stage;
                ModBehaviour.energy_last_stage = energy_stage;
            }
            else
            {
                // 旧逻辑
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
        }

        // 三阶段自动阈值计算并判定阶段：
        // stage1: percent <= limit
        // stage2: percent <= limit/2
        // stage3: percent <= 0
        // 返回：0=正常,1=轻度,2=中度,3=重度
        private int GetStageThreeAuto(float percent, float limit)
        {
            const float zeroThreshold = 1e-6f;

            if (limit < 0f)
            {
                return 0;
            }

            // stage3 优先判断：当 percent 非常接近 0 或等于 0 时触发
            if (percent <= zeroThreshold)
            {
                return 3;
            }

            // 处理 limit 为 0 的情况：当 limit == 0 且 percent > zeroThreshold，则不触发 stage1/2
            if (Mathf.Approximately(limit, 0f))
            {
                return 0;
            }

            // stage2 = limit / 2，但不能小于 0（理论上 limit >= 0）
            float stage2 = Mathf.Max(0f, limit / 2f);

            if (percent <= stage2)
            {
                return 2;
            }
            if (percent <= limit)
            {
                return 1;
            }
            return 0;
        }

        private string GetStageMessage(int stage, bool isWater)
        {
            if (isWater)
            {
                switch (stage)
                {
                    case 1: return "感觉有点渴";
                    case 2: return "感觉非常口渴";
                    case 3: return "快要渴死了";
                    default: return "";
                }
            }
            else
            {
                switch (stage)
                {
                    case 1: return "感觉有点饿";
                    case 2: return "感觉非常饿";
                    case 3: return "快要饿死了";
                    default: return "";
                }
            }
        }

        void LoadConfig()
        {
            try
            {
                string configPath = Path.Combine(Application.streamingAssetsPath, ModBehaviour.config_file_name);
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
            // public float gap_time = 0.5f;
            public bool enable_three_stage = true;
        }

        public static ConfigModel Config = new ConfigModel();

        private static float last_time = 0f;
        private static float gap_time = 0.5f;
        private static bool water_last_state = false;
        private static bool energy_last_state = false;
        private static int water_last_stage = 0;
        private static int energy_last_stage = 0;
        private static string config_file_name = "StateNotifyByPop.cfg";
    }
}