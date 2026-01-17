using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WizshBoneTwitchIntegration.Models;
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
        /// Get a list of all available status effects
        /// </summary>
        /// <returns></returns>
        public static List<string> GetAvailableStatusEffects()
        {
            return typeof(StatusEffectType).GetProperties().Select(x => x.GetValue(null).ToString()).ToList();
        }

        /// <summary>
        /// Get the status effect id via mead name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetByMeadID(string name)
        {
            switch (name)
            {
                case "MeadBugRepellent": return StatusEffectType.AntiSting;
                case "BarleyWine": return StatusEffectType.BarlyWine;
                case "MeadTamer": return StatusEffectType.BrewOfAnimalWispers;
                case "MeadBzerker": return StatusEffectType.Bzerker;
                case "MeadFrostResist": return StatusEffectType.FrostResist;
                case "MeadLightfoot": return StatusEffectType.LightFoot;
                case "MeadTrollPheromones": return StatusEffectType.LovePotion;
                case "MeadPoisonResist": return StatusEffectType.PoisonResist;
                case "MeadHasty": return StatusEffectType.Ratatosk;
                case "MeadStrength": return StatusEffectType.TrollStrength;
                case "MeadSwimmer": return StatusEffectType.Vananidir;

                case "MeadEitrMinor": return StatusEffectType.EitrMinor;
                case "MeadEitrLingering": return StatusEffectType.EitrLingering;
                case "MeadHealthMinor": return StatusEffectType.HealingMinor;
                case "MeadHealthMediumr": return StatusEffectType.HealingMedium;
                case "MeadHealthMajor": return StatusEffectType.HealingMajor;
                case "MeadHealthLingering": return StatusEffectType.HealingLingering;
                case "MeadStaminaMinor": return StatusEffectType.StaminaMinor;
                case "MeadStaminaMedium": return StatusEffectType.StaminaMedium;
                case "MeadStaminaLingering": return StatusEffectType.StaminaLingering;

                default: return "";
            }
        }

        /// <summary>
        /// Get a random status effect from a list
        /// </summary>
        /// <returns></returns>
        public static StatusEffectData GetRandomStatusEffect(List<StatusEffectData> statusEffects)
        {
            int index = Random.Range(0, statusEffects.Count);
            return statusEffects[index];
        }
    }
}
