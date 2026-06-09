using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Components;
using WizshBoneTwitchIntegration.Gui;
using WizshBoneTwitchIntegration.Helpers;
using WizshBoneTwitchIntegration.TwitchIntegration;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class LoginPatchesWBTI
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
                Game.instance.gameObject.AddComponent<SafeZoneHUDPanel>();
                Game.instance.gameObject.AddComponent<StatusEffectManager>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add Twitch components in GameAwake_Postfix: " + e);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Menu), "OnLogoutYes")]
        public static bool OnLogoutYes_Postfix()
        {
            try
            {
                TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();

                if (auth == null)
                    return true;

                if (auth.m_loggedIn)
                {
                    ExtraConfigHelper.WriteBannedUsersToFile(auth.m_customRewards.m_bannedUsers);
                    auth.LogoutBackToMainMenu();
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not clear rewards on OnLogoutYes_Postfix: " + e);
                return true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Menu), "OnQuitYes")]
        public static bool OnQuitYes_Prefix(ref Menu __instance)
        {
            try
            {
                if (__instance == null)
                    return true;

                TwitchAuth auth = Game.instance.gameObject.GetComponent<TwitchAuth>();

                if (auth == null)
                    return true;

                if (auth.m_loggedIn)
                {
                    ExtraConfigHelper.WriteBannedUsersToFile(auth.m_customRewards.m_bannedUsers);
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Clearing redeems, please wait!", 1000);
                    auth.LogoutQuitApplication();
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not clear rewards on OnQuitYes_Prefix: " + e);
                return true;
            }
        }
    }
}
