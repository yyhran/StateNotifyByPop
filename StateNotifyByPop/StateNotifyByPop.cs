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

            CheckHealth(main);
            CheckWaterAndEnergy(main);
        }

        private void CheckHealth(CharacterMainControl main)
        {
            float maxHealth = Mathf.Max(1f, main.Health.MaxHealth);
            float currHealth = Mathf.Clamp(main.Health.CurrentHealth, 0f, maxHealth);
            float healthPercent = currHealth / maxHealth;

            var cfg = ConfigManager.Config;
            int healthStage = GetStageByPercent(healthPercent, cfg.health_limit);

            if (healthStage > 0 && _healthLastStage == 0)
            {
                main.PopText(LocalizationProvider.GetLocalized("SNBP_Health_Low"), -1f);
            }

            _healthLastStage = healthStage;
        }

        private void CheckWaterAndEnergy(CharacterMainControl main)
        {
            float maxWater = Mathf.Max(1f, main.MaxWater);
            float maxEnergy = Mathf.Max(1f, main.MaxEnergy);
            float currWater = Mathf.Clamp(main.CurrentWater, 0f, maxWater);
            float currEnergy = Mathf.Clamp(main.CurrentEnergy, 0f, maxEnergy);

            float waterPercent = currWater / maxWater;
            float energyPercent = currEnergy / maxEnergy;

            var cfg = ConfigManager.Config;
            int waterStage = GetStageByPercent(waterPercent, cfg.water_limit);
            int energyStage = GetStageByPercent(energyPercent, cfg.energy_limit);

            if (cfg.enable_three_stage)
            {
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
                else if (waterStage > 0 && waterStage != _waterLastStage)
                {
                    main.PopText(LocalizationProvider.GetLocalized("SNBP_Water_Stage" + waterStage), -1f);
                }
                else if (energyStage > 0 && energyStage != _energyLastStage)
                {
                    main.PopText(LocalizationProvider.GetLocalized("SNBP_Energy_Stage" + energyStage), -1f);
                }
            }
            else
            {
                if (waterStage > 0 && energyStage > 0)
                {
                    if (_waterLastStage == 0 || _energyLastStage == 0)
                    {
                        main.PopText(LocalizationProvider.GetLocalized("SNBP_BothSimple"), -1f);
                    }
                }
                else if (waterStage > 0 && _waterLastStage == 0)
                {
                    main.PopText(LocalizationProvider.GetLocalized("SNBP_Water_Stage1"), -1f);
                }
                else if (energyStage > 0 && _energyLastStage == 0)
                {
                    main.PopText(LocalizationProvider.GetLocalized("SNBP_Energy_Stage1"), -1f);
                }
            }

            _waterLastStage = waterStage;
            _energyLastStage = energyStage;
        }

        // 三阶段阈值计算并判定阶段
        // stage1: percent <= limit
        // stage2: percent <= limit/2
        // stage3: percent <= 0
        // 返回：0=正常,1=轻度,2=中度,3=重度
        private int GetStageByPercent(float percent, float limit)
        {
            // 当 limit == 0 不触发
            if (Mathf.Approximately(limit, 0f))
            {
                return 0;
            }

            // stage3: 当 percent 非常接近 0 或等于 0 时触发
            if (Mathf.Approximately(percent, 0f))
            {
                return 3;
            }

            // stage2 = limit / 2，但不能小于 0（理论上 limit > 0）
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
        private static int _waterLastStage = 0;
        private static int _energyLastStage = 0;
        private static int _healthLastStage = 0;
    }
}