using HarmonyLib;
using Jotunn.Managers;
using PlayAsSkeleton.Components;
using PlayAsSkeleton.Configs;
using PlayAsSkeleton.Helpers;
using PlayAsSkeleton.Models;
using PlayAsSkeleton.Types;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;

namespace PlayAsSkeleton.Harmony
{
    [HarmonyPatch]
    public class PatchesPAS
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerController), "Awake")]
        public static void AwakePlayerController_Postfix(ref PlayerController __instance)
        {
            try
            {
                if (__instance.GetComponent<SkeletonPAS>() == null)
                    __instance.gameObject.AddComponent<SkeletonPAS>();
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

                SkeletonPAS comp = __instance.GetComponent<SkeletonPAS>();
                string customData = __instance.m_customData.GetValueSafe(PlayAsSkeleton.playerDataKey);
                long playerID = __instance.GetPlayerID();
                PlayAsSkeleton.playerID = playerID;
                comp.SetPlayerID(playerID);

                if (customData != null)
                {
                    string[] data = customData.Split(',');
                    comp.SetIsSkeleton(data[1].ToLower() == "true" ? true : false, false);
                    comp.SetSkin(data[2], false);
                    comp.SetCanSwim(data[3].ToLower() == "true" ? true : false);
                    comp.UpdateCurrentSettingValues();
                }
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add player ID in OnSpawned_Postfix: " + e);
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(VisEquipment), "UpdateEquipmentVisuals")]
        //public static void UpdateEquipmentVisuals_Postfix(ref VisEquipment __instance)
        //{
        //    try
        //    {
        //        if (__instance == null)
        //            return;

        //        SkeletonPAS comp = __instance.gameObject.GetComponent<SkeletonPAS>();

        //        if (comp == null || !comp.GetIsInitialised() || !comp.GetIsEnabled())
        //            return;

        //        SkeletonHelper.HideEquipment(__instance, VisEquipmentSlot.Helmet, (bool)comp.GetHideSlot(VisEquipmentSlot.Helmet));
        //        SkeletonHelper.HideEquipment(__instance, VisEquipmentSlot.Cape, (bool)comp.GetHideSlot(VisEquipmentSlot.Cape));
        //        SkeletonHelper.HideEquipment(__instance, VisEquipmentSlot.Chest, (bool)comp.GetHideSlot(VisEquipmentSlot.Chest));
        //        SkeletonHelper.HideEquipment(__instance, VisEquipmentSlot.Utility, (bool)comp.GetHideSlot(VisEquipmentSlot.Utility));
        //        SkeletonHelper.HideEquipment(__instance, VisEquipmentSlot.Legs, (bool)comp.GetHideSlot(VisEquipmentSlot.Legs));
        //    }
        //    catch (Exception e)
        //    {
        //        Jotunn.Logger.LogError("Something went wrong in UpdateEquipmentVisuals_Postfix: " + e);
        //    }
        //}
    }
}
