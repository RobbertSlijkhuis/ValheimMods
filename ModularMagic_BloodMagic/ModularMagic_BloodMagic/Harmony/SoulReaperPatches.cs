using HarmonyLib;
using Jotunn.Managers;
using ModularMagic_BloodMagic.Components;
using ModularMagic_BloodMagic.Configs;
using ModularMagic_BloodMagic.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_BloodMagic.Harmony
{
    [HarmonyPatch]
    public class SoulReaperPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Awake")]
        public static void GameAwake_Postfix()
        {
            try
            {
                SoulReaperHelper.RegisterReapers();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not register SoulReaper components in GameAwake_Postfix: " + e);
            }
        }

        // A hit of the local player with a scythe charges it, and a full scythe summons an apparition
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "Damage")]
        public static void CharacterDamage_Postfix(Character __instance, ref HitData hit)
        {
            SoulReaperHelper.OnLocalPlayerHit(__instance, hit);
        }

        /// <summary>
        /// Fires every frame for every player: shows the charge of their scythe on the weapon so all clients see the correct state,
        /// and keeps the Foul Magic Exhaustion of the local player equal to its active apparitions.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "Update")]
        public static void PlayerUpdate_Postfix(Player __instance)
        {
            SoulReaperHelper.UpdateChargeVisuals(__instance);
            ExhaustionHelper.Update(__instance);
        }

        /// <summary>
        /// Fires for every player when any item is attached — registers the clone
        /// so PlayerUpdate_Postfix can apply visuals without path searches.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VisEquipment), "AttachItem")]
        public static void VisEquipmentAttachItem_Postfix(VisEquipment __instance, Transform joint, GameObject __result)
        {
            if (__result == null)
                return;

            Player? player = __instance.GetComponent<Player>();
            if (player == null)
                return;

            bool isRightHand = joint.name == "RightHand_Attach";
            bool isBack      = joint.name == "BackTwohanded_attach";

            if (!isRightHand && !isBack)
                return;

            if (__result.transform.Find("scythe") == null || __result.transform.Find("charged") == null)
                return;

            SoulReaperHelper.RegisterAttachForPlayer(player, __result);
        }

        /// <summary>
        /// Cleans up cached attach references when a player is destroyed (logout/disconnect).
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "OnDestroy")]
        public static void PlayerOnDestroy_Postfix(Player __instance)
        {
            SoulReaperHelper.UnregisterPlayerAttaches(__instance);
        }
    }
}
