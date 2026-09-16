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
                Game.instance.gameObject.AddComponent<StatusEffectManager>();
                Game.instance.gameObject.AddComponent<WizshBoneGUI>();

                TwitchSafeZone.ResetLocalPlayerZoneCount();
                TwitchSafeZone.ResetActiveSafeZones();
                FlashBangHelper.ResetQueue();
                DetonateHelper.ResetQueue();
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
                    if (auth.m_customRewards.HasUnresolvedRedeems())
                    {
                        WizshBoneGUI gui = Game.instance.gameObject.GetComponent<WizshBoneGUI>();
                        gui?.ShowExitConfirm(
                            title:       "Log Out",
                            description: "Auto-resolve is off. Pending redeems won't be refunded automatically. Log out anyway?",
                            onConfirm:   () =>
                            {
                                ExtraConfigHelper.WriteBannedUsersToFile(auth.m_customRewards.m_bannedUsers);
                                auth.LogoutBackToMainMenu();
                            },
                            confirmText: "Log Out",
                            cancelText:  "Open History",
                            onCancel:    () => gui?.ShowHistory()
                        );
                        return false;
                    }

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
                    if (auth.m_customRewards.HasUnresolvedRedeems())
                    {
                        WizshBoneGUI gui = Game.instance.gameObject.GetComponent<WizshBoneGUI>();
                        gui?.ShowExitConfirm(
                            title:       "Quit Game",
                            description: "Auto-resolve is off. Pending redeems won't be refunded automatically. Quit anyway?",
                            onConfirm:   () =>
                            {
                                ExtraConfigHelper.WriteBannedUsersToFile(auth.m_customRewards.m_bannedUsers);
                                auth.LogoutQuitApplication();
                            },
                            confirmText: "Quit",
                            cancelText:  "Open History",
                            onCancel:    () => gui?.ShowHistory()
                        );
                        return false;
                    }

                    ExtraConfigHelper.WriteBannedUsersToFile(auth.m_customRewards.m_bannedUsers);
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
