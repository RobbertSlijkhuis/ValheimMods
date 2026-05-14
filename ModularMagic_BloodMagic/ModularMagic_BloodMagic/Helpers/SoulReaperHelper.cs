using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_BloodMagic.Helpers
{
    internal static class SoulReaperHelper
    {
        private static readonly MaterialPropertyBlock PropBlock         = new MaterialPropertyBlock();
        private static readonly int                   EmissionColorId  = Shader.PropertyToID("_EmissionColor");
        private static readonly Color                 BaseEmissionColor = new Color(0f, 2.666667f, 2.996078f);

        // Keyed by player ZDOID — stores all scythe attach clones for each player
        private static readonly Dictionary<ZDOID, List<GameObject>> _playerAttachCache
            = new Dictionary<ZDOID, List<GameObject>>();

        /// <summary>
        /// Called from VisEquipmentAttachItem_Postfix — caches the direct attach clone
        /// reference for a player so PlayerUpdate_Postfix can find it without path searches.
        /// </summary>
        public static void RegisterAttachForPlayer(Player player, GameObject attachClone)
        {
            ZDOID id = player.GetZDOID();

            if (!_playerAttachCache.TryGetValue(id, out List<GameObject> list))
            {
                list = new List<GameObject>();
                _playerAttachCache[id] = list;
            }

            if (!list.Contains(attachClone))
                list.Add(attachClone);
        }

        public static void UnregisterPlayerAttaches(Player player)
        {
            _playerAttachCache.Remove(player.GetZDOID());
        }

        /// <summary>
        /// Applies charge visuals to all cached attach clones for a player.
        /// Called every frame from PlayerUpdate_Postfix.
        /// </summary>
        public static void ApplyChargeVisualsForPlayer(Player player, float charge, float maxCharge)
        {
            ZDOID id = player.GetZDOID();

            if (!_playerAttachCache.TryGetValue(id, out List<GameObject> attaches))
                return;

            for (int i = attaches.Count - 1; i >= 0; i--)
            {
                // Clean up destroyed clones (e.g. weapon unequipped)
                if (attaches[i] == null)
                {
                    attaches.RemoveAt(i);
                    continue;
                }

                ApplyChargeVisualsToAttach(attaches[i].transform, charge, maxCharge);
            }
        }

        private static void ApplyChargeVisualsToAttach(Transform attach, float charge, float maxCharge)
        {
            MeshRenderer? renderer = attach.Find("scythe")?.GetComponent<MeshRenderer>();
            GameObject?   charged  = attach.Find("charged")?.gameObject;

            if (renderer == null || charged == null)
                return;

            charged.SetActive(charge >= maxCharge);

            renderer.GetPropertyBlock(PropBlock);
            PropBlock.SetColor(EmissionColorId, BaseEmissionColor * charge);
            renderer.SetPropertyBlock(PropBlock);
        }

        public static void SpawnApparition(Character target, GameObject prefab, string apparitionName)
        {
            if (prefab == null)
            {
                Jotunn.Logger.LogError("Apparition spawn prefab is null.");
                return;
            }

            SpawnAbility? spawnAbility = prefab.GetComponent<SpawnAbility>();
            if (spawnAbility == null)
            {
                Jotunn.Logger.LogError($"Apparition prefab '{prefab.name}' is missing a SpawnAbility component.");
                return;
            }

            Vector3 spawnPos = target.transform.position
                - target.transform.forward * 1.5f
                + Vector3.up * 0.5f;

            GameObject creature = Object.Instantiate(prefab, spawnPos, Quaternion.identity);

            SpawnAbility instanceAbility                      = creature.GetComponent<SpawnAbility>();
            instanceAbility.m_owner                          = Player.m_localPlayer;
            instanceAbility.m_commandOnSpawn                 = true;
            instanceAbility.m_alertSpawnedCreature           = false;
            instanceAbility.m_setMaxInstancesFromWeaponLevel = false;

            Game.instance.StartCoroutine(instanceAbility.Spawn());
            Jotunn.Logger.LogInfo($"Triggered SpawnAbility for '{apparitionName}' at {spawnPos}");
        }

        public static void ApplyShield(Player player)
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
            shield.m_levelUpSkillFactor        = 0.2f;
            shield.m_absorbDamagePerSkillLevel = 0.2f;

            Jotunn.Logger.LogInfo($"Shield applied — absorb: {shield.m_totalAbsorbDamage}, duration: {shield.m_ttl}s");
        }

        /// <summary>
        /// Returns true if the given weapon is equipped by the player,
        /// whether drawn or sheathed.
        /// </summary>
        public static bool IsWeaponEquipped(Player player, string weaponName)
        {
            if (player.GetInventory().GetEquippedItems()
                .Exists(item => item.m_shared.m_name == weaponName))
                return true;

            if (player.m_hiddenRightItem?.m_shared.m_name == weaponName)
                return true;

            if (player.m_hiddenLeftItem?.m_shared.m_name == weaponName)
                return true;

            return false;
        }

        /// <summary>Returns true if the player has any registered attach clones.</summary>
        public static bool HasCachedAttach(Player player)
        {
            ZDOID id = player.GetZDOID();
            return _playerAttachCache.TryGetValue(id, out List<GameObject> attaches)
                && attaches.Count > 0;
        }
    }
}