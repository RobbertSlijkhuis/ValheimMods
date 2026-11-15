using Jotunn.Managers;
using ModularMagic_BloodMagic.Components;
using ModularMagic_BloodMagic.Configs;
using ModularMagic_BloodMagic.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Imbuement = ModularMagic_Core.Models.Imbuement;
using ImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;
using ImbuementType = ModularMagic_Core.Types.ImbuementType;

namespace ModularMagic_BloodMagic.Helpers
{
    internal static class SoulReaperHelper
    {
        private static readonly MaterialPropertyBlock PropBlock        = new MaterialPropertyBlock();
        private static readonly int                   EmissionColorId = Shader.PropertyToID("_EmissionColor");

        // Keyed by player ZDOID — stores all scythe attach clones for each player
        private static readonly Dictionary<ZDOID, List<GameObject>> _playerAttachCache
            = new Dictionary<ZDOID, List<GameObject>>();

        // One SoulReaper per scythe, keyed by the prefab name of the scythe
        private static readonly Dictionary<string, SoulReaper> _reapers
            = new Dictionary<string, SoulReaper>();

        /// <summary>
        /// Creates the SoulReaper of every scythe. Called when the game starts (Game.Awake), the reapers live on the game object.
        /// </summary>
        public static void RegisterReapers()
        {
            CustomPrefabs prefabs = ModularMagic_BloodMagic.Instance.prefabs;

            // A scythe is identified by its prefab name, the display name can be changed in the config
            RegisterReaper(prefabs.Scythe1.name, "SoulReaperCharge_1", prefabs.SpawnAbilityScythe1, new Color(0f, 2.666667f, 2.996078f));
            RegisterReaper(prefabs.Scythe2.name, "SoulReaperCharge_2", prefabs.SpawnAbilityScythe2, new Color(2.839216f, 0f, 4f));
            RegisterReaper(prefabs.Scythe3.name, "SoulReaperCharge_3", prefabs.SpawnAbilityScythe3, new Color(4f, 0.03725543f, 0f));
        }

        private static void RegisterReaper(string weaponName, string zdoKeyName, GameObject apparitionPrefab, Color emissionColor)
        {
            SoulReaper reaper = Game.instance.gameObject.AddComponent<SoulReaper>();
            reaper.Init(zdoKeyName);
            reaper.m_apparitionPrefab = apparitionPrefab;
            reaper.m_emissionColor = emissionColor;
            _reapers[weaponName] = reaper;
        }

        /// <summary>
        /// Called for every hit the local player deals. A hit with a drawn scythe charges it, and a full scythe summons an apparition
        /// behind the target when the player has a free slot. Without a free slot the scythe stays full until one opens.
        /// </summary>
        public static void OnLocalPlayerHit(Character target, HitData hit)
        {
            Player player = Player.m_localPlayer;

            if (player == null || target == null || hit == null)
                return;

            Character attacker = hit.GetAttacker();
            if (attacker == null || attacker != player)
                return;

            // Only an enemy charges the scythe, not your own apparitions or a tame (they can be hit with PvP on)
            if (target.IsTamed())
                return;

            SoulReaper? reaper = GetDrawnReaper(out _);
            ItemDrop.ItemData? scythe = player.GetCurrentWeapon();

            if (reaper == null || scythe == null)
                return;

            int slots = ApparitionHelper.CountLocalPlayerSlots();

            if (reaper.CanDischarge())
            {
                int max = ApparitionHelper.GetMaxApparitions();

                if (slots >= max)
                {
                    Jotunn.Logger.LogWarning($"[SoulReaper] Full, but all {max} apparition slots are taken ({slots})");
                    return;
                }

                reaper.ResetCharge();
                Jotunn.Logger.LogWarning($"[SoulReaper] Discharged, summoning apparition {slots + 1} of {max}");
                SpawnApparition(target, reaper.m_apparitionPrefab, scythe);
                return;
            }

            // Full, in the moment before it can discharge: there is nothing to add, so the hit costs no eitr
            if (reaper.m_charge >= reaper.m_maxCharge)
                return;

            float eitr = GetChargeEitr(scythe);

            if (!player.HaveEitr(eitr))
            {
                Jotunn.Logger.LogWarning($"[SoulReaper] Cannot charge, the hit needs {eitr:F2} eitr");
                return;
            }

            float charge = GetChargePerHit(slots);

            player.UseEitr(eitr);
            reaper.Charge(charge);
            Jotunn.Logger.LogWarning($"[SoulReaper] Charged {charge:P1} for {eitr:F2} eitr with {slots} apparitions, now {reaper.m_charge:P1}");
            SpawnChargeEffects(hit.m_point, hit.m_dir);
        }

        /// <summary>
        /// Fills the scythe at once, for the Blood Tap rune (BloodTapHelper.Tap). Like a full scythe from hits, the next hit summons an
        /// apparition when there is a free slot.
        /// </summary>
        public static void FillCharge(SoulReaper reaper, Vector3 point, Vector3 dir)
        {
            reaper.Charge(reaper.m_maxCharge);
            Jotunn.Logger.LogWarning($"[SoulReaper] Filled by Blood Tap, now {reaper.m_charge:P1}");
            SpawnChargeEffects(point, dir);
        }

        /// <summary>
        /// The part of a full charge a hit adds. A scythe charges faster while the player has few apparitions: without any it is the
        /// config multiplier, and the bonus gets smaller with every apparition until it is gone at the config number of apparitions.
        /// </summary>
        private static float GetChargePerHit(int apparitions)
        {
            float normalAt = Mathf.Max(1, PluginConfig.apparitionsForNormalCharge.Value);
            float free = Mathf.Clamp01((normalAt - apparitions) / normalAt);

            return PluginConfig.chargePerHit.Value * (1f + (PluginConfig.chargeMultiplierWithoutApparitions.Value - 1f) * free);
        }

        /// <summary>
        /// The eitr a hit needs and uses to charge the scythe: the config value, lowered by the Eitr Cost rune of the scythe.
        /// </summary>
        private static float GetChargeEitr(ItemDrop.ItemData scythe)
        {
            float eitr = PluginConfig.chargeEitr.Value;

            foreach (Imbuement imbuement in ImbuementHelper.Read(scythe))
            {
                if (imbuement.type == ImbuementType.EitrCost)
                    eitr -= PluginConfig.imbuementConfig.EitrCost.Value * imbuement.level;
            }

            return Mathf.Max(0f, eitr);
        }

        /// <summary>
        /// Shows the charge of the scythe a player has equipped on the scythe, read from the ZDO of the player so every client sees it.
        /// </summary>
        public static void UpdateChargeVisuals(Player player)
        {
            if (player == null || player.m_nview == null || !player.m_nview.IsValid())
                return;

            foreach (KeyValuePair<string, SoulReaper> pair in _reapers)
            {
                bool hasWeapon = player == Player.m_localPlayer
                    ? IsWeaponEquipped(player, pair.Key)
                    : HasCachedAttach(player);

                if (!hasWeapon)
                    continue;

                float charge = player.m_nview.GetZDO().GetFloat(pair.Value.ZDOKey, 0f);
                ApplyChargeVisualsForPlayer(player, charge, pair.Value.m_maxCharge, pair.Value.m_emissionColor);
                break; // Only one scythe can be equipped at a time
            }
        }

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

        /// <summary>
        /// Spawns an apparition behind the target with the SpawnAbility of the scythe, like the game does for a summon. The game handles the
        /// spawn, then ApparitionHelper sets up the apparition from the scythe.
        /// </summary>
        public static void SpawnApparition(Character target, GameObject prefab, ItemDrop.ItemData scythe)
        {
            if (prefab == null)
            {
                Jotunn.Logger.LogError("Apparition spawn prefab is null.");
                return;
            }

            if (prefab.GetComponent<SpawnAbility>() == null)
            {
                Jotunn.Logger.LogError($"Apparition prefab '{prefab.name}' is missing SpawnAbility.");
                return;
            }

            Vector3 spawnPos = target.transform.position
                - target.transform.forward * 1.5f
                + Vector3.up * 0.5f;

            // The same as an Attack does with the prefab of a spawn ability: the spawn ability starts when it gets its owner and weapon
            GameObject spawnAbility = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
            SpawnAbility ability = spawnAbility.GetComponent<SpawnAbility>();

            // The game spawns the apparition after a delay. Until then the summon takes a slot, so a second summon can not go over the
            // maximum. It is added before the spawn ability starts, a spawn ability without a delay spawns right away
            ApparitionHelper.AddPendingSummon(ability);
            ability.Setup(Player.m_localPlayer, Vector3.zero, 0f, null, scythe, null);
        }

        /// <summary>
        /// Returns the SoulReaper for the scythe the local player currently has drawn,
        /// or null if no scythe is actively drawn. Also outputs the weapon name.
        /// </summary>
        public static SoulReaper? GetDrawnReaper(out string? weaponName)
        {
            weaponName = null;

            if (Player.m_localPlayer == null)
                return null;

            ItemDrop.ItemData? drawnWeapon = Player.m_localPlayer.GetCurrentWeapon();
            if (drawnWeapon == null)
                return null;

            foreach (KeyValuePair<string, SoulReaper> pair in _reapers)
            {
                if (GetPrefabName(drawnWeapon) == pair.Key)
                {
                    weaponName = pair.Key;
                    return pair.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// The prefab name of a scythe. A scythe is identified by it and not by its display name, which can be changed in the config.
        /// </summary>
        private static string? GetPrefabName(ItemDrop.ItemData? item)
        {
            return item?.m_dropPrefab != null ? item.m_dropPrefab.name : null;
        }

        /// <summary>
        /// Returns true if the weapon with the given prefab name is equipped by the player,
        /// whether drawn or sheathed.
        /// </summary>
        public static bool IsWeaponEquipped(Player player, string weaponName)
        {
            if (player.GetInventory().GetEquippedItems()
                .Exists(item => GetPrefabName(item) == weaponName))
                return true;

            if (GetPrefabName(player.m_hiddenRightItem) == weaponName)
                return true;

            if (GetPrefabName(player.m_hiddenLeftItem) == weaponName)
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