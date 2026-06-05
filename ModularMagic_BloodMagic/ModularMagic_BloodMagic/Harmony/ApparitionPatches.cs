using HarmonyLib;
using ModularMagic_BloodMagic.Components;
using ModularMagic_BloodMagic.Models;
using UnityEngine;

namespace ModularMagic_BloodMagic.Harmony
{
    [HarmonyPatch]
    internal class ApparitionPatches
    {
        /// <summary>
        /// Scales outgoing damage for apparitions based on the scythe that spawned them.
        /// Prefix runs before damage is calculated — HitData is a reference type so
        /// mutations here affect the live hit. m_shared is never touched.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "Damage")]
            public static void CharacterDamage_Prefix(Character __instance, HitData hit)
        {
            ApparitionController? controller = hit.GetAttacker()?.GetComponent<ApparitionController>();
            if (controller == null)
                return;

            ApparitionOptions? options = controller.GetOptions();
            if (options == null || Mathf.Approximately(options.damageMultiplier, 1f))
                return;

            HitData.DamageTypes original = hit.m_damage;

            hit.m_damage.m_blunt     *= options.damageMultiplier;
            hit.m_damage.m_slash     *= options.damageMultiplier;
            hit.m_damage.m_pierce    *= options.damageMultiplier;
            hit.m_damage.m_fire      *= options.damageMultiplier;
            hit.m_damage.m_frost     *= options.damageMultiplier;
            hit.m_damage.m_lightning *= options.damageMultiplier;
            hit.m_damage.m_poison    *= options.damageMultiplier;
            hit.m_damage.m_spirit    *= options.damageMultiplier;

            float currentHealth = __instance.GetHealth();
            float totalOriginal = original.m_blunt + original.m_slash + original.m_pierce
                                + original.m_fire  + original.m_frost + original.m_lightning
                                + original.m_poison + original.m_spirit;
            float totalScaled   = hit.m_damage.m_blunt + hit.m_damage.m_slash + hit.m_damage.m_pierce
                                + hit.m_damage.m_fire  + hit.m_damage.m_frost + hit.m_damage.m_lightning
                                + hit.m_damage.m_poison + hit.m_damage.m_spirit;

            Character? attacker     = hit.GetAttacker();
            float      attackerHealth = attacker?.GetHealth() ?? 0f;

            Jotunn.Logger.LogWarning(
                $"[Apparition] Attacker: '{attacker?.name}' | " +
                $"Health: {attackerHealth:F1}/{options.health:F1} | " +
                $"Damage: {totalOriginal:F2} → {totalScaled:F2} (×{options.damageMultiplier})");
        }
    }
}