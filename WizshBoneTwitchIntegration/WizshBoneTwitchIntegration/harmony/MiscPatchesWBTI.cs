using HarmonyLib;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Configs;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class MiscPatchesWBTI
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZoneSystem), "GlobalKeyAdd")]
        public static void GlobalKeyAdd_Postfix(ref ZoneSystem __instance, string keyStr, bool canSaveToServerOptionKeys = true)
        {
            try
            {
                TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();
                string loweredKey = keyStr.ToLower();

                if (!auth || !auth.m_loggedIn || (!loweredKey.Contains("defeated_") && !loweredKey.Contains("killed")))
                    return;

                TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

                if (customRewards == null)
                    return;

                if (customRewards.m_enabled)
                    customRewards.SetRewards();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update rewards on GlobalKeyAdd_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Emote), "DoEmote")]
        public static void DoEmote_Postfix(Emotes emote)
        {
            try
            {
                if (emote == Emotes.ComeHere)
                    CreatureHelper.SetFollowInRadius(true);
                else if (emote == Emotes.NoNoNo)
                    CreatureHelper.SetFollowInRadius(false);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in DoEmote_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tameable), "Command")]
        public static void TameableCommand_Postfix(Tameable __instance)
        {
            try
            {
                TwitchCreaturePersistentData persistentData = __instance.gameObject.GetComponent<TwitchCreaturePersistentData>();

                if (persistentData == null)
                    return;

                bool isNowFollowing = __instance.m_monsterAI.GetFollowTarget() != null;
                persistentData.SetFollowing(isNowFollowing);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in TameableCommand_Postfix: " + e);
            }
        }

        // SAPHONETTE-CLEANUP: wires up the temporary custom death sound (see DeathSoundHelper).
        // CreateDeathEffects is private (patched by string name) and, per decompiled
        // Player.OnDeath(), only ever runs on whichever client owns that Player instance - so this
        // fires exactly once per real death, on the right client, with no IsOwner check needed here.
        // Hooking the actual death-effect moment (rather than baking something in back at
        // Player.Awake) is also what lets DeathSoundHelper.Enabled be read live.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "CreateDeathEffects")]
        public static void CreateDeathEffects_Postfix(Player __instance)
        {
            try
            {
                DeathSoundHelper.PlayIfEnabled(__instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in CreateDeathEffects_Postfix: " + e);
            }
        }
    }
}
