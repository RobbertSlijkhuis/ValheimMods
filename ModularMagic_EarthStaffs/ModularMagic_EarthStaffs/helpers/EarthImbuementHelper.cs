using ModularMagic_Core.Types;
using ModularMagic_EarthStaffs.Configs;
using ModularMagic_EarthStaffs.Models;
using System;
using System.Collections.Generic;
using UnityEngine;
using static ItemDrop;
using CoreImbuementHelper = ModularMagic_Core.Helpers.ImbuementHelper;
using Imbuement = ModularMagic_Core.Models.Imbuement;

namespace ModularMagic_EarthStaffs.Helpers
{
    /// <summary>
    /// Applies the imbuements of an Earth staff (read through the shared Core API) to its item data.
    /// What a rune does is specific to the staff type, so this stays in the staff mod.
    /// </summary>
    internal class EarthImbuementHelper
    {
        // The runes the Earth staffs support. The weapon decides this, the rune only says what it is
        public static readonly string[] AllowedRunes =
        {
            RuneId.DamagePierce, RuneId.DamageSlash, RuneId.EitrCost, RuneId.ProjectileAccuracy, RuneId.ProjectileBurst,
            RuneId.ProjectileSpeed, RuneId.Cone, RuneId.Creatures, RuneId.Nova, RuneId.Rain,
        };

        // Set when the game has started, the equip patch also runs while the player is being loaded
        public static bool GameIsReady = false;

        // Other staff mods share the imbuement API, so only handle the staffs of this mod
        public static bool IsEarthStaff(ItemData itemData)
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

                if (weapon == null || !CoreImbuementHelper.HasImbuements(weapon) || !IsEarthStaff(weapon))
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
            ApplyImbuements(itemData, imbuements);
        }

        public static void ApplyImbuements(ItemData itemData, List<Imbuement> imbuements)
        {
            UpdateItemDataOptions options = new UpdateItemDataOptions();
            ItemDataSnapShot snapshot = GetItemDataSnapshot(itemData.m_dropPrefab.name);

            if (snapshot == null)
                throw new Exception("Snapshot is null");

            options.damageBlunt = snapshot.damageBlunt;
            options.damageSlash = 0;
            options.damagePierce = 0;
            options.damageBluntPerLevel = snapshot.damageBluntPerlevel;
            options.damageSlashPerLevel = 0;
            options.damagePiercePerLevel = 0;
            options.attackEitr = snapshot.attackEitr;
            // options.timedBlockBonus = snapshot.timedBlockBonus;
            options.projectileAccuracy = snapshot.projectileAccuracy;
            options.projectileBurst = snapshot.projectileBurst;
            options.projectileVelocity = snapshot.projectileVelocity;
            options.mainAttack = snapshot.mainAttack;

            // The projectile that shows the damage type. Every damage type has its own prefab, so all clients spawn the same visual
            GameObject damageProjectile = null;

            foreach (Imbuement imbuement in imbuements)
            {
                switch (imbuement.type)
                {
                    case ImbuementType.DamageType:
                        if (imbuement.value == "Slash")
                        {
                            options.damageSlash = snapshot.damageBlunt;
                            options.damageSlashPerLevel = snapshot.damageBluntPerlevel;
                            damageProjectile = ModularMagic_EarthStaffs.prefabs.ProjectileSlash;
                        }
                        else if (imbuement.value == "Pierce")
                        {
                            options.damagePierce = snapshot.damageBlunt;
                            options.damagePiercePerLevel = snapshot.damageBluntPerlevel;
                            damageProjectile = ModularMagic_EarthStaffs.prefabs.ProjectilePierce;
                        }

                        options.damageBlunt = 0;
                        options.damageBluntPerLevel = 0;
                        break;
                    case ImbuementType.EitrCost:
                        options.attackEitr -= PluginConfig.imbuementConfig.EitrCost.Value * imbuement.level;
                        break;
                    //case ImbuementType.ParryBonus:
                    //    options.timedBlockBonus += float.Parse(imbuement.value, CultureInfo.InvariantCulture);
                    //    break;
                    case ImbuementType.ProjectileAccuracy:
                        options.projectileAccuracy -= PluginConfig.imbuementConfig.ProjectileAccuracy.Value * imbuement.level;
                        break;
                    case ImbuementType.ProjectileBurst:
                        options.projectileBurst -= PluginConfig.imbuementConfig.ProjectileBurst.Value * imbuement.level;
                        break;
                    case ImbuementType.ProjectileVelocity:
                        options.projectileVelocity += PluginConfig.imbuementConfig.ProjectileSpeed.Value * imbuement.level;
                        break;
                    case ImbuementType.MainAttack:
                        if (imbuement.value == "Cone")
                            options.mainAttack = ModularMagic_EarthStaffs.prefabs.mainAttackCone.GetComponent<ItemDrop>().m_itemData;
                        break;
                    case ImbuementType.SecondaryAttack:
                        if (imbuement.value == "Nova")
                            options.secondaryAttack = ModularMagic_EarthStaffs.prefabs.SecondaryAttackNova.GetComponent<ItemDrop>().m_itemData;
                        else if (imbuement.value == "Rain")
                            options.secondaryAttack = ModularMagic_EarthStaffs.prefabs.SecondaryAttackBoulder.GetComponent<ItemDrop>().m_itemData;
                        else if (imbuement.value == "Summon")
                            options.secondaryAttack = ModularMagic_EarthStaffs.prefabs.SecondaryAttackRoots.GetComponent<ItemDrop>().m_itemData;

                        // Every level above 1 saves a part of the eitr cost. The base cost is the one of the attack itself,
                        // which is read again on every equip, so a changed or synced config value is used
                        if (options.secondaryAttack != null)
                        {
                            float saved = Mathf.Clamp01(PluginConfig.imbuementConfig.SecondaryAttackEitr.Value * (imbuement.level - 1));
                            options.secondaryAttackEitr = options.secondaryAttack.m_shared.m_attack.m_attackEitr * (1f - saved);
                        }
                        break;
                }
            }

            // A rune can not make the eitr cost, the spread or the burst interval negative
            options.attackEitr = Mathf.Max(0f, options.attackEitr.Value);
            options.projectileAccuracy = Mathf.Max(0f, options.projectileAccuracy.Value);
            options.projectileBurst = Mathf.Max(0f, options.projectileBurst.Value);

            // The cone main attack has its own projectile, so the damage type projectile only replaces the default one
            if (damageProjectile != null && options.mainAttack == snapshot.mainAttack)
                options.attackProjectile = damageProjectile;

            Jotunn.Logger.LogWarning($"[Imbuements] {itemData.m_dropPrefab.name}: damage (B, P, S) {options.damageBlunt}, {options.damagePierce}, {options.damageSlash}, eitr {options.attackEitr}, accuracy {options.projectileAccuracy}, burst {options.projectileBurst}, speed {options.projectileVelocity}, projectile {(options.attackProjectile != null ? options.attackProjectile.name : "default")}, main {(options.mainAttack == null ? "null" : options.mainAttack.m_shared?.m_attack?.m_attackProjectile?.name)}, secondary {(options.secondaryAttack == null ? "null" : options.secondaryAttack.m_shared?.m_attack?.m_attackProjectile?.name)}, secondary eitr {(options.secondaryAttackEitr == null ? "default" : options.secondaryAttackEitr.ToString())}");

            StatusEffect ImbuementEffect = ScriptableObject.CreateInstance<StatusEffect>();
            List<string> tooltipLines = new List<string>();

            foreach (Imbuement imbuement in imbuements)
            {
                if (imbuement.type == ImbuementType.None)
                    continue;

                tooltipLines.Add($"<color=green>{imbuement.name} {(imbuement.level > 0 ? imbuement.level : "")} </color>");
            }

            ImbuementEffect.m_tooltip = string.Join("\n", tooltipLines);
            ImbuementEffect.name = "Imbuements_MMES";
            ImbuementEffect.m_name = "Imbuements";
            options.equipStatusEffect = ImbuementEffect;

            StatusEffect previousEquipEffect = itemData.m_shared.m_equipStatusEffect;

            UpdateHelper.UpdateItemData(itemData, options);

            DestroyReplacedEquipStatusEffect(previousEquipEffect, itemData);
        }

        // Every apply makes a new effect for the tooltip, so the one of the previous apply is destroyed. Only an effect of this mod is destroyed.
        // The player keeps a copy of the effect it runs, and the game only reads the name hash of the old one, which it cached when the effect was added
        private static void DestroyReplacedEquipStatusEffect(StatusEffect previous, ItemData itemData)
        {
            if (previous == null || previous.name != "Imbuements_MMES" || ReferenceEquals(previous, itemData.m_shared.m_equipStatusEffect))
                return;

            UnityEngine.Object.Destroy(previous);
        }

        private static ItemDataSnapShot GetItemDataSnapshot(string name)
        {
            switch (name)
            {
                case var value when value == ModularMagic_EarthStaffs.prefabs.StaffEarth0.name:
                    return ModularMagic_EarthStaffs.snapshots.staffEarth0;
                case var value when value == ModularMagic_EarthStaffs.prefabs.StaffEarth1.name:
                    return ModularMagic_EarthStaffs.snapshots.staffEarth1;
                case var value when value == ModularMagic_EarthStaffs.prefabs.StaffEarth2.name:
                    return ModularMagic_EarthStaffs.snapshots.staffEarth2;
                case var value when value == ModularMagic_EarthStaffs.prefabs.StaffEarth3.name:
                    return ModularMagic_EarthStaffs.snapshots.staffEarth3;
                default:
                    return null;
            }
        }
    }
}
