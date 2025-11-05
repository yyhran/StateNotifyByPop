using System;
using Duckov.UI;
using Duckov.Utilities;
using Duckov.Modding;
using ItemStatsSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

namespace StateNotifyByPop
{

    public class ModBehaviour : Duckov.Modding.ModBehaviour
    {
        void Awake()
        {
            Debug.Log("[StateNotifyByPop]: Loaded!!!");
        }
        void OnDestroy()
        {
        }
        void OnEnable()
        {
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

            if (waterPercent <= ModBehaviour.water_limit && energyPercent <= ModBehaviour.engery_limit)
            {
                if (!ModBehaviour.water_last_state && !ModBehaviour.engery_last_state)
                {
                    main.PopText("我现在又渴又饿", -1f);
                    ModBehaviour.water_last_state = true;
                    ModBehaviour.engery_last_state = true;
                }
            }
            else if (waterPercent <= ModBehaviour.water_limit)
            {
                if (!ModBehaviour.water_last_state)
                {
                    main.PopText("我现在好渴啊", -1f);
                    ModBehaviour.water_last_state = true;
                    ModBehaviour.engery_last_state = false;
                }
            }
            else if (energyPercent <= ModBehaviour.engery_limit)
            {
                if (!ModBehaviour.engery_last_state)
                {
                    main.PopText("我现在好饿啊", -1f);
                    ModBehaviour.engery_last_state = true;
                    ModBehaviour.water_last_state = false;
                }
            }
            else
            {
                ModBehaviour.water_last_state = false;
                ModBehaviour.engery_last_state = false;
            }
        }

        public static float last_time = 0f;
        private static bool water_last_state = false;
        private static bool engery_last_state = false;
        public static float gap_time = 1f;
        public static float water_limit = 0.3f;
        public static float engery_limit = 0.3f;
    }
}