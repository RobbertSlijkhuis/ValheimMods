using ModularMagic_EarthStaffs.Models;
using ModularMagic_EarthStaffs.Types;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using static ItemDrop;

namespace ModularMagic_EarthStaffs.Helpers
{
    internal class ImbuementHelper
    {
        public static void ApplyImbuements(ItemData itemData, string imbuementsString)
        {
            List<Imbuement> imbuements = StringToList(imbuementsString);
            ApplyImbuements(itemData, imbuements);
        }

        public static void ApplyImbuements(ItemData itemData, List<Imbuement> imbuements)
        {
            UpdateItemDataOptions options = new UpdateItemDataOptions();
            ItemData snapShotData = GetItemDataSnapshot(itemData.m_dropPrefab.name);

            if (snapShotData == null)
                throw new Exception("Snapshot ItemData is null");

            options.damageBlunt = snapShotData.m_shared.m_damages.m_blunt;
            options.damageSlash = 0;
            options.damagePierce = 0;
            options.damageBluntPerLevel = snapShotData.m_shared.m_damagesPerLevel.m_blunt;
            options.damageSlashPerLevel = 0;
            options.damagePiercePerLevel = 0;
            options.attackEitr = snapShotData.m_shared.m_attack.m_attackEitr;
            options.maxQuality = snapShotData.m_shared.m_maxQuality;
            options.timedBlockBonus = snapShotData.m_shared.m_timedBlockBonus;
            options.projectileAccuracy = snapShotData.m_shared.m_attack.m_projectileAccuracy;
            options.projectileBurst = snapShotData.m_shared.m_attack.m_burstInterval;
            options.projectileVelocity = snapShotData.m_shared.m_attack.m_projectileVel;

            GameObject projectileBlunt = ModularMagic_EarthStaffs.Instance.prefabs.ProjectileDefault.transform.Find("visual/blunt").gameObject;
            GameObject projectileSlash = ModularMagic_EarthStaffs.Instance.prefabs.ProjectileDefault.transform.Find("visual/slash").gameObject;
            GameObject projectilePierce = ModularMagic_EarthStaffs.Instance.prefabs.ProjectileDefault.transform.Find("visual/pierce").gameObject;
            Projectile projectile = ModularMagic_EarthStaffs.Instance.prefabs.ProjectileDefault.GetComponent<Projectile>();
            projectileBlunt.SetActive(true);
            projectileSlash.SetActive(false);
            projectilePierce.SetActive(false);
            projectile.m_rotateVisual = 300f;
            projectile.m_rotateVisualY = 0f;
            projectile.m_rotateVisualZ = 0f;
            projectile.m_visual = projectileBlunt;

            foreach (Imbuement imbuement in imbuements)
            {
                if (!imbuement.enabled) 
                    continue;

                switch (imbuement.type)
                {
                    case nameof(ImbuementType.DamageType):
                        if (imbuement.value == "Slash")
                        {
                            options.damageSlash = snapShotData.m_shared.m_damages.m_blunt;
                            options.damageSlashPerLevel = snapShotData.m_shared.m_damagesPerLevel.m_blunt;
                            projectileSlash.SetActive(true);
                            projectile.m_rotateVisual = 500f;
                            projectile.m_rotateVisualY = 0f;
                            projectile.m_rotateVisualZ = 10f;
                            projectile.m_visual = projectileSlash;
                        }
                        else if (imbuement.value == "Pierce")
                        {
                            options.damagePierce = snapShotData.m_shared.m_damages.m_blunt;
                            options.damagePiercePerLevel = snapShotData.m_shared.m_damagesPerLevel.m_blunt;
                            projectilePierce.SetActive(true);
                            projectile.m_rotateVisual = 0f;
                            projectile.m_rotateVisualY = 0f;
                            projectile.m_rotateVisualZ = 500f;
                            projectile.m_visual = projectilePierce;
                        }

                        options.damageBlunt = 0;
                        options.damageBluntPerLevel = 0;
                        projectileBlunt.SetActive(false);
                        break;
                    case nameof(ImbuementType.EitrCost):
                        options.attackEitr -= float.Parse(imbuement.value, CultureInfo.InvariantCulture) * imbuement.level;
                        break;
                    case nameof(ImbuementType.MaxQuality):
                        options.maxQuality += int.Parse(imbuement.value, CultureInfo.InvariantCulture);
                        break;
                    case nameof(ImbuementType.ParryBonus):
                        options.timedBlockBonus += float.Parse(imbuement.value, CultureInfo.InvariantCulture);
                        break;
                    case nameof(ImbuementType.ProjectileAccuracy):
                        options.projectileAccuracy -= float.Parse(imbuement.value, CultureInfo.InvariantCulture) * imbuement.level;
                        break;
                    case nameof(ImbuementType.ProjectileBurst):
                        options.projectileBurst -= float.Parse(imbuement.value, CultureInfo.InvariantCulture) * imbuement.level;
                        break;
                    case nameof(ImbuementType.ProjectileVelocity):
                        options.projectileVelocity += float.Parse(imbuement.value, CultureInfo.InvariantCulture) * imbuement.level;
                        break;
                    case nameof(ImbuementType.SecondaryAttack):
                        if (imbuement.value == "Rain")
                            options.secondaryAttack = ModularMagic_EarthStaffs.Instance.prefabs.SecondaryAttackBoulder.GetComponent<ItemDrop>().m_itemData;
                        else if (imbuement.value == "Summon")
                            options.secondaryAttack = ModularMagic_EarthStaffs.Instance.prefabs.SecondaryAttackRoots.GetComponent<ItemDrop>().m_itemData;
                        break;
                }
            }

            Jotunn.Logger.LogWarning("Damage (B, P, S): " + options.damageBlunt + ", " + options.damagePierce + ", " + options.damageSlash);
            Jotunn.Logger.LogWarning("Eitr cost: " + options.attackEitr);
            Jotunn.Logger.LogWarning("ParryBonus: " + options.timedBlockBonus);
            Jotunn.Logger.LogWarning("Accuracy: " + options.projectileAccuracy);
            Jotunn.Logger.LogWarning("Burst: " + options.projectileBurst);
            Jotunn.Logger.LogWarning("Speed: " + options.projectileVelocity);
            Jotunn.Logger.LogWarning("Secondary: " + options.secondaryAttack == null ? "null" : options.secondaryAttack?.m_shared?.m_attack?.m_attackProjectile?.name);

            StatusEffect ImbuementEffect = ScriptableObject.CreateInstance<StatusEffect>();

            Imbuement last = imbuements.Last();
            foreach (Imbuement imbuement in imbuements.FindAll(item => item.enabled))
            {
                ImbuementEffect.m_tooltip += $"<color=green>{imbuement.name} {(imbuement.level > 0 ? imbuement.level : "")} </color>{(imbuement.Equals(last) ? "" : "\n")}";
            }

            ImbuementEffect.name = "Imbuements_MMES";
            ImbuementEffect.m_name = "Imbuements";
            options.equipStatusEffect = ImbuementEffect;

            UpdateHelper.UpdateItemData(itemData, options);
        }

        private static ItemData GetItemDataSnapshot(string name)
        {
            switch (name)
            {
                case var value when value == ModularMagic_EarthStaffs.Instance.prefabs.StaffEarth1.name:
                    return ModularMagic_EarthStaffs.Instance.snapshots.staffEarth1;
                case var value when value == ModularMagic_EarthStaffs.Instance.prefabs.StaffEarth2.name:
                    return ModularMagic_EarthStaffs.Instance.snapshots.staffEarth2;
                case var value when value == ModularMagic_EarthStaffs.Instance.prefabs.StaffEarth3.name:
                    return ModularMagic_EarthStaffs.Instance.snapshots.staffEarth3;
                default:
                    return null;
            }
        }

        public static List<Imbuement> StringToList(string value)
        {
            List<Imbuement> imbuements = new List<Imbuement>();
            string[] data = value.Split(';');

            Jotunn.Logger.LogWarning(data[0]);

            foreach (string item in data)
            {
                string[] properties = item.Split('|');
                imbuements.Add(new Imbuement(
                    properties[0],
                    properties[1],
                    properties[2],
                    properties[3],
                    int.Parse(properties[4]),
                    properties[5],
                    int.Parse(properties[6]),
                    int.Parse(properties[7]),
                    bool.Parse(properties[8])
                ));
            }

            return imbuements;
        }

        public static string ListToString(List<Imbuement> imbuements)
        {
            string items = "";

            foreach (Imbuement i in imbuements)
            {
                items += $"{i.name}|{i.description}|{i.type}|{i.category}|{i.column}|{i.value}|{i.level}|{i.maxLevel}|{i.enabled};";
            }

            if (items != "")
                items = items.Remove(items.Length - 1);

            Jotunn.Logger.LogWarning("=== Earth To String ==============================");
            Jotunn.Logger.LogWarning(items);

            return items;
        }
    }
}
