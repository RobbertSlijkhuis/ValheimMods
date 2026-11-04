using ModularMagic_Core.Types;
using ModularMagic_LightningStaffs.Configs;
using ModularMagic_LightningStaffs.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;
using Imbuement = ModularMagic_Core.Models.Imbuement;

namespace ModularMagic_LightningStaffs.Helpers
{
    /// <summary>
    /// Applies the imbuements of a Lightning staff (read through the shared Core API) to its item data.
    /// What a rune does is specific to the staff type, so this stays in the staff mod.
    /// </summary>
    internal class LightningImbuementHelper
    {
        // The runes the Lightning staffs support. The weapon decides this, the rune only says what it is
        public static readonly string[] AllowedRunes =
        {
            RuneId.EitrCost, RuneId.ProjectileAccuracy, RuneId.ProjectileBurst, RuneId.ProjectileSpeed,
            RuneId.Nova, RuneId.Rain, RuneId.Cone,
        };

        // Set when the game has started, the equip patch also runs while the player is being loaded
        public static bool GameIsReady = false;

        // Other staff mods share the imbuement API, so only handle the imbuable staffs of this mod
        public static bool IsLightningStaff(ItemData itemData)
        {
            return itemData != null && itemData.m_dropPrefab != null && GetItemDataSnapshot(itemData.m_dropPrefab.name) != null;
        }

        // Applies the runes again on the staff in the hand, so a changed config value reaches it together with the runes
        public static void ReapplyOnHeldStaff()
        {
            try
            {
                Player player = Player.m_localPlayer;

                if (player == null || !GameIsReady)
                    return;

                ItemData weapon = player.GetCurrentWeapon();

                if (weapon == null || !CoreImbuementHelper.HasImbuements(weapon) || !IsLightningStaff(weapon))
                    return;

                ApplyImbuements(weapon);
            }
            catch (Exception e)
            {
                Jotunn.Logger.LogError("Could not apply the imbuements to the staff in the hand: " + e);
            }
        }

        public static void ApplyImbuements(ItemData itemData)
        {
            List<Imbuement> imbuements = CoreImbuementHelper.Read(itemData);

            ItemDataSnapShot snapshot = GetItemDataSnapshot(itemData.m_dropPrefab.name);

            if (snapshot == null)
                throw new Exception("Snapshot is null");

            // The attack of the prefab is copied for the staff, the base stats are read from the config below
            ItemData baseData = snapshot.mainAttack;

            // An item made with Inventory.AddItem(prefab, amount) shares its shared data with the prefab. Changing it would
            // put the runes (for example the secondary attack) on every new staff of this type, so leave such an item as it is
            if (ReferenceEquals(itemData.m_shared, baseData.m_shared))
            {
                Jotunn.Logger.LogWarning($"[Imbuements] {itemData.m_dropPrefab.name} shares its data with the prefab, the runes are not applied");
                return;
            }

            // The cone rune replaces the main attack of the staff. The cone has its own eitr cost per burst and fires an AOE, so the
            // eitr, accuracy and speed of the staff do not apply to it (the Eitr Cost rune still lowers the eitr of the cone)
            bool cone = imbuements.Exists(i => i.type == ImbuementType.MainAttack && i.value == "Cone");
            ItemData mainAttack = cone ? ModularMagic_LightningStaffs.Instance.prefabs.mainAttackCone.GetComponent<ItemDrop>().m_itemData : baseData;

            // The base stats come from the config, which also holds the values a server syncs, so the runes are always applied on top of the current base stats
            float eitr = cone ? PluginConfig.mainAttackCone.useEitr.Value : snapshot.attackEitr;
            float accuracy = snapshot.projectileAccuracy;
            float velocity = snapshot.projectileVelocity;

            // Only the cone fires bursts, the Attack Speed rune lowers the time between its bursts. Without the cone the rune has no effect
            float burst = cone ? PluginConfig.mainAttackCone.burstInterval.Value : 0f;

            // The secondary attack (nova or lightning bolts), only when the staff has the rune
            ItemData secondaryAttack = null;
            float? secondaryEitr = null;

            foreach (Imbuement imbuement in imbuements)
            {
                switch (imbuement.type)
                {
                    case ImbuementType.EitrCost:
                        eitr -= PluginConfig.imbuementConfig.EitrCost.Value * imbuement.level;
                        break;
                    case ImbuementType.ProjectileAccuracy:
                        accuracy -= PluginConfig.imbuementConfig.ProjectileAccuracy.Value * imbuement.level;
                        break;
                    case ImbuementType.ProjectileBurst:
                        burst -= PluginConfig.imbuementConfig.ProjectileBurst.Value * imbuement.level;
                        break;
                    case ImbuementType.ProjectileVelocity:
                        velocity += PluginConfig.imbuementConfig.ProjectileSpeed.Value * imbuement.level;
                        break;
                    case ImbuementType.SecondaryAttack:
                        if (imbuement.value == "Nova")
                            secondaryAttack = ModularMagic_LightningStaffs.Instance.prefabs.secondaryAttackNova.GetComponent<ItemDrop>().m_itemData;
                        else if (imbuement.value == "Rain")
                            secondaryAttack = ModularMagic_LightningStaffs.Instance.prefabs.secondaryAttackRain.GetComponent<ItemDrop>().m_itemData;

                        // Every level above 1 saves a part of the eitr cost. The base cost is the one of the attack itself,
                        // which is read again on every equip, so a changed or synced config value is used
                        if (secondaryAttack != null)
                        {
                            float saved = Mathf.Clamp01(PluginConfig.imbuementConfig.SecondaryAttackEitr.Value * (imbuement.level - 1));
                            secondaryEitr = secondaryAttack.m_shared.m_attack.m_attackEitr * (1f - saved);
                        }
                        break;
                }
            }

            UpdateItemDataOptions options = new UpdateItemDataOptions()
            {
                mainAttack = mainAttack,
                attackEitr = Mathf.Max(0f, eitr),
                projectileAccuracy = cone ? (float?)null : Mathf.Max(0f, accuracy),
                projectileVelocity = cone ? (float?)null : velocity,
                burstInterval = cone ? (float?)Mathf.Max(0f, burst) : null,
                secondaryAttack = secondaryAttack,
                secondaryAttackEitr = secondaryEitr,
                equipStatusEffect = CreateEquipStatusEffect(imbuements),
            };

            Jotunn.Logger.LogWarning($"[Imbuements] {itemData.m_dropPrefab.name}: main {(cone ? "cone" : "default")}, eitr {options.attackEitr}, accuracy {options.projectileAccuracy}, burst {(options.burstInterval == null ? "default" : options.burstInterval.ToString())}, speed {options.projectileVelocity}, secondary eitr {(options.secondaryAttackEitr == null ? "none" : options.secondaryAttackEitr.ToString())}");

            StatusEffect previousEquipEffect = itemData.m_shared.m_equipStatusEffect;

            UpdateHelper.UpdateItemData(itemData, options);

            DestroyReplacedEquipStatusEffect(previousEquipEffect, itemData);
        }

        // Every apply makes a new effect for the tooltip, so the one of the previous apply is destroyed. Only an effect of this mod is destroyed.
        // The player keeps a copy of the effect it runs, and the game only reads the name hash of the old one, which it cached when the effect was added
        private static void DestroyReplacedEquipStatusEffect(StatusEffect previous, ItemData itemData)
        {
            if (previous == null || previous.name != "Imbuements_MMLS" || ReferenceEquals(previous, itemData.m_shared.m_equipStatusEffect))
                return;

            UnityEngine.Object.Destroy(previous);
        }

        private static ItemDataSnapShot GetItemDataSnapshot(string name)
        {
            CustomPrefabs prefabs = ModularMagic_LightningStaffs.Instance.prefabs;
            ItemDataSnapshots snapshots = ModularMagic_LightningStaffs.Instance.snapshots;

            if (name == prefabs.staffLightning1Prefab.name)
                return snapshots.staffLightning1;

            if (name == prefabs.staffLightning2Prefab.name)
                return snapshots.staffLightning2;

            if (name == prefabs.staffLightning3Prefab.name)
                return snapshots.staffLightning3;

            return null;
        }

        // The tooltip of the staff lists the runes
        private static StatusEffect CreateEquipStatusEffect(List<Imbuement> imbuements)
        {
            List<string> tooltipLines = new List<string>();

            foreach (Imbuement imbuement in imbuements)
            {
                if (imbuement.type == ImbuementType.None)
                    continue;

                tooltipLines.Add($"<color=green>{imbuement.name} {(imbuement.level > 0 ? imbuement.level : "")} </color>");
            }

            StatusEffect effect = ScriptableObject.CreateInstance<StatusEffect>();
            effect.name = "Imbuements_MMLS";
            effect.m_name = "Imbuements";
            effect.m_tooltip = string.Join("\n", tooltipLines);

            return effect;
        }
    }
}
