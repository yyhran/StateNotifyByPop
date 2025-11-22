using HarmonyLib;
using UnityEngine;
using SodaCraft.Localizations;

namespace StateNotifyByPop
{

    [HarmonyPatch(typeof(Grenade), "OnCollisionEnter")]
    public static class OnCollisionEnter_Patch
    {
        [HarmonyPostfix]
        static void OnCollisionEnterPostfix(Grenade __instance, Collision collision)
        {
            // 检查是否是首次碰撞(落地)  
            // if (!__instance.collide) return;

            // 获取主角  
            CharacterMainControl mainCharacter = CharacterMainControl.Main;
            if (mainCharacter == null) return;

            // 获取手雷当前位置和爆炸半径  
            Vector3 grenadePosition = __instance.transform.position;
            float explosionRadius = __instance.damageRange;

            // 计算主角到手雷的距离  
            float distance = Vector3.Distance(grenadePosition, mainCharacter.transform.position);

            // 检查主角是否在爆炸范围内  
            if (distance <= explosionRadius)
            {
                mainCharacter.PopText(LocalizationProvider.GetLocalized("SNBP_Grenade_Warning"), -1f);
            }
        }
    }
}
