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
                TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

                if (customRewards == null)
                    return;

                customRewards.ClearRewards();
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
                TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();

                if (!auth || !auth.isLoggedIn || !keyStr.Contains("defeated_"))
                    return;

                TwitchCustomRewards customRewards = Game.instance.gameObject.GetComponent<TwitchCustomRewards>();

                if (customRewards == null)
                    return;

                List<RedeemEntry> redeemList = new List<RedeemEntry>();

                switch (keyStr)
                {
                    case nameof(GlobalKeyType.DefeatedEikthyr):
                        redeemList.AddRange(customRewards.m_redeems.list.FindAll(item => item.globalKey == GlobalKeyType.DefeatedEikthyr));
                        break;
                    case nameof(GlobalKeyType.DefeatedElder):
                        redeemList.AddRange(customRewards.m_redeems.list.FindAll(item => item.globalKey == GlobalKeyType.DefeatedElder));
                        break;
                    case nameof(GlobalKeyType.DefeatedBonemass):
                        redeemList.AddRange(customRewards.m_redeems.list.FindAll(item => item.globalKey == GlobalKeyType.DefeatedBonemass));
                        break;
                    case nameof(GlobalKeyType.DefeatedModer):
                        redeemList.AddRange(customRewards.m_redeems.list.FindAll(item => item.globalKey == GlobalKeyType.DefeatedModer));
                        break;
                }

                customRewards.SetRewards(redeemList);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not clear rewards on GlobalKeyAdd_Postfix: " + e);
            }
        }
    }
}
