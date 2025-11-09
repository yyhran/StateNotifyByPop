using System;
using System.Collections.Generic;
using UnityEngine;
using SodaCraft.Localizations;


namespace StateNotifyByPop
{
    public static class LocalizationProvider
    {
        public static SystemLanguage CurrentLanguage { get; private set; } = LocalizationManager.CurrentLanguage;

        // language -> key -> text
        private static readonly Dictionary<SystemLanguage, Dictionary<string, string>> translations
            = new Dictionary<SystemLanguage, Dictionary<string, string>>();

        public static void InitTranslations()
        {
            // Chinese Simplified
            var zh = new Dictionary<string, string>
            {
                { "SNBP_Water_Stage1", "感觉有点渴" },
                { "SNBP_Water_Stage2", "感觉非常口渴" },
                { "SNBP_Water_Stage3", "快要渴死了" },
                { "SNBP_Energy_Stage1", "感觉有点饿" },
                { "SNBP_Energy_Stage2", "感觉非常饿" },
                { "SNBP_Energy_Stage3", "快要饿死了" },
                { "SNBP_BothConnector", "，还" },
                { "SNBP_BothSimple", "我现在又渴又饿" }
            };
            translations[SystemLanguage.ChineseSimplified] = zh;

            // Chinese Traditional
            var zh_tw = new Dictionary<string, string>
            {
                { "SNBP_Water_Stage1", "感覺有點渴" },
                { "SNBP_Water_Stage2", "感覺非常口渴" },
                { "SNBP_Water_Stage3", "快要渴死了" },
                { "SNBP_Energy_Stage1", "感覺有點餓" },
                { "SNBP_Energy_Stage2", "感覺非常餓" },
                { "SNBP_Energy_Stage3", "快要餓死了" },
                { "SNBP_BothConnector", "，還" },
                { "SNBP_BothSimple", "我現在又渴又餓" }
            };
            translations[SystemLanguage.ChineseTraditional] = zh_tw;

            // English
            var en = new Dictionary<string, string>
            {
                { "SNBP_Water_Stage1", "A bit thirsty" },
                { "SNBP_Water_Stage2", "Very thirsty" },
                { "SNBP_Water_Stage3", "About to die of thirst" },
                { "SNBP_Energy_Stage1", "A bit hungry" },
                { "SNBP_Energy_Stage2", "Very hungry" },
                { "SNBP_Energy_Stage3", "About to starve" },
                { "SNBP_BothConnector", ", also " },
                { "SNBP_BothSimple", "I'm thirsty and hungry" }
            };
            translations[SystemLanguage.English] = en;

            // Japanese
            var ja = new Dictionary<string, string>
            {
                { "SNBP_Water_Stage1", "ちょっと喉が渇いた" },
                { "SNBP_Water_Stage2", "とても喉が渇いた" },
                { "SNBP_Water_Stage3", "もうすぐ干からびそうだ" },
                { "SNBP_Energy_Stage1", "ちょっとお腹が空いた" },
                { "SNBP_Energy_Stage2", "とてもお腹が空いた" },
                { "SNBP_Energy_Stage3", "もうすぐ餓死しそうだ" },
                { "SNBP_BothConnector", "、そして" },
                { "SNBP_BothSimple", "喉が渇いてお腹が空いている" }
            };
            translations[SystemLanguage.Japanese] = ja;

            // Korean
            var ko = new Dictionary<string, string>
            {
                { "SNBP_Water_Stage1", "약간 목이 마르다" },
                { "SNBP_Water_Stage2", "매우 목이 마르다" },
                { "SNBP_Water_Stage3", "목말라 죽을 것 같다" },
                { "SNBP_Energy_Stage1", "약간 배고프다" },
                { "SNBP_Energy_Stage2", "매우 배고프다" },
                { "SNBP_Energy_Stage3", "거의 굶어 죽을 것 같다" },
                { "SNBP_BothConnector", " 그리고 " },
                { "SNBP_BothSimple", "목이 마르고 배고프다" }
            };
            translations[SystemLanguage.Korean] = ko;

            CurrentLanguage = LocalizationManager.CurrentLanguage;
            ApplyLocalizationToManager(CurrentLanguage);
            Debug.Log($"[StateNotifyByPop.Localization]: Current language: {CurrentLanguage}");
        }


        public static void OnLanguageChanged(SystemLanguage lang)
        {
            CurrentLanguage = lang;
            ApplyLocalizationToManager(lang);
            Debug.Log($"[StateNotifyByPop.Localization]: Language changed to: {lang}");
        }


        public static string GetLocalized(string key)
        {
            if (translations.TryGetValue(CurrentLanguage, out var map) && map.TryGetValue(key, out var s))
            {
                return s;
            }

            // fallback
            if (translations.TryGetValue(SystemLanguage.ChineseSimplified, out var zh) && zh.TryGetValue(key, out var s2))
            {
                return s2;
            }

            if (translations.TryGetValue(SystemLanguage.English, out var en) && en.TryGetValue(key, out var s3))
            {
                return s3;
            }
            return key;
        }


        public static void ApplyLocalizationToManager(SystemLanguage language)
        {
            if (!translations.TryGetValue(language, out var map)) return;
            try
            {
                foreach (var kv in map)
                {
                    LocalizationManager.SetOverrideText(kv.Key, kv.Value);
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[StateNotifyByPop.Localization]: ApplyLocalizationToManager failed: " + ex.Message);
            }
        }
    }
}