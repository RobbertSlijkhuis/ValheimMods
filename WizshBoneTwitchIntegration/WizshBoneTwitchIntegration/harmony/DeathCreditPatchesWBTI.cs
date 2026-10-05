using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Helpers;

namespace WizshBoneTwitchIntegration.Harmony
{
    /// <summary>
    /// Patch methods only - all state and logic lives in <see cref="DeathCreditHelper"/>. Together
    /// they credit a death of the streamer to the viewer/redeem that caused it (see that class).
    /// Every patch swallows its own exceptions (logged) so a bug here can never break the damage
    /// path of the game.
    /// </summary>
    [HarmonyPatch]
    public class DeathCreditPatchesWBTI
    {
        // ── hazard damage calls: note whose hazard it is around each Damage call ──────────────
        // Prefix sets the note, finalizer clears it (a finalizer, so an exception in the hazard's own
        // code can't leave a stale note). The finalizers are void on purpose: they don't swallow anything.

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Aoe), "OnHit")]
        public static void AoeOnHit_Prefix(Aoe __instance) => BeginHazard(__instance);

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(Aoe), "OnHit")]
        public static void AoeOnHit_Finalizer() => DeathCreditHelper.EndHazard();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Projectile), "OnHit")]
        public static void ProjectileOnHit_Prefix(Projectile __instance) => BeginHazard(__instance);

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(Projectile), "OnHit")]
        public static void ProjectileOnHit_Finalizer() => DeathCreditHelper.EndHazard();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Projectile), "DoAOE")]
        public static void ProjectileDoAOE_Prefix(Projectile __instance) => BeginHazard(__instance);

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(Projectile), "DoAOE")]
        public static void ProjectileDoAOE_Finalizer() => DeathCreditHelper.EndHazard();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ImpactEffect), "OnCollisionEnter")]
        public static void ImpactEffectOnCollisionEnter_Prefix(ImpactEffect __instance) => BeginHazard(__instance);

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(ImpactEffect), "OnCollisionEnter")]
        public static void ImpactEffectOnCollisionEnter_Finalizer() => DeathCreditHelper.EndHazard();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Fire), "DoDamage")]
        public static void FireDoDamage_NotePrefix(Fire __instance) => BeginHazard(__instance);

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(Fire), "DoDamage")]
        public static void FireDoDamage_Finalizer() => DeathCreditHelper.EndHazard();

        private static void BeginHazard(UnityEngine.Component source)
        {
            try
            {
                DeathCreditHelper.BeginHazard(source);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DeathCredit BeginHazard: " + e);
            }
        }

        // ── damage reaching a player ──────────────────────────────────────────────────────────

        // Last, so it runs after CharacterDamage_Prefix (DamagePatchesWBTI) - which may still scale the hit.
        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(typeof(Character), "Damage")]
        public static void CharacterDamage_NotePrefix(Character __instance, HitData hit)
        {
            try
            {
                DeathCreditHelper.NoteHazardHit(__instance, hit);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DeathCredit Character.Damage prefix: " + e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "RPC_Damage")]
        public static void CharacterRPCDamage_Prefix(Character __instance, HitData hit)
        {
            try
            {
                DeathCreditHelper.MatchHazard(__instance, hit);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DeathCredit RPC_Damage prefix: " + e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "ApplyDamage")]
        public static void CharacterApplyDamage_Prefix(Character __instance, HitData hit)
        {
            try
            {
                DeathCreditHelper.OnApplyDamagePrefix(__instance, hit);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DeathCredit ApplyDamage prefix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "ApplyDamage")]
        public static void CharacterApplyDamage_Postfix(Character __instance, HitData hit)
        {
            try
            {
                DeathCreditHelper.OnApplyDamagePostfix(__instance, hit);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DeathCredit ApplyDamage postfix: " + e);
            }
        }

        // ── the death itself and swimming ─────────────────────────────────────────────────────

        // Runs only on the dying player's owning client (Player.OnDeath returns early for non-owners).
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnDeath")]
        public static void PlayerOnDeath_CreditPostfix(Player __instance)
        {
            try
            {
                DeathCreditHelper.OnLocalPlayerDeath(__instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DeathCredit Player.OnDeath postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnSwimming")]
        public static void PlayerOnSwimming_Postfix(Player __instance)
        {
            try
            {
                if (__instance == Player.m_localPlayer)
                    DeathCreditHelper.NoteSwimming();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DeathCredit Player.OnSwimming postfix: " + e);
            }
        }

        // ── boats: who destroyed the streamer's boat ──────────────────────────────────────────

        // First, so an earlier prefix returning false (e.g. WearNTearDamage_Prefix) can't skip it.
        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(WearNTear), "Damage")]
        public static void WearNTearDamage_NotePrefix(WearNTear __instance, HitData hit)
        {
            try
            {
                DeathCreditHelper.NoteShipHit(__instance, hit);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DeathCredit WearNTear.Damage prefix: " + e);
            }
        }

        // Private Destroy(HitData, bool): reached both when damage destroys the piece and through RPC_Remove.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(WearNTear), "Destroy", new Type[] { typeof(HitData), typeof(bool) })]
        public static void WearNTearDestroy_Prefix(WearNTear __instance)
        {
            try
            {
                DeathCreditHelper.NoteShipDestroyed(__instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DeathCredit WearNTear.Destroy prefix: " + e);
            }
        }
    }
}
