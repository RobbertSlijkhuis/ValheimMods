using HarmonyLib;
using ModularMagic_BloodMagic.Components;
using ModularMagic_BloodMagic.Configs;
using ModularMagic_BloodMagic.Helpers;
using System;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_BloodMagic.Harmony
{
    [HarmonyPatch]
    public class SoulReaperPatches
    {
        private static SoulReaper? _soulReaper;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Awake")]
        public static void GameAwake_Postfix()
        {
            try
            {
                _soulReaper                    = Game.instance.gameObject.AddComponent<SoulReaper>();
                _soulReaper.m_apparitionPrefab = ModularMagic_BloodMagic.Instance.prefabs.ScytheSkeletonSpawn;
                _soulReaper.m_apparitionName   = PluginConfig.scythe1.summonPrefab?.Value ?? "Apparition";
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add SoulReaper component in GameAwake_Postfix: " + e);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "Damage")]
        public static void CharacterDamage_Postfix(Character __instance, ref HitData hit)
        {
            if (_soulReaper == null || Player.m_localPlayer == null)
                return;

            Character attacker = hit.GetAttacker();
            if (attacker == null || attacker != Player.m_localPlayer)
                return;

            ItemData? scythe = Player.m_localPlayer.GetInventory()
                .GetEquippedItems()
                .Find(item => item.m_shared.m_name == PluginConfig.scythe1Name);

            if (scythe == null)
                return;

            if (_soulReaper.CanDischarge())
            {
                _soulReaper.ResetCharge();
                Jotunn.Logger.LogInfo("Soul Reaper discharged — spawning apparition");
                SoulReaperHelper.SpawnApparition(__instance, _soulReaper.m_apparitionPrefab, _soulReaper.m_apparitionName);
            }
            else if (Player.m_localPlayer.GetEitr() > 10f)
            {
                Player.m_localPlayer.UseEitr(10f);
                _soulReaper.Charge(0.1f);
                Jotunn.Logger.LogInfo("Charging Soul Reaper: " + _soulReaper.m_charge);

                if (ModularMagic_BloodMagic.Instance.prefabs.ScytheHitEffect != null)
                {
                    GameObject vfxInstance = UnityEngine.Object.Instantiate(
                        ModularMagic_BloodMagic.Instance.prefabs.ScytheHitEffect,
                        hit.m_point,
                        Quaternion.LookRotation(-hit.m_dir));
                    vfxInstance.transform.localScale = Vector3.one * 0.4f;
                }
                else
                    Jotunn.Logger.LogWarning("Could not find scythe hit effect prefab.");
            }
            else
            {
                Jotunn.Logger.LogInfo("Soul Reaper cannot charge — insufficient eitr.");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "Dodge")]
        public static void PlayerDodge_Postfix(Player __instance)
        {
            if (_soulReaper == null || Player.m_localPlayer == null)
                return;

            if (__instance != Player.m_localPlayer)
                return;

            if (!_soulReaper.CanDischarge())
                return;

            ItemData? scythe = Player.m_localPlayer.GetInventory()
                .GetEquippedItems()
                .Find(item => item.m_shared.m_name == PluginConfig.scythe1Name);

            if (scythe == null)
                return;

            _soulReaper.ResetCharge();
            Jotunn.Logger.LogInfo("Soul Reaper charge consumed on dodge — applying shield");

            SoulReaperHelper.ApplyShield(__instance);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tameable), "Awake")]
        public static void TameableAwake_Postfix(Tameable __instance)
        {
            if (_soulReaper == null || Player.m_localPlayer == null)
                return;

            if (__instance.GetText() != _soulReaper.m_apparitionName)
                return;

            if (__instance.m_monsterAI == null)
                return;

            CharacterDrop? characterDrop = __instance.GetComponent<CharacterDrop>();
            if (characterDrop != null)
                characterDrop.m_drops.Clear();

            __instance.m_monsterAI.SetFollowTarget(Player.m_localPlayer.gameObject);
            Jotunn.Logger.LogInfo($"Restored follow target for '{_soulReaper.m_apparitionName}' after reload.");
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

            // For the local player use inventory check — for remote players rely on
            // the attach cache since inventory fields are not reliable cross-client
            if (__instance == Player.m_localPlayer)
            {
                if (!SoulReaperHelper.IsWeaponEquipped(__instance, PluginConfig.scythe1Name))
                    return;
            }
            else
            {
                if (!SoulReaperHelper.HasCachedAttach(__instance))
                    return;
            }

            float charge = __instance.m_nview.GetZDO().GetFloat(SoulReaper.ZDOKey, 0f);
            SoulReaperHelper.ApplyChargeVisualsForPlayer(__instance, charge, _soulReaper?.m_maxCharge ?? 1f);
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
