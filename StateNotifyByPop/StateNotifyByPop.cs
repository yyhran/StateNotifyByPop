using System;
using System.Collections.Generic;
using UnityEngine;
using SodaCraft.Localizations;

namespace StateNotifyByPop
{

    public class ModBehaviour : Duckov.Modding.ModBehaviour
    { 
        void Awake()
        {
            ConfigManager.LoadConfig();
            LocalizationProvider.InitTranslations();

            Debug.Log("[StateNotifyByPop]: Loaded!!!");
        }
        void OnDestroy()
        {
        }
        void OnEnable()
        {
            ConfigManager.LoadConfig();
            LocalizationManager.OnSetLanguage += LocalizationProvider.OnLanguageChanged;
            Debug.Log("[StateNotifyByPop]: Enabled!!! ");
        }

        void OnDisable()
        {
            LocalizationManager.OnSetLanguage -= LocalizationProvider.OnLanguageChanged;
            ConfigManager.SaveConfig();
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
            float maxHealth = Mathf.Max(1f, main.Health.MaxHealth);
            float currWater = Mathf.Clamp(main.CurrentWater, 0f, maxWater);
            float currEnergy = Mathf.Clamp(main.CurrentEnergy, 0f, maxEnergy);
            float currHealth = Mathf.Clamp(main.Health.CurrentHealth, 0f, maxHealth);

            float waterPercent = currWater / maxWater;
            float energyPercent = currEnergy / maxEnergy;
            float healthPercent = currHealth / maxHealth;

            // Debug.Log($"[StateNotifyByPop]: Water: {currWater}/{maxWater} ({waterPercent * 100f}%), Energy: {currEnergy}/{maxEnergy} ({energyPercent * 100f}%), Health: {currHealth}/{maxHealth} ({healthPercent * 100f}%)");

            var cfg = ConfigManager.Config;
            // 水分和能量提示
            if (cfg.enable_three_stage)
            {
                int water_stage = GetStageThreeAuto(waterPercent, cfg.water_limit);
                int energy_stage = GetStageThreeAuto(energyPercent, cfg.energy_limit);

                // 若水和能量同时处于任一非 0 阶段，显示合并提示
                if (water_stage > 0 && energy_stage > 0)
                {
                    if (water_stage != ModBehaviour.water_last_stage || energy_stage != ModBehaviour.energy_last_stage)
                    {
                        if (water_stage >= energy_stage)
                        {
                            string msg = LocalizationProvider.GetLocalized("SNBP_Water_Stage" + water_stage);
                            string extra = LocalizationProvider.GetLocalized("SNBP_Energy_Stage" + energy_stage);
                            string connector = LocalizationProvider.GetLocalized("SNBP_BothConnector");
                            main.PopText($"{msg}{connector}{extra}", -1f);
                        }
                        else
                        {
                            string msg = LocalizationProvider.GetLocalized("SNBP_Energy_Stage" + energy_stage);
                            string extra = LocalizationProvider.GetLocalized("SNBP_Water_Stage" + water_stage);
                            string connector = LocalizationProvider.GetLocalized("SNBP_BothConnector");
                            main.PopText($"{msg}{connector}{extra}", -1f);
                        }
                    }
                }
                else if (water_stage > 0)
                {
                    if (water_stage != water_last_stage)
                    {
                        main.PopText(LocalizationProvider.GetLocalized("SNBP_Water_Stage" + water_stage), -1f);
                    }
                }
                else if (energy_stage > 0)
                {
                    if (energy_stage != energy_last_stage)
                    {
                        main.PopText(LocalizationProvider.GetLocalized("SNBP_Energy_Stage" + energy_stage), -1f);
                    }
                }

                ModBehaviour.water_last_stage = water_stage;
                ModBehaviour.energy_last_stage = energy_stage;
            }
            else
            {
                bool water_now = waterPercent <= cfg.water_limit;
                bool energy_now = energyPercent <= cfg.energy_limit;

                if (water_now && energy_now)
                {
                    if (!water_last_state || !energy_last_state)
                    {
                        main.PopText(LocalizationProvider.GetLocalized("SNBP_BothSimple"), -1f);
                    }
                }
                else if (water_now && !water_last_state)
                {
                    main.PopText(LocalizationProvider.GetLocalized("SNBP_Water_Stage1"), -1f);
                }
                else if (energy_now && !energy_last_state)
                {
                    main.PopText(LocalizationProvider.GetLocalized("SNBP_Energy_Stage1"), -1f);
                }

                ModBehaviour.water_last_state = water_now;
                ModBehaviour.energy_last_state = energy_now;
            }

            // 血量提示
            bool health_now = healthPercent < cfg.health_limit;
            if (health_now)
            {
                if (!health_last_state)
                {
                    main.PopText(LocalizationProvider.GetLocalized("SNBP_Health_Low"), -1f);
                }
            }
            ModBehaviour.health_last_state = health_now;

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

        private static float last_time = 0f;
        private static float gap_time = 0.5f;
        private static bool water_last_state = false;
        private static bool energy_last_state = false;
        private static bool health_last_state = false;
        private static int water_last_stage = 0;
        private static int energy_last_stage = 0;
    }
}