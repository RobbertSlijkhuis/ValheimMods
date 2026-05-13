using HarmonyLib;
using Jotunn.Managers;
using ModularMagic_BloodMagic.Components;
using ModularMagic_BloodMagic.Configs;
using System;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_BloodMagic.Harmony
{
    [HarmonyPatch]
    public class SoulReaperPatches
    {
        private static SoulReaper? _soulReaper;
        private static readonly MaterialPropertyBlock _propBlock = new MaterialPropertyBlock();
        private static readonly int _emissionColorId = Shader.PropertyToID("_EmissionColor");

        private static MeshRenderer? _cachedMeshRenderer;
        private static GameObject? _cachedChargedObject;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Game), "Awake")]
        public static void GameAwake_Postfix()
        {
            try
            {
                _soulReaper = Game.instance.gameObject.AddComponent<SoulReaper>();
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not add SoulReaper component in GameAwake_Postfix: " + e);
            }
        }

        /// <summary>
        /// Fires immediately after VisEquipment instantiates any attach clone.
        /// Applies the current charge state if the clone belongs to the scythe.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(VisEquipment), "AttachItem")]
        public static void VisEquipmentAttachItem_Postfix(VisEquipment __instance, Transform joint, GameObject __result)
        {
            if (_soulReaper == null || Player.m_localPlayer == null || __result == null)
                return;

            if (__instance != Player.m_localPlayer.GetComponent<VisEquipment>())
                return;

            // Only handle right hand and back attach points
            bool isRightHand = joint.name == "RightHand_Attach";
            bool isBack      = joint.name == "BackTwohanded_attach";

            if (!isRightHand && !isBack)
                return;

            // Verify this clone actually belongs to the scythe by checking its children
            MeshRenderer? renderer = __result.transform.Find("default (1)")?.GetComponent<MeshRenderer>();
            GameObject? charged    = __result.transform.Find("charged")?.gameObject;

            if (renderer == null || charged == null)
                return;

            // Refresh the right hand cache when the equipped clone is (re)created
            if (isRightHand)
            {
                _cachedMeshRenderer  = renderer;
                _cachedChargedObject = charged;
            }

            ApplyChargeVisuals(renderer, charged);
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

            ItemData scythe = Player.m_localPlayer.GetInventory()
                .GetEquippedItems()
                .Find(item => item.m_shared.m_name == PluginConfig.scythe1Name);

            if (scythe == null)
            {
                InvalidateCache();
                return;
            }

            if (!TryGetWeaponComponents(out MeshRenderer meshRenderer, out GameObject chargedObject))
                return;

            if (_soulReaper.CanDischarge())
            {
                _soulReaper.ResetCharge();
                Jotunn.Logger.LogInfo("Soul Reaper fully charged, resetting charge");
                RemoveChargeStatusEffect();
                SpawnGhost(__instance);
            }
            else
            {
                // Only charge if the player has eitr available
                if (Player.m_localPlayer.GetEitr() > 10f)
                {
                    Player.m_localPlayer.UseEitr(10f);
                    _soulReaper.Charge(0.1f);
                    Jotunn.Logger.LogInfo("Charging Soul Reaper: " + _soulReaper.m_charge);

                    UpdateChargeStatusEffect();

                    if (_soulReaper.m_charge >= _soulReaper.m_maxCharge)
                        Jotunn.Logger.LogInfo("Soul Reaper is now fully charged!");
                }
                else
                {
                    Jotunn.Logger.LogInfo("Soul Reaper cannot charge — player has no eitr.");
                }
            }

            ApplyChargeVisuals(meshRenderer, chargedObject);
        }

        /// <summary>
        /// Fires when the player performs a dodge roll.
        /// Consumes a full charge to apply a shield instead.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Player), "Dodge")]
        public static void PlayerDodge_Postfix(Player __instance)
        {
            if (_soulReaper == null || Player.m_localPlayer == null)
                return;

            if (__instance != Player.m_localPlayer)
                return;

            if (_soulReaper.m_charge < _soulReaper.m_maxCharge)
                return;

            ItemData scythe = Player.m_localPlayer.GetInventory()
                .GetEquippedItems()
                .Find(item => item.m_shared.m_name == PluginConfig.scythe1Name);

            if (scythe == null)
                return;

            if (!_soulReaper.CanDischarge())
                return;

            _soulReaper.ResetCharge();
            Jotunn.Logger.LogInfo("Soul Reaper charge consumed on dodge — applying shield");

            RemoveChargeStatusEffect();
            ApplyShield(__instance);

            // Sync visuals to reflect the reset charge
            if (TryGetWeaponComponents(out MeshRenderer meshRenderer, out GameObject chargedObject))
                ApplyChargeVisuals(meshRenderer, chargedObject);
        }

        /// <summary>
        /// Adds or refreshes the SE_SoulReaperCharge status effect on the local player.
        /// </summary>
        private static void UpdateChargeStatusEffect()
        {
            if (_soulReaper == null || Player.m_localPlayer == null)
                return;

            SEMan seman = Player.m_localPlayer.GetSEMan();

            // Reuse the existing instance if already active
            SE_SoulReaperCharge? existing = seman.GetStatusEffect(typeof(SE_SoulReaperCharge).Name.GetStableHashCode()) as SE_SoulReaperCharge;
            if (existing != null)
            {
                existing.m_soulReaper = _soulReaper;
                return;
            }

            // Create and register a new instance
            SE_SoulReaperCharge effect = ScriptableObject.CreateInstance<SE_SoulReaperCharge>();
            effect.name              = typeof(SE_SoulReaperCharge).Name;
            effect.m_soulReaper      = _soulReaper;

            seman.AddStatusEffect(effect);
        }

        /// <summary>
        /// Removes the SE_SoulReaperCharge status effect from the local player.
        /// </summary>
        private static void RemoveChargeStatusEffect()
        {
            if (Player.m_localPlayer == null)
                return;

            Player.m_localPlayer.GetSEMan()
                .RemoveStatusEffect(typeof(SE_SoulReaperCharge).Name.GetStableHashCode());
        }

        private static void ApplyShield(Player player)
        {
            StatusEffect? shieldTemplate = ObjectDB.instance?.GetStatusEffect("Staff_shield".GetStableHashCode());
            if (shieldTemplate == null)
            {
                Jotunn.Logger.LogError("Could not find Staff_shield status effect in ObjectDB.");
                return;
            }

            SE_Shield? shield = player.GetSEMan().AddStatusEffect(shieldTemplate) as SE_Shield;
            if (shield == null)
            {
                Jotunn.Logger.LogError("Could not cast active status effect to Staff_shield.");
                return;
            }

            shield.m_ttl               = 15f;
            shield.m_totalAbsorbDamage = 15f;
            shield.m_levelUpSkillFactor           = 0.2f;
            shield.m_absorbDamagePerSkillLevel     = 0.2f;

            Jotunn.Logger.LogInfo($"Shield applied — absorb: {shield.m_totalAbsorbDamage}, duration: {shield.m_ttl}s");
        }

        private static void ApplyChargeVisuals(MeshRenderer meshRenderer, GameObject chargedObject)
        {
            chargedObject.SetActive(_soulReaper!.m_charge >= _soulReaper.m_maxCharge);

            meshRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(_emissionColorId, new Color(0f, 2.666667f, 2.996078f) * _soulReaper.m_charge);
            meshRenderer.SetPropertyBlock(_propBlock);
        }

        private static bool TryGetWeaponComponents(out MeshRenderer meshRenderer, out GameObject chargedObject)
        {
            if (_cachedMeshRenderer != null && _cachedChargedObject != null)
            {
                meshRenderer = _cachedMeshRenderer;
                chargedObject = _cachedChargedObject;
                return true;
            }

            meshRenderer = null!;
            chargedObject = null!;
            return false;
        }

        private static void InvalidateCache()
        {
            _cachedMeshRenderer  = null;
            _cachedChargedObject = null;
        }

        /// <summary>
        /// Spawns a Ghost at the position of the struck character, allied to and following the local player.
        /// </summary>
        private static void SpawnGhost(Character target)
        {
            GameObject? prefab = PrefabManager.Instance.GetPrefab("Ghost");
            if (prefab == null)
            {
                Jotunn.Logger.LogError("Could not find Ghost prefab via PrefabManager.");
                return;
            }

            Vector3 spawnPos = target.transform.position
                - target.transform.forward * 1.5f
                + Vector3.up * 0.5f;

            GameObject? vfxPrefab = PrefabManager.Instance.GetPrefab("vfx_corpse_destruction_small");
            if (vfxPrefab != null)
                UnityEngine.Object.Instantiate(vfxPrefab, spawnPos, Quaternion.identity);
            else
                Jotunn.Logger.LogWarning("Could not find vfx_corpse_destruction_small prefab.");

            GameObject ghost = UnityEngine.Object.Instantiate(prefab, spawnPos, Quaternion.identity);

            // Clear the drop table so the ghost leaves no loot on death
            CharacterDrop? characterDrop = ghost.GetComponent<CharacterDrop>();
            if (characterDrop != null)
                characterDrop.m_drops.Clear();

            Character? ghostCharacter = ghost.GetComponent<Character>();
            if (ghostCharacter != null)
                ghostCharacter.m_faction = Character.Faction.Players;

            Tameable tameable = ghost.GetComponent<Tameable>();
            tameable.m_monsterAI.MakeTame();
            tameable.m_monsterAI.SetFollowTarget(Player.m_localPlayer.gameObject);
            tameable.SetText("Apparition");

            Jotunn.Logger.LogInfo($"Spawned Ghost at {spawnPos}");
        }

        /// <summary>
        /// Fires after Tameable.Awake — restores the follow target for apparitions
        /// owned by the local player after a zone reload.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Tameable), "Awake")]
        public static void TameableAwake_Postfix(Tameable __instance)
        {
            if (Player.m_localPlayer == null)
                return;

            if (__instance.GetText() != "Apparition")
                return;

            if (__instance.m_monsterAI == null)
                return;

            // Clear drops again after reload — the prefab defaults are restored on reinstantiation
            CharacterDrop? characterDrop = __instance.GetComponent<CharacterDrop>();
            if (characterDrop != null)
                characterDrop.m_drops.Clear();

            __instance.m_monsterAI.SetFollowTarget(Player.m_localPlayer.gameObject);
            Jotunn.Logger.LogInfo($"Restored follow target for Apparition after reload.");
        }
    }
}
