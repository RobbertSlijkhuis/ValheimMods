using System.Collections.Generic;
using UnityEngine;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Helpers
{
    internal class StatusEffectHelper
    {
        /// <summary>
        /// Create a simple status effect instance
        /// </summary>
        /// <param name="name"></param>
        /// <param name="duration"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static StatusEffect CreateSimple(string name, float duration, Sprite icon = null)
        {
            StatusEffect statusEffect = ScriptableObject.CreateInstance<StatusEffect>();
            statusEffect.name = name;
            statusEffect.m_name = name;
            statusEffect.m_icon = icon;
            statusEffect.m_ttl = duration;

            return statusEffect;
        }

        /// <summary>
        /// Get a random status effect from a list
        /// </summary>
        /// <returns></returns>
        public static int GetRandomStatusEffect()
        {
            List<string> hashList = new List<string>();
            hashList.Add(StatusEffectType.BarlyWine);
            hashList.Add(WizshBoneTwitchIntegration.Instance.effects.Burning.name);
            hashList.Add(StatusEffectType.FrostResist); // FrostResist
            hashList.Add(WizshBoneTwitchIntegration.Instance.effects.Freezing.name);
            hashList.Add(StatusEffectType.PoisonResist); // PoisonResist
            hashList.Add(WizshBoneTwitchIntegration.Instance.effects.Poison.name);
            hashList.Add(StatusEffectType.Ratatosk);
            hashList.Add(StatusEffectType.Puke);
            hashList.Add(StatusEffectType.Bzerker);
            hashList.Add(StatusEffectType.Wet);
            hashList.Add(StatusEffectType.LightFoot);
            hashList.Add(StatusEffectType.Tarred);
            hashList.Add(StatusEffectType.Rested);
            hashList.Add(StatusEffectType.Tarred);

            int index = Random.Range(0, hashList.Count);
            return hashList[index].GetStableHashCode();
        }
    }
}
