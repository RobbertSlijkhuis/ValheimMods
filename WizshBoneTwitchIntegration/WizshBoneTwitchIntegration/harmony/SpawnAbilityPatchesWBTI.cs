using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Extensions;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class SpawnAbilityPatchesWBTI
    {
        // Redirects SpawnAbility.Setup - the entry point used whenever an attack fires a summon
        // (player summon staffs, wild monster summons, and a Twitch-spawned creature's own
        // reinforcements alike) - to the SpawnAbilityExtension.Spawn2(Character) overload, but only
        // when the summoning creature (owner) is itself a Twitch spawn. Everything else (player-used
        // staffs, wild-monster-on-monster summons) falls through to vanilla untouched. See
        // CreatureHelper.ApplyInheritedScaling for why this exists.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(SpawnAbility), nameof(SpawnAbility.Setup))]
        public static bool Setup_Prefix(SpawnAbility __instance, Character owner, ItemDrop.ItemData item)
        {
            try
            {
                if (owner == null)
                    return true;

                TwitchCreatureClaim ownerClaim = owner.gameObject.GetComponent<TwitchCreatureClaim>();

                if (ownerClaim == null || !ownerClaim.m_isSpawn)
                    return true;

                __instance.m_owner = owner;
                __instance.m_weapon = item;
                __instance.StartCoroutine(__instance.Spawn2(owner));
                return false;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in Setup_Prefix: " + e);
                return true;
            }
        }
    }
}
