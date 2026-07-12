using HarmonyLib;
using System;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class ShipPatchesWBTI
    {
        // Ship safezones must only exist on boats a player actually built, not on redeem-spawned
        // ones - so attach here, at the moment of placement, instead of baking TwitchSafeZone onto
        // the shared ship prefab. Player.PlacePiece only has the original prefab reference, not the
        // placed instance (it's created via a local Instantiate() call inside that method), so patch
        // WearNTear.OnPlaced instead - decompiled assembly_valheim.dll confirms it's called exactly
        // once, from Player.PlacePiece right after the new instance is created, giving __instance as
        // that exact placed GameObject. Adds directly for the placing client (immediate effect) and
        // flags the ZDO so every other client's own TwitchBasePersistentData.Awake() attaches its
        // own copy too, per the mod's usual ZDO-flag rehydration pattern.
        [HarmonyPostfix]
        [HarmonyPatch(typeof(WearNTear), nameof(WearNTear.OnPlaced))]
        public static void WearNTear_OnPlaced_Postfix(WearNTear __instance)
        {
            try
            {
                GameObject shipRoot = __instance.gameObject;

                if (shipRoot.GetComponent<Ship>() == null)
                    return;

                TwitchSafeZone.AttachToShip(shipRoot);
                shipRoot.GetComponent<TwitchBasePersistentData>()?.SetFlag(PersistentComponentFlags.ShipSafeZone, true);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogWarning($"[WBTI] ShipPatchesWBTI.WearNTear_OnPlaced_Postfix: {e}");
            }
        }
    }
}
