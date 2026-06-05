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
        // One SoulReaper per weapon, keyed by item shared name
        private static readonly Dictionary<string, SoulReaper> _reapers
            = new Dictionary<string, SoulReaper>();

        private static void RegisterReaper(string weaponName, string zdoKeyName, GameObject apparitionPrefab, Color emissionColor)
        {
            SoulReaper reaper = Game.instance.gameObject.AddComponent<SoulReaper>();
            reaper.Init(zdoKeyName);
            reaper.m_apparitionPrefab = apparitionPrefab;
            reaper.m_emissionColor = emissionColor;
            _reapers[weaponName] = reaper;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Awake")]
        public static void GameAwake_Postfix()
        {
            try
            {
                RegisterReaper(
                    PluginConfig.scythe1Name,
                    zdoKeyName:       "SoulReaperCharge_1",
                    apparitionPrefab: ModularMagic_BloodMagic.Instance.prefabs.SpawnAbilityScythe1,
                    emissionColor:    new Color(0f, 2.666667f, 2.996078f));

                RegisterReaper(
                    PluginConfig.scythe2Name,
                    zdoKeyName:       "SoulReaperCharge_2",
                    apparitionPrefab: ModularMagic_BloodMagic.Instance.prefabs.SpawnAbilityScythe2,
                    emissionColor:    new Color(2.839216f, 0f, 4f));

                RegisterReaper(
                    PluginConfig.scythe3Name,
                    zdoKeyName:       "SoulReaperCharge_3",
                    apparitionPrefab: ModularMagic_BloodMagic.Instance.prefabs.SpawnAbilityScythe3,
                    emissionColor:    new Color(4f, 0.03725543f, 0f));

                RegisterReaper(
                    PluginConfig.scythe4Name,
                    zdoKeyName:       "SoulReaperCharge_4",
                    apparitionPrefab: ModularMagic_BloodMagic.Instance.prefabs.SpawnAbilityScythe4,
                    emissionColor:    new Color(0.43921569f, 4f, 0f));
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not register SoulReaper components in GameAwake_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "Damage")]
        public static void CharacterDamage_Postfix(Character __instance, ref HitData hit)
        {
            if (Player.m_localPlayer == null)
                return;

            Character attacker = hit.GetAttacker();
            if (attacker == null || attacker != Player.m_localPlayer)
                return;

            SoulReaper? reaper = SoulReaperHelper.GetDrawnReaper(_reapers, out string? weaponName);
            if (reaper == null || weaponName == null)
                return;

            if (reaper.CanDischarge())
            {
                reaper.ResetCharge();
                Jotunn.Logger.LogInfo("Soul Reaper discharged — spawning apparition");
                SoulReaperHelper.SpawnApparition(__instance, reaper.m_apparitionPrefab, weaponName);
            }
            else if (Player.m_localPlayer.GetEitr() > 10f)
            {
                Player.m_localPlayer.UseEitr(10f);
                reaper.Charge(0.1f);
                Jotunn.Logger.LogInfo("Charging Soul Reaper: " + reaper.m_charge);
                SoulReaperHelper.SpawnChargeEffects(hit.m_point, hit.m_dir);
            }
            else
            {
                Jotunn.Logger.LogInfo("Soul Reaper cannot charge — insufficient eitr.");
            }
        }

        /// <summary>
        /// Fires every frame for every player — reads their ZDO charge and
        /// applies weapon visuals so all clients see the correct state.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "Update")]
        public static void PlayerUpdate_Postfix(Player __instance)
        {
            if (__instance == null || __instance.m_nview == null || !__instance.m_nview.IsValid())
                return;

            foreach (KeyValuePair<string, SoulReaper> pair in _reapers)
            {
                bool hasWeapon = __instance == Player.m_localPlayer
                    ? SoulReaperHelper.IsWeaponEquipped(__instance, pair.Key)
                    : SoulReaperHelper.HasCachedAttach(__instance);

                if (!hasWeapon)
                    continue;

                float charge = __instance.m_nview.GetZDO().GetFloat(pair.Value.ZDOKey, 0f);
                SoulReaperHelper.ApplyChargeVisualsForPlayer(__instance, charge, pair.Value.m_maxCharge, pair.Value.m_emissionColor);
                break; // Only one scythe can be equipped at a time
            }
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
