using HarmonyLib;
using System;
using WizshBoneTwitchIntegration.Gui;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    internal class RedeemCatalogPatchesWBTI
    {
        // RedeemPrefabCatalog.WeatherNames needs an actual loaded world (EnvMan doesn't exist at
        // PrefabManager.OnVanillaPrefabsAvailable time, unlike the ZNetScene/ObjectDB-backed
        // catalogs built there) - OnSpawned is the established "world is actually loaded" hook
        // this mod already uses elsewhere (FlashbangPatchesWBTI, StatusEffectPatchesWBTI).
        // BuildWeatherCatalogOnce guards against rebuilding on every respawn.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static void OnSpawned_Postfix(Player __instance)
        {
            try
            {
                if (Player.m_localPlayer == null || Player.m_localPlayer.GetPlayerID() != __instance.GetPlayerID())
                    return;

                RedeemPrefabCatalog.BuildWeatherCatalogOnce();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Something went wrong in RedeemCatalogPatchesWBTI.OnSpawned_Postfix: " + e);
            }
        }
    }
}
