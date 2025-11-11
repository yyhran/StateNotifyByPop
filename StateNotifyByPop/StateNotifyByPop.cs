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
            float num = Time.time - _lastTime;
            if (num < _gapTime)
            {
                return;
            }

            _lastTime = Time.time;
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
                int waterStage = GetStageThreeAuto(waterPercent, cfg.water_limit);
                int energyStage = GetStageThreeAuto(energyPercent, cfg.energy_limit);

                // 若水和能量同时处于任一非 0 阶段，显示合并提示
                if (waterStage > 0 && energyStage > 0)
                {
                    if (waterStage != _waterLastStage || energyStage != _energyLastStage)
                    {
                        if (waterStage >= energyStage)
                        {
                            string msg = LocalizationProvider.GetLocalized("SNBP_Water_Stage" + waterStage);
                            string extra = LocalizationProvider.GetLocalized("SNBP_Energy_Stage" + energyStage);
                            string connector = LocalizationProvider.GetLocalized("SNBP_BothConnector");
                            main.PopText($"{msg}{connector}{extra}", -1f);
                        }
                        else
                        {
                            string msg = LocalizationProvider.GetLocalized("SNBP_Energy_Stage" + energyStage);
                            string extra = LocalizationProvider.GetLocalized("SNBP_Water_Stage" + waterStage);
                            string connector = LocalizationProvider.GetLocalized("SNBP_BothConnector");
                            main.PopText($"{msg}{connector}{extra}", -1f);
                        }
                    }
                }
                else if (waterStage > 0)
                {
                    if (waterStage != _waterLastStage)
                    {
                        main.PopText(LocalizationProvider.GetLocalized("SNBP_Water_Stage" + waterStage), -1f);
                    }
                }
                else if (energyStage > 0)
                {
                    if (energyStage != _energyLastStage)
                    {
                        main.PopText(LocalizationProvider.GetLocalized("SNBP_Energy_Stage" + energyStage), -1f);
                    }
                }

                _waterLastStage = waterStage;
                _energyLastStage = energyStage;
            }
            else
            {
                bool waterNow = waterPercent <= cfg.water_limit;
                bool energyNow = energyPercent <= cfg.energy_limit;

                if (waterNow && energyNow)
                {
                    if (!_waterLastState || !_energyLastState)
                    {
                        main.PopText(LocalizationProvider.GetLocalized("SNBP_BothSimple"), -1f);
                    }
                }
                else if (waterNow && !_waterLastState)
                {
                    main.PopText(LocalizationProvider.GetLocalized("SNBP_Water_Stage1"), -1f);
                }
                else if (energyNow && !_energyLastState)
                {
                    main.PopText(LocalizationProvider.GetLocalized("SNBP_Energy_Stage1"), -1f);
                }

                _waterLastState = waterNow;
                _energyLastState = energyNow;
            }

            // 血量提示
            bool healthNow = healthPercent < cfg.health_limit;
            if (healthNow)
            {
                if (!_healthLastState)
                {
                    main.PopText(LocalizationProvider.GetLocalized("SNBP_Health_Low"), -1f);
                }
            }
            _healthLastState = healthNow;

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

        private static float _lastTime = 0f;
        private static float _gapTime = 0.5f;
        private static bool _waterLastState = false;
        private static bool _energyLastState = false;
        private static bool _healthLastState = false;
        private static int _waterLastStage = 0;
        private static int _energyLastStage = 0;
    }
}