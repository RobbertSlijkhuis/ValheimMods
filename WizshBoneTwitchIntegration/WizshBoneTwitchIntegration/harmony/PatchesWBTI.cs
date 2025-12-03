using HarmonyLib;
using System;
using System.Collections.Generic;
using WizshBoneTwitchIntegration.Models;
using WizshBoneTwitchIntegration.TwitchIntegration;
using WizshBoneTwitchIntegration.Types;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class PatchesWBTI
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Awake")]
        public static void GameAwake_Postfix()
        {
            try
            {
                Jotunn.Logger.LogWarning("Adding twitch components to Game");
                Game.instance.gameObject.AddComponent<TwitchChat>();
                Game.instance.gameObject.AddComponent<TwitchCustomRewards>();
                Game.instance.gameObject.AddComponent<TwitchAuth>();
                Game.instance.gameObject.AddComponent<TwitchChatting>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add Twitch components in GameAwake_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Shutdown")]
        public static void Shutdown_Postfix(ref PlayerController __instance)
        {
            try
            {
                TwitchCustomRewards rewardComp = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

                if (rewardComp == null)
                    return;

                Jotunn.Logger.LogWarning("Disabling redeems");
                rewardComp.ClearRewards();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not clear rewards on Shutdown_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ZoneSystem), "GlobalKeyAdd")]
        public static void GlobalKeyAdd_Postfix(ref ZoneSystem __instance, string keyStr, bool canSaveToServerOptionKeys = true)
        {
            try
            {
                TwitchAuth authComp = Game.instance.gameObject.GetComponent<TwitchAuth>();

                if (!authComp || !authComp.isLoggedIn || !keyStr.Contains("defeated_"))
                    return;

                TwitchCustomRewards rewardComp = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

                if (rewardComp == null)
                    return;

                List<RedeemEntry> redeemList = new List<RedeemEntry>();

                switch (keyStr)
                {
                    case nameof(GlobalKeyType.DefeatedEikthyr):
                        redeemList.AddRange(rewardComp.m_redeems.list.FindAll(item => item.globalKey == GlobalKeyType.DefeatedEikthyr));
                        break;
                    case nameof(GlobalKeyType.DefeatedElder):
                        redeemList.AddRange(rewardComp.m_redeems.list.FindAll(item => item.globalKey == GlobalKeyType.DefeatedElder));
                        break;
                    case nameof(GlobalKeyType.DefeatedBonemass):
                        redeemList.AddRange(rewardComp.m_redeems.list.FindAll(item => item.globalKey == GlobalKeyType.DefeatedBonemass));
                        break;
                    case nameof(GlobalKeyType.DefeatedModer):
                        redeemList.AddRange(rewardComp.m_redeems.list.FindAll(item => item.globalKey == GlobalKeyType.DefeatedModer));
                        break;
                }

                rewardComp.SetRewards(redeemList);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not clear rewards on GlobalKeyAdd_Postfix: " + e);
            }
        }
    }
}
