using Jotunn.Managers;
using ModularMagic_BloodMagic.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModularMagic_BloodMagic.Helpers
{
    internal static class SoulReaperHelper
    {
        private static readonly MaterialPropertyBlock PropBlock        = new MaterialPropertyBlock();
        private static readonly int                   EmissionColorId = Shader.PropertyToID("_EmissionColor");

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
        public static void ApplyChargeVisualsForPlayer(Player player, float charge, float maxCharge, Color emissionColor)
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

                ApplyChargeVisualsToAttach(attaches[i].transform, charge, maxCharge, emissionColor);
            }
        }

        private static void ApplyChargeVisualsToAttach(Transform attach, float charge, float maxCharge, Color emissionColor)
        {
            MeshRenderer? renderer = attach.Find("scythe")?.GetComponent<MeshRenderer>();
            GameObject?   charged  = attach.Find("charged")?.gameObject;

            if (renderer == null || charged == null)
                return;

            float t = maxCharge > 0f ? Mathf.Clamp01(charge / maxCharge) : 0f;

            charged.SetActive(charge >= maxCharge);

            renderer.GetPropertyBlock(PropBlock);
            PropBlock.SetColor(EmissionColorId, emissionColor * charge);
            renderer.SetPropertyBlock(PropBlock);

            // Embers particle emission — scale rate 1→7 with charge
            Transform? embersTransform = attach.Find("equiped/embers");
            if (embersTransform != null)
            {
                ParticleSystem? embers = embersTransform.GetComponent<ParticleSystem>();
                if (embers != null)
                {
                    ParticleSystem.EmissionModule emission = embers.emission;
                    emission.rateOverTime     = t > 0f ? t * 7f : 1f;
                    emission.rateOverDistance = t > 0f ? t * 7f : 1f;
                }
            }
        }

        public static void SpawnApparition(Character target, GameObject prefab, string weaponName)
        {
            if (prefab == null)
            {
                Jotunn.Logger.LogError("Apparition spawn prefab is null.");
                return;
            }

            SpawnAbility? spawnAbility = prefab.GetComponent<SpawnAbility>();
            if (spawnAbility == null || spawnAbility.m_spawnPrefab == null || spawnAbility.m_spawnPrefab.Length == 0)
            {
                Jotunn.Logger.LogError($"Apparition prefab '{prefab.name}' is missing SpawnAbility or spawn prefabs.");
                return;
            }

            Vector3 spawnPos = target.transform.position
                - target.transform.forward * 1.5f
                + Vector3.up * 0.5f;

            Game.instance.StartCoroutine(SpawnApparitionCoroutine(spawnAbility, spawnPos, weaponName));
        }

        private static IEnumerator SpawnApparitionCoroutine(SpawnAbility spawnAbility, Vector3 spawnPos, string weaponName)
        {
            if (spawnAbility.m_initialSpawnDelay > 0f)
                yield return new WaitForSeconds(spawnAbility.m_initialSpawnDelay);

            spawnAbility.m_preSpawnEffects.Create(spawnPos, Quaternion.identity);

            if (spawnAbility.m_preSpawnDelay > 0f)
                yield return new WaitForSeconds(spawnAbility.m_preSpawnDelay);

            Vector3 finalPos = spawnPos;
            if (spawnAbility.m_snapToTerrain)
            {
                ZoneSystem.instance.GetSolidHeight(spawnPos, out float height, spawnAbility.m_getSolidHeightMargin);
                finalPos.y = height + spawnAbility.m_spawnGroundOffset;
            }

            GameObject creaturePrefab = spawnAbility.m_spawnPrefab[Random.Range(0, spawnAbility.m_spawnPrefab.Length)];

            if (spawnAbility.m_maxSpawned > 0 && SpawnSystem.GetNrOfInstances(creaturePrefab) >= spawnAbility.m_maxSpawned)
            {
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, spawnAbility.m_maxSummonReached);
                yield break;
            }

            GameObject creature = Object.Instantiate(
                creaturePrefab,
                finalPos,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));

            // Delegate all per-weapon setup — including follower and drops — to ApparitionController
            creature.GetComponent<ApparitionController>()?.SetData(weaponName);

            spawnAbility.m_spawnEffects.Create(finalPos, Quaternion.identity);

            if (spawnAbility.m_wakeUpAnimation)
                creature.GetComponent<ZSyncAnimation>()?.SetBool("wakeup", value: true);

            Jotunn.Logger.LogInfo($"SpawnApparitionCoroutine complete — spawned '{creaturePrefab.name}' at {finalPos}");
        }

        /// <summary>
        /// Returns the SoulReaper for the scythe the local player currently has drawn,
        /// or null if no scythe is actively drawn. Also outputs the weapon name.
        /// </summary>
        public static SoulReaper? GetDrawnReaper(Dictionary<string, SoulReaper> reapers, out string? weaponName)
        {
            weaponName = null;

            if (Player.m_localPlayer == null)
                return null;

            ItemDrop.ItemData? drawnWeapon = Player.m_localPlayer.GetCurrentWeapon();
            if (drawnWeapon == null)
                return null;

            foreach (KeyValuePair<string, SoulReaper> pair in reapers)
            {
                if (drawnWeapon.m_shared.m_name == pair.Key)
                {
                    weaponName = pair.Key;
                    return pair.Value;
                }
            }

            return null;
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

        public static void SpawnChargeEffects(Vector3 point, Vector3 dir)
        {
            GameObject vfxInstance = Object.Instantiate(
                ModularMagic_BloodMagic.Instance.prefabs.SpikeEffect,
                point,
                Quaternion.LookRotation(-dir));
            vfxInstance.transform.localScale = Vector3.one * 1.4f;

            GameObject vfxInstance2 = Object.Instantiate(
                ModularMagic_BloodMagic.Instance.prefabs.SporeEffect,
                point,
                Quaternion.LookRotation(-dir));
            vfxInstance2.transform.localScale = Vector3.one * 1.4f;

            GameObject sfxPrefab = PrefabManager.Instance.GetPrefab("sfx_staff_elder_cast");
            if (sfxPrefab != null)
                Object.Instantiate(sfxPrefab, point, Quaternion.LookRotation(-dir));
        }
    }
}