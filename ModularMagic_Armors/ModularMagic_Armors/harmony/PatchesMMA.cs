using HarmonyLib;
using Jotunn.Managers;
using ModularMagic_Armors.Components;
using ModularMagic_Armors.Configs;
using ModularMagic_Armors.Helpers;
using ModularMagic_Utilities.Models;
using System;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

namespace ModularMagic_Armors.Harmony
{
    [HarmonyPatch]
    public class PatchesMMA
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerController), "Awake")]
        public static void AwakePlayerController_Postfix(ref PlayerController __instance)
        {
            try
            {
                if (__instance.GetComponent<ArmorMMA>() == null)
                    __instance.gameObject.AddComponent<ArmorMMA>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add component in AwakePlayerController_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnSpawned")]
        public static void OnSpawned_Postfix(ref Player __instance)
        {
            try
            {
                if (__instance == null)
                    return;

                ArmorMMA comp = __instance.GetComponent<ArmorMMA>();
                long playerId = __instance.GetPlayerID();
                int? armorSetHash = comp.GetArmorSetHashFromPlayer(playerId);
                List<int> armorPieces = comp.GetArmorHashesFromPlayer(playerId);
                ArmorStatus armorStatus = new ArmorStatus(playerId, armorSetHash, armorPieces);
                comp.SetPlayerStatus(playerId, armorStatus);

                if (!ModularMagic_Armors.Instance.gameIsReady)
                    ModularMagic_Armors.Instance.gameIsReady = true;
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError(e);
            }
        }

        // Runs every frame for every VisEquipment, so the accessory is checked first
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VisEquipment), "GetHairItem")]
        public static void GetHairItem_Postfix(VisEquipment __instance, ItemDrop.ItemData.AccessoryType accessory, ref int __result)
        {
            if (accessory != ItemDrop.ItemData.AccessoryType.Beard || __result == 0)
                return;

            if (BeardHelper.ShouldHideBeard(__instance))
                __result = 0;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "EquipItem")]
        public static void EquipItem_Postfix(ref Humanoid __instance, ItemDrop.ItemData item)
        {
            try
            {
                if (!ModularMagic_Armors.Instance.gameIsReady || __instance == null || !__instance.IsPlayer() || item == null)
                    return;

                CheckEquipment();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update item/set effects in EquipItem_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Humanoid), "UnequipItem")]
        public static void UnequipItem_Postfix(ref Humanoid __instance, ItemDrop.ItemData item)
        {
            try
            {
                if (!ModularMagic_Armors.Instance.gameIsReady || __instance == null || !__instance.IsPlayer() || item == null)
                    return;

                CheckEquipment();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update item/sets effects in UnequipItem_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FootStep), "DoEffect")]
        public static void DoEffect_Postfix(FootStep __instance, UnityEngine.Vector3 point)
        {
            FootstepHelper.AddDarkWizardFootstep(__instance, point, __instance.transform.position - __instance.m_lastPosition);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "GetTotalFoodValue")]
        public static void GetTotalFoodValue_Postfix(ref Player __instance, ref float eitr)
        {
            try
            {
                eitr += StatsHelper.GetEquippedEitr(__instance);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not update Eitr in GetTotalFoodValue_Postfix: " + e);
            }
        }

        private static void CheckEquipment()
        {
            if (Player.m_localPlayer == null)
                return;

            ArmorMMA comp = Player.m_localPlayer.GetComponent<ArmorMMA>();
            ArmorStatus armorStatus = comp.GetPlayerStatus();

            if (armorStatus == null)
                return;

            long playerId = Player.m_localPlayer.GetPlayerID();
            int? armorSetHash = comp.GetArmorSetHashFromPlayer(playerId);
            List<int> armorPieces = comp.GetArmorHashesFromPlayer(playerId);
            armorStatus.set = armorSetHash;
            armorStatus.items = armorPieces;
            comp.SetPlayerStatus(playerId, armorStatus);
        }
    }
}
