using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using WizshBoneTwitchIntegration.Components;

namespace WizshBoneTwitchIntegration.Harmony
{
    [HarmonyPatch]
    public class ShipPatchesWBTI
    {
        private static readonly FieldInfo s_lineAttachRendererField = typeof(LineAttach)
            .GetField("m_lineRenderer", BindingFlags.NonPublic | BindingFlags.Instance);

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

        // Ship.CustomFixedUpdate runs a full buoyancy simulation every physics tick for every ship
        // regardless of occupancy - expensive with many redeem-spawned boats piled up at once (e.g.
        // Boatpocalypse). Throttle idle ones via TwitchShipIdlePhysicsData; player-built ships have no
        // such component so GetComponent returns null and this is a no-op for them. HarmonyLib matches
        // prefix parameters by name - "fixedDeltaTime" must match the original's parameter name
        // exactly, and `ref` lets us rewrite the value the original (and any other prefix on this same
        // method, e.g. TimeStopPatchesWBTI's freeze-zone prefix) actually sees. If any prefix on a
        // method returns false, the original is skipped, so this coexists fine alongside that one.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Ship), nameof(Ship.CustomFixedUpdate))]
        public static bool Ship_CustomFixedUpdate_Prefix(Ship __instance, ref float fixedDeltaTime)
        {
            TwitchShipIdlePhysicsData idlePhysics = __instance.GetComponent<TwitchShipIdlePhysicsData>();
            return idlePhysics == null || idlePhysics.ShouldRunTick(ref fixedDeltaTime);
        }

        // LineAttach.OnEnable() adds itself to the update list synchronously and immediately the
        // moment a spawned object becomes active, but LineAttach.Start() - the only place its private
        // m_lineRenderer field is ever assigned - is deferred by Unity to just before that object's
        // first Update. A boat instantiated from within a coroutine continuation (e.g. our Spawn2,
        // rather than a plain MonoBehaviour.Update()) can have that same frame's LateUpdate run
        // before the deferred Start() has fired, so m_lineRenderer is still null the first time
        // CustomLateUpdate executes - vanilla never null-checks it, throwing a NullReferenceException
        // every time a coroutine-driven redeem (e.g. Boatpocalypse) creates a rigged boat. Not a
        // Ship-specific bug (LineAttach is generic vanilla code, used for any rigging/rope visual),
        // but this is the only place we've actually observed it fire, so it's guarded here.
        [HarmonyPrefix]
        [HarmonyPatch(typeof(LineAttach), nameof(LineAttach.CustomLateUpdate))]
        public static bool LineAttach_CustomLateUpdate_Prefix(LineAttach __instance)
        {
            return s_lineAttachRendererField?.GetValue(__instance) != null;
        }
    }
}
