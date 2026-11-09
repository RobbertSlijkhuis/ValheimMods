using ModularMagic_Core.Types;
using ModularMagic_IceStaffs.configs;
using ModularMagic_IceStaffs.Configs;
using ModularMagic_IceStaffs.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;
using Imbuement = ModularMagic_Core.Models.Imbuement;

namespace ModularMagic_IceStaffs.Helpers
{
    /// <summary>
    /// Applies the imbuements of an Ice staff (read through the shared Core API) to its item data.
    /// What a rune does is specific to the staff type, so this stays in the staff mod.
    /// </summary>
    internal class IceImbuementHelper
    {
        // The runes the Ice staffs support. The weapon decides this, the rune only says what it is
        public static readonly string[] AllowedRunes =
        {
            RuneId.DamageBlunt, RuneId.DamageSlash, RuneId.EitrCost,
            RuneId.ProjectileAccuracy, RuneId.AttackSpeed, RuneId.ProjectileSpeed,
            RuneId.Nova, RuneId.Rain, RuneId.Cone,
        };

        // Set when the game has started, the equip patch also runs while the player is being loaded
        public static bool GameIsReady = false;

        // Other staff mods share the imbuement API, so only handle the imbuable staffs of this mod (not the Skull of Frost)
        public static bool IsIceStaff(ItemData itemData)
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

                if (weapon == null || !CoreImbuementHelper.HasImbuements(weapon) || !IsIceStaff(weapon))
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

            // The cone rune replaces the main attack of the staff. The cone costs a part of the eitr of the staff per burst, has its own burst interval
            // and fires an AOE, so the accuracy and speed of the staff do not apply to it (the Eitr Cost rune still lowers the eitr of the cone,
            // the Attack Speed rune is ignored)
            bool cone = imbuements.Exists(i => i.type == ImbuementType.MainAttack && i.value == "Cone");
            ItemData mainAttack = cone ? ModularMagic_IceStaffs.Instance.prefabs.MainAttackCone.GetComponent<ItemDrop>().m_itemData : baseData;

            // The base stats come from the config, which also holds the values a server syncs, so the runes are always applied on top of the current base stats
            float eitr = cone ? snapshot.attackEitr * PluginConfig.mainAttackCone.eitrMultiplier.Value : snapshot.attackEitr;
            float accuracy = snapshot.projectileAccuracy;
            float burst = cone ? PluginConfig.mainAttackCone.burstInterval.Value : snapshot.projectileBurst;
            float velocity = snapshot.projectileVelocity;

            // The runes change the type of the physical damage, the frost damage is never touched
            float physical = snapshot.damagePierce;
            float physicalPerLevel = snapshot.damagePiercePerLevel;
            string physicalType = "Pierce";

            // The secondary attack (nova), only when the staff has the rune
            ItemData secondaryAttack = null;
            float? secondaryEitr = null;
            bool rain = false;

            foreach (Imbuement imbuement in imbuements)
            {
                switch (imbuement.type)
                {
                    case ImbuementType.DamageType:
                        if (imbuement.value == "Blunt" || imbuement.value == "Slash")
                            physicalType = imbuement.value;
                        break;
                    case ImbuementType.EitrCost:
                        eitr -= PluginConfig.imbuementConfig.EitrCost.Value * imbuement.level;
                        break;
                    case ImbuementType.ProjectileAccuracy:
                        accuracy -= PluginConfig.imbuementConfig.ProjectileAccuracy.Value * imbuement.level;
                        break;
                    case ImbuementType.AttackSpeed:
                        // The cone is timed by its own burst interval, so the rune only applies to the normal attack
                        if (!cone)
                            burst -= PluginConfig.imbuementConfig.AttackSpeed.Value * imbuement.level;
                        break;
                    case ImbuementType.ProjectileVelocity:
                        velocity += PluginConfig.imbuementConfig.ProjectileSpeed.Value * imbuement.level;
                        break;
                    case ImbuementType.SecondaryAttack:
                        if (imbuement.value == "Nova")
                            secondaryAttack = ModularMagic_IceStaffs.Instance.prefabs.SecondaryAttackNova.GetComponent<ItemDrop>().m_itemData;
                        else if (imbuement.value == "Rain")
                            secondaryAttack = ModularMagic_IceStaffs.Instance.prefabs.SecondaryAttackRain.GetComponent<ItemDrop>().m_itemData;

                        // Every level above 1 saves a part of the eitr cost. The base cost is a multiplier of the eitr of the staff,
                        // read again on every equip, so a changed or synced config value is used
                        if (secondaryAttack != null)
                        {
                            SecondaryAttackConfig secondaryConfig = imbuement.value == "Nova" ? PluginConfig.secondaryAttackNova : PluginConfig.secondaryAttackRain;
                            float saved = Mathf.Clamp01(PluginConfig.imbuementConfig.SecondaryAttackEitr.Value * (imbuement.level - 1));
                            secondaryEitr = snapshot.attackEitr * secondaryConfig.eitrMultiplier.Value * (1f - saved);
                            rain = imbuement.value == "Rain";
                        }
                        break;
                }
            }

            UpdateItemDataOptions options = new UpdateItemDataOptions()
            {
                mainAttack = mainAttack,
                attackEitr = Mathf.Max(0f, eitr),
                projectileAccuracy = cone ? (float?)null : Mathf.Max(0f, accuracy),
                projectileBurst = Mathf.Max(0f, burst),
                projectileVelocity = cone ? (float?)null : velocity,
                damagePierce = physicalType == "Pierce" ? physical : 0f,
                damagePiercePerLevel = physicalType == "Pierce" ? physicalPerLevel : 0f,
                damageBlunt = physicalType == "Blunt" ? physical : 0f,
                damageBluntPerLevel = physicalType == "Blunt" ? physicalPerLevel : 0f,
                damageSlash = physicalType == "Slash" ? physical : 0f,
                damageSlashPerLevel = physicalType == "Slash" ? physicalPerLevel : 0f,
                secondaryAttack = secondaryAttack,
                secondaryAttackEitr = secondaryEitr,
                equipStatusEffect = CreateEquipStatusEffect(imbuements),
            };

            Jotunn.Logger.LogWarning($"[Imbuements] {itemData.m_dropPrefab.name}: main {(cone ? "cone" : "default")}, {physicalType} {physical}, eitr {options.attackEitr}, accuracy {options.projectileAccuracy}, burst {options.projectileBurst}, speed {options.projectileVelocity}, secondary eitr {(options.secondaryAttackEitr == null ? "none" : options.secondaryAttackEitr.ToString())}");

            StatusEffect previousEquipEffect = itemData.m_shared.m_equipStatusEffect;

            UpdateHelper.UpdateItemData(itemData, options);

            // After the staff is updated, so the damage types of the damage type rune are used
            if (rain)
                ApplyShardDamage(itemData);

            DestroyReplacedEquipStatusEffect(previousEquipEffect, itemData);
        }

        // The script that spawns the shards does not pass the damage of the staff on, so the frost and physical damage of the shard prefab is set from the staff
        // (quality included) times the damage multiplier, and the attack force from the attack force of the staff times the force multiplier.
        // The shards are hit by the client of the shooter, and the chop and pickaxe damage stay config values
        private static void ApplyShardDamage(ItemData staff)
        {
            HitData.DamageTypes damage = staff.GetDamage();
            SecondaryAttackConfig rain = PluginConfig.secondaryAttackRain;
            Projectile shard = ModularMagic_IceStaffs.Instance.prefabs.ProjectileIceShard.GetComponent<Projectile>();
            float multiplier = rain.damageMultiplier.Value;

            shard.m_damage.m_frost = damage.m_frost * multiplier;
            shard.m_damage.m_pierce = damage.m_pierce * multiplier;
            shard.m_damage.m_blunt = damage.m_blunt * multiplier;
            shard.m_damage.m_slash = damage.m_slash * multiplier;
            shard.m_attackForce = staff.m_shared.m_attackForce * rain.forceMultiplier.Value;
        }

        // Every apply makes a new effect for the tooltip, so the one of the previous apply is destroyed. Only an effect of this mod is destroyed.
        // The player keeps a copy of the effect it runs, and the game only reads the name hash of the old one, which it cached when the effect was added
        private static void DestroyReplacedEquipStatusEffect(StatusEffect previous, ItemData itemData)
        {
            if (previous == null || previous.name != "Imbuements_MMIS" || ReferenceEquals(previous, itemData.m_shared.m_equipStatusEffect))
                return;

            UnityEngine.Object.Destroy(previous);
        }

        private static ItemDataSnapShot GetItemDataSnapshot(string name)
        {
            CustomPrefabs prefabs = ModularMagic_IceStaffs.Instance.prefabs;
            ItemDataSnapshots snapshots = ModularMagic_IceStaffs.Instance.snapshots;

            if (name == prefabs.StaffIce1Prefab.name)
                return snapshots.staffIce1;

            if (name == prefabs.StaffIce2Prefab.name)
                return snapshots.staffIce2;

            if (name == prefabs.StaffIce3Prefab.name)
                return snapshots.staffIce3;

            return null;
        }

        // The tooltip of the staff lists the runes
        private static StatusEffect CreateEquipStatusEffect(List<Imbuement> imbuements)
        {
            List<string> tooltipLines = new List<string>();
            bool cone = imbuements.Exists(i => i.type == ImbuementType.MainAttack && i.value == "Cone");

            foreach (Imbuement imbuement in imbuements)
            {
                if (imbuement.type == ImbuementType.None)
                    continue;

                // The cone is timed by its own burst interval, so the Attack Speed rune does nothing while it is slotted
                if (cone && imbuement.type == ImbuementType.AttackSpeed)
                {
                    tooltipLines.Add($"<color=red>{imbuement.name} {(imbuement.level > 0 ? imbuement.level : "")} (no effect with the Cone rune)</color>");
                    continue;
                }

                tooltipLines.Add($"<color=green>{imbuement.name} {(imbuement.level > 0 ? imbuement.level : "")} </color>");
            }

            StatusEffect effect = ScriptableObject.CreateInstance<StatusEffect>();
            effect.name = "Imbuements_MMIS";
            effect.m_name = "Imbuements";
            effect.m_tooltip = string.Join("\n", tooltipLines);

            return effect;
        }
    }
}
