using UnityEngine;

namespace WizshBoneTwitchIntegration.Helpers
{
    /// <summary>
    /// A short-lived frost resistance that a weather zone keeps topping up on the local player while
    /// they stand inside it. Frost resistance is what makes vanilla's Player.UpdateEnvStatusEffects skip
    /// (and remove) the Cold/Freezing effects, so it protects against weather damage the same way a
    /// frost resistance mead does. The effect is its own (never the mead's), so it can't shorten or
    /// replace a real mead the player drank.
    /// </summary>
    internal static class FrostWardHelper
    {
        private const string EffectName = "WBTI_FrostWard";

        // The zone refreshes this every fraction of a second; it only has to outlast the gap between
        // refreshes, so the buff disappears moments after leaving the zone or the zone ending.
        private const float Lifetime = 3f;

        private static SE_Stats s_effect;

        private static SE_Stats GetEffect()
        {
            // A ScriptableObject nothing in Unity references can be unloaded, hence the HideAndDontSave
            // and the recreate-if-destroyed check.
            if (s_effect != null)
                return s_effect;

            SE_Stats effect = ScriptableObject.CreateInstance<SE_Stats>();
            effect.name = EffectName;
            effect.m_name = "Frost ward";
            effect.m_ttl = Lifetime;
            effect.hideFlags = HideFlags.HideAndDontSave;
            effect.m_mods.Add(new HitData.DamageModPair { m_type = HitData.DamageType.Frost, m_modifier = HitData.DamageModifier.Resistant });

            StatusEffect mead = ObjectDB.instance != null ? ObjectDB.instance.GetStatusEffect("Potion_frostresist".GetStableHashCode()) : null;
            if (mead != null)
                effect.m_icon = mead.m_icon;

            s_effect = effect;
            return s_effect;
        }

        public static void Apply(Player player)
        {
            if (player == null)
                return;

            SE_Stats effect = GetEffect();
            player.GetSEMan().AddStatusEffect(effect, resetTime: true);
        }

        public static void Remove(Player player)
        {
            if (player == null || s_effect == null)
                return;

            player.GetSEMan().RemoveStatusEffect(s_effect.NameHash(), quiet: true);
        }
    }
}
