using UnityEngine;

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
    }
}
