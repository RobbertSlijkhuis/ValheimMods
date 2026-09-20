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
        // Other staff mods share the imbuement API, so only handle the staffs of this mod
        public static bool IsEarthStaff(ItemData itemData)
        {
            return itemData != null && itemData.m_dropPrefab != null && GetItemDataSnapshot(itemData.m_dropPrefab.name) != null;
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

            // The cone main attack has its own projectile, so the damage type projectile only replaces the default one
            if (damageProjectile != null && options.mainAttack == snapshot.mainAttack)
                options.attackProjectile = damageProjectile;

            Jotunn.Logger.LogWarning("Damage (B, P, S): " + options.damageBlunt + ", " + options.damagePierce + ", " + options.damageSlash);
            Jotunn.Logger.LogWarning("Eitr cost: " + options.attackEitr);
            //Jotunn.Logger.LogWarning("ParryBonus: " + options.timedBlockBonus);
            Jotunn.Logger.LogWarning("Accuracy: " + options.projectileAccuracy);
            Jotunn.Logger.LogWarning("Burst: " + options.projectileBurst);
            Jotunn.Logger.LogWarning("Speed: " + options.projectileVelocity);
            Jotunn.Logger.LogWarning("Projectile: " + (options.attackProjectile != null ? options.attackProjectile.name : "default"));
            Jotunn.Logger.LogWarning("Main: " + (options.mainAttack == null ? "null" : options.mainAttack.m_shared?.m_attack?.m_attackProjectile?.name));
            Jotunn.Logger.LogWarning("Secondary: " + (options.secondaryAttack == null ? "null" : options.secondaryAttack.m_shared?.m_attack?.m_attackProjectile?.name));
            Jotunn.Logger.LogWarning("Secondary eitr cost: " + (options.secondaryAttackEitr == null ? "default" : options.secondaryAttackEitr.ToString()));

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

            UpdateHelper.UpdateItemData(itemData, options);
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
