using System.Collections.Generic;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Helpers
{
    // Stops projectiles fired by Twitch-spawned monsters at the edge of a safe zone, the same way
    // the vanilla Ashlands ShieldGenerator does (ShieldGenerator.CheckProjectile, polled from
    // Projectile.FixedUpdate). Projectiles have no Rigidbody, so TwitchSafeZone's trigger collider
    // never sees them and a position poll is the only way to catch them.
    //
    // Multiplayer: Projectile.FixedUpdate only runs its logic on the projectile's owner, so this is
    // too - the owning client destroys it and ZNetScene.Destroy replicates the removal. No ZDO data
    // or RPC needed; the zone and creature checks read state every client already has.
    internal static class SafeZoneProjectileHelper
    {
        private static readonly AccessTools.FieldRef<Projectile, Character> s_ownerRef =
            AccessTools.FieldRefAccess<Projectile, Character>("m_owner");

        private static EffectList s_hitEffects;
        private static bool s_hitEffectsResolved;

        public static void TryBlock(Projectile projectile)
        {
            if (!TwitchSafeZone.HasActiveSafeZones || !TwitchSafeZone.KeepsMonstersOut)
                return;

            ZNetView nview = projectile.GetComponent<ZNetView>();

            if (nview == null || !nview.IsValid() || !nview.IsOwner())
                return;

            Character owner = s_ownerRef(projectile);

            if (owner == null || !TwitchSafeZone.IsWardTargetCreature(owner.gameObject))
                return;

            Vector3 position = projectile.transform.position;

            // Only where the projectile is matters, not where it was fired from: one fired by a
            // monster that clipped into the zone is destroyed on its first step too.
            if (!TwitchSafeZone.IsInsideEnabledSafeZone(position))
                return;

            // Deliberately not Projectile.OnHit (which vanilla's shield calls): that would run
            // SpawnOnHit/AOE and detonate it at the zone edge, hurting players inside.
            PlayHitEffect(position, projectile.m_startPoint);
            ZNetScene.instance.Destroy(projectile.gameObject);
        }

        private const string DomeHitPrefabName = "WBTI_dome_hit";

        // WBTI_dome_hit is built from vanilla's fx_shieldgenerator_domehit, so it carries the vanilla
        // ShieldDomeParticleColor too. Its Start recolors the particles by the closest
        // ShieldGenerator's fuel and throws when the world has none (the usual case), so skip it:
        //  - for our own prefab always: it is ward-colored on purpose, and a real generator elsewhere
        //    must not recolor it;
        //  - for anything else only when there is no generator to read (the vanilla-hit fallback in
        //    BuildHitEffects, which would otherwise throw the same way).
        public static bool ShouldSkipParticleColor(ShieldDomeParticleColor particleColor)
        {
            if (particleColor.gameObject.name.StartsWith(DomeHitPrefabName))
                return true;

            return ShieldGenerator.GetClosestShieldGenerator(
                particleColor.transform.position,
                particleColor.m_colorMode == ShieldDomeParticleColor.ColorMode.ClosestShieldGenerator) == null;
        }

        // The vanilla shield's hit effects with fx_shieldgenerator_domehit swapped for our own
        // ward-colored WBTI_dome_hit. Any other vanilla entries (e.g. the hit sound) are kept.
        // Falls back to the untouched vanilla list if our prefab failed to load.
        private static EffectList BuildHitEffects()
        {
            GameObject shieldGen = PrefabManager.Instance.GetPrefab("piece_shieldgenerator");
            EffectList vanilla = shieldGen != null ? shieldGen.GetComponent<ShieldGenerator>()?.m_shieldHitEffects : null;
            GameObject domeHit = WizshBoneTwitchIntegration.Instance.prefabs.DomeHit;

            if (domeHit == null)
            {
                Jotunn.Logger.LogWarning("[WBTI] WBTI_dome_hit prefab not found, safe zone projectile blocks use the vanilla hit effect");
                return vanilla;
            }

            List<EffectList.EffectData> effects = new List<EffectList.EffectData>();

            if (vanilla?.m_effectPrefabs != null)
            {
                foreach (EffectList.EffectData data in vanilla.m_effectPrefabs)
                {
                    if (data.m_prefab != null && data.m_prefab.name == "fx_shieldgenerator_domehit")
                        continue;

                    effects.Add(data);
                }
            }

            effects.Add(new EffectList.EffectData { m_enabled = true, m_prefab = domeHit, m_variant = -1 });

            return new EffectList { m_effectPrefabs = effects.ToArray() };
        }

        private static void PlayHitEffect(Vector3 position, Vector3 from)
        {
            if (!s_hitEffectsResolved)
            {
                s_hitEffectsResolved = true;
                s_hitEffects = BuildHitEffects();
            }

            if (s_hitEffects == null)
                return;

            Vector3 outward = from - position;
            Quaternion rotation = outward.sqrMagnitude > 0.0001f ? Quaternion.LookRotation(outward.normalized) : Quaternion.identity;

            s_hitEffects.Create(position, rotation);
        }
    }
}
